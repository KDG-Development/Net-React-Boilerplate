import { useState, useCallback, useMemo } from 'react'
import { TError } from 'kdg-react'
import { parseValidationErrors } from '../util/errorParsing'

/**
 * Extracts a property path from a selector callback using a Proxy.
 * Example: extractPath<T>(x => x.items) returns "items"
 * Example: extractPath<T>(x => x.address.street) returns "address.street"
 */
function extractPath<T>(selector: (obj: T) => unknown): string {
  const path: string[] = []
  
  const handler: ProxyHandler<object> = {
    get(_, prop) {
      path.push(String(prop))
      return new Proxy({}, handler)
    }
  }
  
  const proxy = new Proxy({}, handler) as T
  selector(proxy)
  return path.join('.')
}

export type UseFormErrorsResult<T> = {
  /** Current error state */
  errors: TError<T>
  /** Get error for a simple field using type-safe selector */
  getError: <K extends keyof T>(selector: (obj: T) => T[K]) => string | undefined
  /** Get error for an array item field using type-safe selectors */
  getArrayItemError: <
    K extends keyof T,
    TItem = T[K] extends (infer U)[] ? U : never
  >(
    arraySelector: (obj: T) => T[K],
    index: number,
    itemSelector?: (item: TItem) => unknown
  ) => string | undefined
  /** Parse validation errors from a Response - pass directly to errorHandler */
  tryParseResponseErrors: (response: Response) => Promise<void>
  /** Clear all errors */
  clearErrors: () => void
}

/**
 * Hook for managing form validation errors with type-safe accessors.
 * 
 * @example
 * ```tsx
 * const { getError, getArrayItemError, tryParseResponseErrors, clearErrors } = useFormErrors<TCartForm>()
 * 
 * // In API call
 * await createItem({ body: form, errorHandler: tryParseResponseErrors })
 * 
 * // In render - simple field
 * <TextInput error={getError(e => e.buttonText)} />
 * 
 * // In render - array item
 * items.map((item, i) => (
 *   <NumberInput error={getArrayItemError(e => e.items, i, item => item.quantity)} />
 * ))
 * ```
 */
export function useFormErrors<T extends object>(): UseFormErrorsResult<T> {
  const [errors, setErrors] = useState<TError<T>>({} as TError<T>)

  const getError = useCallback(<K extends keyof T>(
    selector: (obj: T) => T[K]
  ): string | undefined => {
    const path = extractPath(selector)
    return errors[path.toLowerCase() as keyof TError<T>] as string | undefined
  }, [errors])

  const getArrayItemError = useCallback(<
    K extends keyof T,
    TItem = T[K] extends (infer U)[] ? U : never
  >(
    arraySelector: (obj: T) => T[K],
    index: number,
    itemSelector?: (item: TItem) => unknown
  ): string | undefined => {
    const arrayPath = extractPath(arraySelector)
    const itemPath = itemSelector ? extractPath(itemSelector as (obj: TItem) => unknown) : ''
    
    const fullPath = itemPath
      ? `${arrayPath}.${index}.${itemPath}`
      : `${arrayPath}.${index}`
    
    return errors[fullPath.toLowerCase() as keyof TError<T>] as string | undefined
  }, [errors])

  const tryParseResponseErrors = useCallback(async (response: Response): Promise<void> => {
    if (!response.ok) {
      try {
        const body = await response.clone().json()
        const parsedErrors = parseValidationErrors<T>(body)
        setErrors(parsedErrors)
      } catch {
        // Response wasn't JSON or parsing failed - clear errors
        setErrors({} as TError<T>)
      }
    }
  }, [])

  const clearErrors = useCallback(() => {
    setErrors({} as TError<T>)
  }, [])

  return useMemo(() => ({
    errors,
    getError,
    getArrayItemError,
    tryParseResponseErrors,
    clearErrors
  }), [errors, getError, getArrayItemError, tryParseResponseErrors, clearErrors])
}

