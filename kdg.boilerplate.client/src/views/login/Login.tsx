import { Row, Col, AsyncButton, useAppNavigation, TextInput, PasswordInput } from 'kdg-react'
import { useEffect, useState } from 'react'
import { useAuthContext } from '../../context/AuthContext'
import { ROUTE_PATH } from '../../routing/AppRouter'
import { defaultUserLoginForm, TUserLoginForm } from '../../types/user/user'
import { appLogin } from '../../api/auth'
import { tryParseJWT } from '../../util/jwt'

export const Login = () => {

  const [form,setForm] = useState<TUserLoginForm>(defaultUserLoginForm)
  const [loading,setLoading] = useState(false)

  const {login,user} = useAuthContext()
  const navigate = useAppNavigation()

  useEffect(() => {
    if (user){
      navigate(ROUTE_PATH.Home)
    }
  },[user])

  const handleLogin = async () => {
    setLoading(true)
    try {
      // add your custom login functionality here
      await appLogin({
        body:form,
        success:x => login(tryParseJWT(x)),
      })
    } catch (e) {
      console.error('unable to login:', e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    // in case your form isnt wrapped, this will alos submission via the enter key
    const handleEnterKey = (event: KeyboardEvent) => {
      if (event.key === 'Enter') {
        handleLogin()
      }
    }

    window.addEventListener('keydown', handleEnterKey)

    return () => {
      window.removeEventListener('keydown', handleEnterKey)
    }
  }, [handleLogin])

  return (
    <>
    <div className='d-flex min-vh-100 align-items-center'>
      <Row className=''>
        <Col sm={6}>
          <TextInput
            label='Email'
            value={form.email}
            onChange={email => setForm(prev => ({...prev, email}))}
          />
          <PasswordInput
            label='Password'
            value={form.password}
            onChange={password => setForm(prev => ({...prev, password}))}
          /> 
          <AsyncButton
            className='my-4'
            loading={loading}
            onClick={handleLogin}
          >
            Login
          </AsyncButton> 
        </Col>
      </Row>
    </div>
    </>
  )
}