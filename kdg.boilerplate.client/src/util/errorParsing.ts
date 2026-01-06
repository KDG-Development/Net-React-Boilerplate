import { TError } from 'kdg-react'

/**
 * ValidationProblemDetails format from ASP.NET:
 * {
 *   "errors": {
 *     "Items.0.Quantity": ["Must be greater than zero"],
 *     "ButtonText": ["Button text is required"]
 *   }
 * }
 */
type ValidationProblemDetails = {
  errors?: Record<string, string[]>
}

/**
 * Parses ValidationProblemDetails JSON body into a TError object.
 * Server returns keys in PascalCase dot notation (e.g., "Items.0.Quantity").
 * Client normalizes to lowercase for matching.
 */
export const parseValidationErrors = <T extends object>(
  body: ValidationProblemDetails
): TError<T> => {
  const result: Record<string, string> = {}
  const errors = body?.errors
  
  if (errors) {
    for (const [key, messages] of Object.entries(errors)) {
      if (messages && messages.length > 0) {
        // Convert PascalCase to lowercase for matching
        result[key.toLowerCase()] = messages[0]
      }
    }
  }
  
  return result as TError<T>
}

