export const PermissionGroup = {
  SystemAdmin: 'system-admin',
  CustomerAdmin: 'customer-admin',
  CustomerUser: 'customer-user',
} as const;

export type TPermissionGroup = typeof PermissionGroup[keyof typeof PermissionGroup];

