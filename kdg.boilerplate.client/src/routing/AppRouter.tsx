import { composeAppRoutingProvider, Conditional, Loader, RouteType } from 'kdg-react'
import { useCallback, useEffect, useState } from 'react'
import { useAuthContext } from '../context/AuthContext'
import Storage from '../common/storage'
import { Login } from '../views/login/Login'
import { Home } from '../views/Home/Home'
import { CategoryPage } from '../views/category/CategoryPage'
import { tryParseJWT, isTokenExpired } from '../util/jwt'

export const ROUTE_BASE = {
  Products: '/products',
} as const;

export const ROUTE_PATH = {
  NOT_FOUND: '*',
  LOGIN: '/login',
  Home: '/',
  Products: `${ROUTE_BASE.Products}/*`,
  Favorites: '/favorites',
  MyAccount: '/my-account',
} as const;

export const AppRouter = () => {

  const [loading,setLoading] = useState(true)

  const {user,login} = useAuthContext()

  const defaultGateRender = <Login/>

  const tryLoadAuthFromStorage = useCallback(() => {
    try {
      const token = Storage.getAuthToken()
      if (token) {
        if (isTokenExpired(token)) {
          Storage.clearAuthToken()
        } else {
          login(tryParseJWT(token))
        }
      }
    } catch (e) {
      console.error('unable to load token from storage', e)
      Storage.clearAuthToken()
    } finally {
      setLoading(false)
    }
  },[])

  useEffect(() => {
    tryLoadAuthFromStorage()
  },[tryLoadAuthFromStorage])

  return (
    <Conditional
      condition={loading}
      onTrue={() => <Loader/>}
      onFalse={() => (
        composeAppRoutingProvider({
          routes:[
            {
              kind:RouteType.PUBLIC,
              paths:[ROUTE_PATH.NOT_FOUND],
              render:<>404 Not Found</>,
            },
            {
              kind:RouteType.PUBLIC,
              paths:[ROUTE_PATH.LOGIN],
              render:<Login/>,
            },
            // add your custom routes below
            {
              kind:RouteType.PRIVATE,
              paths:[ROUTE_PATH.Home],
              gate:{
                allow:!!user,
                onNotAllow:{
                  render:defaultGateRender,
                }
              },
              render:<Home/>,
            },
            {
              kind:RouteType.PRIVATE,
              paths:[ROUTE_PATH.Products],
              gate:{
                allow:!!user,
                onNotAllow:{
                  render:defaultGateRender,
                }
              },
              render:<CategoryPage/>,
            },
          ],
        })
      )}
    />
  )
}
