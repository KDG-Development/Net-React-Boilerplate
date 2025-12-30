import { createContext, useContext } from "react"
import { TUserBase } from "../types/user/user"
import { useAuthContext } from "./AuthContext"

type TUserContext = {
  user: TUserBase
  logout: () => void
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

  return (
    <UserContext.Provider
      value={{
        user: auth.user.user,
        logout: auth.logout,
      }}
    >
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

