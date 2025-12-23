import { TUserAuthBase } from "../types/common/auth"

export const tryParseJWT = (jwt:string):TUserAuthBase => {
  const payload = JSON.parse(atob(jwt.split('.')[1]))
  // The server stores user data as a JSON string in the 'user' claim
  const user = JSON.parse(payload.user)
  return ({
    id: user.Id,
    jwt,
    user: {
      id: user.Id,
      email: user.Email,
      permissionGroups: (user.PermissionGroups ?? []).map((pg: { PermissionGroup: string }) => pg.PermissionGroup),
      permissions: (user.Permissions ?? []).map((p: { Permission: string }) => p.Permission),
    }
  })
}

export const isTokenExpired = (jwt: string): boolean => {
  try {
    const payload = JSON.parse(atob(jwt.split('.')[1]))
    const expiration = payload.exp * 1000 // Convert to milliseconds
    return Date.now() >= expiration
  } catch {
    return true
  }
}