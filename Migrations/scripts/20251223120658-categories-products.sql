-- drop existing objects for re-runnable development
drop trigger if exists prevent_category_cycle_trigger on categories;
drop function if exists prevent_category_cycle;
drop table if exists products;
drop table if exists categories;

-- initialize categories table (self-referential hierarchy)
create table categories (
    id uuid primary key default uuid_generate_v4(),
    parent_id uuid references categories (id),
    name text not null,
    description text
);

create index idx_categories_parent_id on categories (parent_id);

-- initialize products table
create table products (
    id uuid primary key default uuid_generate_v4(),
    category_id uuid references categories (id),
    name text not null,
    description text,
    price numeric(10, 2) not null
);

create index idx_products_category_id on products (category_id);

-- prevent circular dependencies in category hierarchy
create or replace function prevent_category_cycle()
returns trigger as $$
begin
    -- allow null parent_id (root categories)
    if new.parent_id is null then
        return new;
    end if;

    -- prevent direct self-reference
    if new.id = new.parent_id then
        raise exception 'category cannot be its own parent';
    end if;

    -- check for cycles by walking up the ancestor chain
    if exists (
        with recursive ancestors as (
            select id, parent_id from categories where id = new.parent_id
            union all
            select c.id, c.parent_id from categories c
            join ancestors a on c.id = a.parent_id
        )
        select 1 from ancestors where id = new.id
    ) then
        raise exception 'circular dependency detected in category hierarchy';
    end if;

    return new;
end;
$$ language plpgsql;

create trigger prevent_category_cycle_trigger
before insert or update on categories
for each row
execute function prevent_category_cycle();

