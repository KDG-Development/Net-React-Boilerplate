-- drop existing objects for re-runnable development
drop table if exists organizations;

-- organizations table
create table organizations (
    id uuid primary key default uuid_generate_v4(),
    name text not null
);

-- add organization_id to users
alter table users add column if not exists organization_id uuid references organizations(id);

