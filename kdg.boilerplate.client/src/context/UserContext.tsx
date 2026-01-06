import { createContext, useContext, useMemo } from "react"
import { TUserBase } from "../types/user/user"
import { useAuthContext } from "./AuthContext"
import { TPermissionGroup } from "../types/common/permissionGroups"

type TUserContext = {
  user: TUserBase
  logout: () => void
  hasPermissionGroup: (group: TPermissionGroup) => boolean
  hasAnyPermissionGroup: (groups: TPermissionGroup[]) => boolean
}

const UserContext = createContext<TUserContext | undefined>(undefined)

type TProviderProps = {
  children: React.ReactNode
}

export const UserContextProvider = (props: TProviderProps) => {
  const auth = useAuthContext()

  if (!auth.user) {
    throw new Error("UserContextProvider rendered without authenticated user - this is a bug")
  }

  const hasPermissionGroup = (group: TPermissionGroup) =>
    auth.user!.user.permissionGroups.includes(group)

  const hasAnyPermissionGroup = (groups: TPermissionGroup[]) =>
    groups.some(hasPermissionGroup)

  const value = useMemo(() => ({
    user: auth.user!.user,
    logout: auth.logout,
    hasPermissionGroup,
    hasAnyPermissionGroup,
  }), [auth.user, auth.logout])

  return (
    <UserContext.Provider value={value}>
      {props.children}
    </UserContext.Provider>
  )
}

export const useUserContext = () => {
  const userContext = useContext(UserContext)
  if (!userContext) {
    throw new Error("useUserContext called outside UserContextProvider - ensure component is within an authenticated route")
  }

  return userContext
}

