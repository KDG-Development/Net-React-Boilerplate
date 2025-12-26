-- drop for re-runnable development
drop table if exists product_images;

create table product_images (
    id uuid primary key default uuid_generate_v4(),
    product_id uuid not null references products (id) on delete cascade,
    src text not null,
    sort_order integer not null default 0,
    unique (product_id, sort_order)
);

create index idx_product_images_product_id on product_images (product_id);

