import { getRequestWithReturn } from 'kdg-react'
import { Api } from './_common'

export const getHealth = async () =>
  await getRequestWithReturn({
    url: Api.BASE_URL + '/health',
  })


