import { composeAppRoutingProvider, Conditional, Loader, RouteType } from 'kdg-react'
import { useCallback, useEffect, useState } from 'react'
import { useAuthContext } from '../context/AuthContext'
import Storage from '../common/storage'
import { Login } from '../views/login/Login'
import { Home } from '../views/Home/Home'

export enum ROUTE_PATH {
  NOT_FOUND='*',
  LOGIN='/login',
  Home='/',
  Products='/products',
  Favorites='/favorites',
  MyAccount='/my-account',

  // add additional enums as needed
}

export const AppRouter = () => {

  const [loading,setLoading] = useState(true)

  const {user,login} = useAuthContext()

  const defaultGateRender = <Login/>

  const tryLoadAuthFromStorage = useCallback(() => {
    try {
      const token = Storage.getAuthToken()
      if (token) {
        // add your custom jwt parsing functionality
        login({
            id:'example-id',
            jwt:'example-jwt',
            user:{
              id:'example-id',
              email:'example@example.com',
              permissionGroups:['example-permission-group'],
              permissions:['example-permission'],
            }
        })
      }
    } catch (e) {
      console.error('unable to load token from storage', e)
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
          ],
        })
      )}
    />
  )
}
