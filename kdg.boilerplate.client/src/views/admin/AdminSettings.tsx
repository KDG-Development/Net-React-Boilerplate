import { useState } from 'react'
import { Row, Col, Conditional, List } from 'kdg-react'
import { Link } from 'react-router-dom'
import { BaseTemplate } from '../_common/templates/BaseTemplate'
import { ROUTE_PATH } from '../../routing/AppRouter'
import { useUserContext } from '../../context/UserContext'
import { PermissionGroup, TPermissionGroup } from '../../types/common/permissionGroups'
import { HeroBannerSettings } from './HeroBannerSettings'
import { UserManagement } from './UserManagement'

type TAdminPage = {
  key: string
  label: string
  permissionGroup?: TPermissionGroup
  component: () => JSX.Element
}

const adminPages: TAdminPage[] = [
  {
    key: 'hero-banner',
    label: 'Hero Banner',
    permissionGroup: PermissionGroup.SystemAdmin,
    component: HeroBannerSettings,
  },
  {
    key: 'user-management',
    label: 'User Management',
    permissionGroup: PermissionGroup.SystemAdmin,
    component: UserManagement,
  },
]

export const AdminSettings = () => {
  const { hasPermissionGroup } = useUserContext()

  const visiblePages = adminPages.filter(
    (page) => !page.permissionGroup || hasPermissionGroup(page.permissionGroup)
  )

  const [activeKey, setActiveKey] = useState(visiblePages[0]?.key)

  const activePage = visiblePages.find((page) => page.key === activeKey)

  return (
    <BaseTemplate>
      <Row>
        <Col md={12}>
          <nav aria-label="breadcrumb" className="mb-4">
            <ol className="breadcrumb">
              <li className="breadcrumb-item">
                <Link to={ROUTE_PATH.Home}>Home</Link>
              </li>
              <li className="breadcrumb-item active" aria-current="page">
                Admin Settings
              </li>
            </ol>
          </nav>

          <h1 className="mb-4">Admin Settings</h1>
        </Col>
      </Row>

      <Row>
        <Col md={3}>
          <List
            options={visiblePages}
            active={activePage}
            parseKey={(page) => page.key}
            projection={(page) => page.label}
            onClick={(page) => setActiveKey(page.key)}
            flush
          />
        </Col>

        <Col md={9}>
          <Conditional
            condition={!!activePage}
            onTrue={() => {
              const ActiveComponent = activePage!.component
              return <ActiveComponent />
            }}
            onFalse={() => (
              <div className="alert alert-warning">
                You do not have permission to access any admin settings.
              </div>
            )}
          />
        </Col>
      </Row>
    </BaseTemplate>
  )
}

