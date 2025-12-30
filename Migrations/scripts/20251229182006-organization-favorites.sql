-- drop existing objects for re-runnable development
drop table if exists organization_favorites;

-- organization favorites table
create table organization_favorites (
    organization_id uuid not null references organizations(id) on delete cascade,
    product_id uuid not null references products(id) on delete cascade,
    primary key (organization_id, product_id)
);

create index idx_organization_favorites_org_id on organization_favorites(organization_id);

