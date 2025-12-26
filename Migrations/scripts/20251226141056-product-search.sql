-- drop existing objects for re-runnable development
drop trigger if exists tsvector_update on products;
drop function if exists products_search_trigger;
drop index if exists idx_products_search;
alter table products drop column if exists search_vector;

-- add tsvector column for pre-computed search index
alter table products add column search_vector tsvector;

-- populate with weighted search (name = A priority, description = B)
update products set search_vector = 
    setweight(to_tsvector('english', coalesce(name, '')), 'A') ||
    setweight(to_tsvector('english', coalesce(description, '')), 'B');

-- create GIN index for fast lookups
create index idx_products_search on products using GIN(search_vector);

-- trigger function to auto-update search_vector on insert/update
create or replace function products_search_trigger() returns trigger as $$
begin
    NEW.search_vector :=
        setweight(to_tsvector('english', coalesce(NEW.name, '')), 'A') ||
        setweight(to_tsvector('english', coalesce(NEW.description, '')), 'B');
    return NEW;
end
$$ language plpgsql;

-- trigger to invoke function on insert/update
create trigger tsvector_update before insert or update on products
for each row execute function products_search_trigger();

