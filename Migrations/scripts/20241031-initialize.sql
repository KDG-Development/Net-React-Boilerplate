-- initialize extensions
create extension if not exists "uuid-ossp";
create extension if not exists "citext";
create extension if not exists "btree_gist";
create extension if not exists pgcrypto;

-- initialize users table
create table if not exists users (
    id uuid primary key default uuid_generate_v4(),
    email citext not null unique check (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);

-- initialize user passwords
create table if not exists user_passwords (
    id uuid primary key default uuid_generate_v4(),
    user_id uuid not null references users (id),
    password_hash varchar not null,
    salt varchar not null,
    created_at timestamptz not null default now(),
    deactivated timestamp,

    constraint ex_user_passwords_single_active_password exclude using gist (
        user_id with =
    ) where (
        deactivated is null
    )
);

-- initialize login attempt tracking
create table if not exists login_attempts (
    id uuid primary key default uuid_generate_v4(),
    user_id uuid not null references users (id),
    successful boolean not null,
    ip_address inet,
    attempted_at timestamptz not null default now()
);

create index if not exists idx_login_attempts_user_id on login_attempts (user_id);
create index if not exists idx_login_attempts_ip_address on login_attempts (ip_address);

-- prevent modifying password values after creation
create or replace function prevent_password_modification()
returns trigger as $$
begin
    if old.password_hash != new.password_hash or old.salt != new.salt then
        raise exception 'password_hash and salt cannot be modified';
    end if;
    return new;
end;
$$ language plpgsql;

drop trigger if exists prevent_password_modification_trigger on user_passwords;
create trigger prevent_password_modification_trigger
before update on user_passwords
for each row
execute function prevent_password_modification();

-- add built-in functionality for validating passwords
drop function if exists validate_password;
create or replace function validate_password(user_uuid uuid, password_attempt text)
returns boolean as $$
begin
    return exists (
        select 1 from user_passwords up
        where true
            and up.user_id = user_uuid
            and up.deactivated is null
        and up.password_hash = crypt(password_attempt, salt)
    );
end;
$$ language plpgsql security definer;

-- initialize permission related tables
create table if not exists permissions (
    permission citext primary key,
    display varchar,
    description varchar,
    created_at timestamptz not null default now()
);

create table if not exists permission_groups (
    permission_group citext primary key,
    display varchar,
    description varchar,
    created_at timestamptz not null default now()
);

create table if not exists permission_group_permissions (
    permission_group citext not null references permission_groups,
    permission citext not null references permissions,
    created_at timestamptz not null default now()
);

-- initialize user permission relationship tables
create table if not exists user_permissions (
    user_id uuid not null references users (id),
    permission citext not null references permissions (permission),
    granted_at timestamptz not null default now(),
    granted_by uuid references users (id),
    constraint PK_user_permissions primary key (user_id,permission)
);

create table if not exists user_permission_groups (
    user_id uuid not null references users (id),
    permission_group citext not null references permission_groups (permission_group),
    granted_at timestamptz not null default now(),
    granted_by uuid references users (id),
    constraint PK_references primary key (user_id,permission_group)
);