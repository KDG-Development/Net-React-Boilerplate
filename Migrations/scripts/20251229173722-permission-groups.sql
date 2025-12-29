-- seed permission groups for user role assignment
insert into permission_groups (permission_group, display, description)
values
    ('system-admin', 'System Administrator', 'Full system access'),
    ('customer-admin', 'Customer Administrator', 'Customer organization administrator'),
    ('customer-user', 'Customer User', 'Standard customer user')
on conflict (permission_group) do update set
    display = excluded.display,
    description = excluded.description;

