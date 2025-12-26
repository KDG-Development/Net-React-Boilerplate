import React from 'react'
import ReactDOM from 'react-dom/client'
import './styles/main.scss'
import { AuthContextProvider } from './context/AuthContext.tsx'
import { CartContextProvider } from './context/CartContext.tsx'
import { NotificationProvider } from 'kdg-react'
import { AppRouter } from './routing/AppRouter.tsx'
import '@coreui/coreui-pro/dist/css/coreui.min.css'
import { Healthcheck } from './common/Healthcheck'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthContextProvider>
      <NotificationProvider>
        <CartContextProvider>
          <Healthcheck/>
          <AppRouter/>
        </CartContextProvider>
      </NotificationProvider>
    </AuthContextProvider>
  </React.StrictMode>,
)
