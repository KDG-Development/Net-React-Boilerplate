import { TOrganizationMeta } from "../organization/organization";

export type TPermissionGroupMetaBase = string;
export type TPermissionMetaBase = string;
export type TUserBase = {
  id: string;
  email: string;
  organization: TOrganizationMeta | null;
  permissionGroups: TPermissionGroupMetaBase[];
  permissions: TPermissionMetaBase[];
};

