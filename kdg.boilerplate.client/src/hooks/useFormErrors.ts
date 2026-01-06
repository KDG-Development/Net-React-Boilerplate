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

type Primitive = string | number | boolean | bigint | symbol
type BuiltIn = Date | File | Blob | FileList | RegExp | Error | Function | Map<unknown, unknown> | Set<unknown> | Promise<unknown>

/**
 * Recursive type that enables type-safe deep path traversal.
 * - Preserves property names for autocomplete on plain objects
 * - Adds numeric indexing for arrays
 * - Returns `unknown` for primitives and built-ins to block further property access
 */
type DeepProxy<T> =
  T extends null | undefined ? T :
  T extends Primitive | BuiltIn ? unknown :
  T extends (infer U)[] ? DeepProxy<U>[] & { [index: number]: DeepProxy<U> } :
  T extends object ? { [K in keyof T]-?: DeepProxy<T[K]> } :
  unknown

export type UseFormErrorsResult<T> = {
  /** Current error state */
  errors: TError<T>
  /** Get error for any field using type-safe selector with deep path support */
  getError: (selector: (obj: DeepProxy<T>) => unknown) => string | undefined
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
 * const { getError, tryParseResponseErrors, clearErrors } = useFormErrors<TCartForm>()
 * 
 * // In API call
 * await createItem({ body: form, errorHandler: tryParseResponseErrors })
 * 
 * // In render - simple field
 * <TextInput error={getError(e => e.buttonText)} />
 * 
 * // In render - array item with nested field
 * items.map((item, i) => (
 *   <NumberInput error={getError(e => e.items[i].quantity)} />
 * ))
 * ```
 */
export function useFormErrors<T extends object>(): UseFormErrorsResult<T> {
  const [errors, setErrors] = useState<TError<T>>({} as TError<T>)

  const getError = useCallback((
    selector: (obj: DeepProxy<T>) => unknown
  ): string | undefined => {
    const path = extractPath(selector)
    return errors[path.toLowerCase() as keyof TError<T>] as string | undefined
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
    tryParseResponseErrors,
    clearErrors
  }), [errors, getError, tryParseResponseErrors, clearErrors])
}

