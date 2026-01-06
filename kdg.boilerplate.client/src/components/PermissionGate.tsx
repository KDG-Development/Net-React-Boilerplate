import { Conditional } from 'kdg-react'
import { useUserContext } from '../context/UserContext'
import { TPermissionGroup } from '../types/common/permissionGroups'

type TPermissionGateProps = {
  group: TPermissionGroup
  onTrue: () => JSX.Element
  onFalse?: () => JSX.Element
}

export const PermissionGate = (props: TPermissionGateProps) => {
  const { hasPermissionGroup } = useUserContext()

  return (
    <Conditional
      condition={hasPermissionGroup(props.group)}
      onTrue={props.onTrue}
      onFalse={props.onFalse}
    />
  )
}

