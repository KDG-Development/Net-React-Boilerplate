import React from 'react'
import ReactDOM from 'react-dom/client'
import './index.css'
import { AuthContextProvider } from './context/AuthContext.tsx'
import { NotificationProvider } from 'kdg-react'
import { AppRouter } from './routing/AppRouter.tsx'
import '@coreui/coreui-pro/dist/css/coreui.min.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthContextProvider>
      <NotificationProvider>
        <AppRouter/>
      </NotificationProvider>
    </AuthContextProvider>
  </React.StrictMode>,
)
