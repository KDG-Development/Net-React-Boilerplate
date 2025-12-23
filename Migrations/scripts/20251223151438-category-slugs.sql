-- drop existing objects for re-runnable development
drop view if exists category_slugs;
drop function if exists slugify;

-- slugify function: converts text to URL-friendly slug
-- Example: slugify('Electronics & Gadgets') -> 'electronics-gadgets'
create or replace function slugify(text) returns text as $$
  select trim(both '-' from 
    lower(
      regexp_replace(
        regexp_replace($1, '[^a-zA-Z0-9\s-]', '', 'g'),
        '\s+', '-', 'g'
      )
    )
  )
$$ language sql immutable;

-- category_slugs view: provides full slug paths for each category
-- Uses recursive CTE to build paths from root to each node
create view category_slugs as
with recursive category_paths as (
    -- base case: root categories (no parent)
    select 
        id,
        parent_id,
        name,
        slugify(name) as slug,
        slugify(name) as full_path,
        1 as depth
    from categories
    where parent_id is null
    
    union all
    
    -- recursive case: child categories
    select 
        c.id,
        c.parent_id,
        c.name,
        slugify(c.name) as slug,
        cp.full_path || '/' || slugify(c.name) as full_path,
        cp.depth + 1 as depth
    from categories c
    join category_paths cp on c.parent_id = cp.id
)
select 
    id,
    parent_id,
    name,
    slug,
    full_path,
    depth
from category_paths;

