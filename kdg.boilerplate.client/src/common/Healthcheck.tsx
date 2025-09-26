import { useEffect, useRef } from 'react'
import { getHealth } from '../api/health'
import { DateUtils, useNotification } from 'kdg-react'

type HealthcheckProps = {
  intervalMs?: number
}

const DEFAULT_INTERVAL_MS = 60_000

export const Healthcheck = ({ intervalMs = DEFAULT_INTERVAL_MS }: HealthcheckProps) => {
  const timerRef = useRef<number | null>(null)
  const {error} = useNotification()

  useEffect(() => {
    const run = async () => {
      try {
        await getHealth()
      } catch (e) {
        error({
          key: 'healthcheck' + DateUtils.Instant.now().toString(),
          title: 'Server healthcheck failed',
          message: 'Something is wrong with the server, please check the logs or contact support for assistance.',
        })
      }
    }

    run()
    timerRef.current = window.setInterval(run, intervalMs)
    return () => {
      if (timerRef.current) window.clearInterval(timerRef.current)
    }
  }, [intervalMs])

  return null
}


