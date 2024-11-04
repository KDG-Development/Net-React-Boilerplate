import { createContext, useContext, useState } from "react"
import { TUserAuthBase } from "../types/common/auth"
import Storage from "../common/storage"

type TAuthContext = {
  user:TUserAuthBase|null
  login:(_:TUserAuthBase)=>void
  logout:()=>void
}

const AuthContext = createContext<TAuthContext|undefined>(undefined)

type TProviderProps = {
  children:React.ReactNode
}
export const AuthContextProvider = ({children}:TProviderProps) => {
  const [user,setUser] = useState<TUserAuthBase|null>(null)

  const login = (auth:TUserAuthBase) => {
    Storage.storeAuthToken(auth.jwt)
    setUser(auth)
  }

  const logout = () => {
    Storage.clearAuthToken()
    setUser(null)
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export const useAuthContext = () => {
  const authContext = useContext(AuthContext)
  if (!authContext) throw new Error("useAuthContext called outside context provider")

  return authContext
}
