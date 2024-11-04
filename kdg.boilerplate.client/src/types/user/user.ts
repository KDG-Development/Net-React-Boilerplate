/**
 * These types are designed to be extended and consumed, rather than modified.
 * e.g. typy MyCustomType = TUserAuth & { customStringProperty:string }
 * This will ensure that shared boilerplate functionalities remain valuable
 */

import { TEntityForm } from "kdg-react"

export type TPermissionGroupMetaBase = string
export type TPermissionMetaBase = string
export type TUserBase = {
  id:string
  email:string
  permissionGroups:TPermissionGroupMetaBase[]
  permissions:TPermissionMetaBase[]
}

export type TUserLoginForm =
  TEntityForm<{
    email:string
    password:string
  }>

export const defaultUserLoginForm:TUserLoginForm = {
  email:null,
  password:null,
}