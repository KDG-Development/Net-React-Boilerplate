-- drop for re-runnable development
drop table if exists hero_slides;

create table hero_slides (
    id uuid primary key default uuid_generate_v4(),
    image_url text not null,
    button_text text not null,
    button_url text not null,
    sort_order integer not null default 0,
    is_active boolean not null default true
);

create index idx_hero_slides_sort_order on hero_slides (sort_order);
create index idx_hero_slides_active on hero_slides (is_active) where is_active = true;




