import { TUserAuthBase } from "../types/common/auth"

export const tryParseJWT = (jwt:string):TUserAuthBase => {
  const x = JSON.parse(atob(jwt.split('.')[1]))
  return ({
    id:x.sub,
    jwt,
    user:{
      id:x.sub,
      email:x.email,
      permissionGroups:x.permissionsGroups,
      permissions:x.permissions,
    }
    // id:x.sub,
    // user:{
    //   id:
    // }
    // email:x.email,
    // role:tryParseRoleString(x['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']),
    // agencyId:x.agency_id || null,
    // permissions:[].concat(x.permission).map(tryParsePermissionString),
    // name:x.name,
    // jwt,
  })
}