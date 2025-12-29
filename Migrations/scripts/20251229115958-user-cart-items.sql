-- drop existing objects for re-runnable development
drop table if exists user_cart_items;

-- user cart items table
create table user_cart_items (
    user_id uuid not null references users(id) on delete cascade,
    product_id uuid not null references products(id),
    quantity int not null check (quantity > 0),
    primary key (user_id, product_id)
);

create index idx_user_cart_items_user_id on user_cart_items(user_id);

