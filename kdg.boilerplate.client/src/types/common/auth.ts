import { TUserBase } from "../user/user"

/**
 * These types are designed to be extended and consumed, rather than modified.
 * e.g. typy MyCustomType = TUserAuth & { customStringProperty:string }
 * This will ensure that shared boilerplate functionalities remain valuable
 */
export type TUserAuthBase = {
  id:string
  jwt:string
  user:TUserBase
}