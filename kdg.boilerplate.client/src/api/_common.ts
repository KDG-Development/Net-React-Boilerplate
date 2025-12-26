import { BaseRequestParams, PostRequestMethodArgs, deleteRequest, getRequest, getRequestWithReturn, postFormDataRequest, postRequest, putFormDataRequest, putRequest } from "kdg-react"
import { ROUTE_PATH } from "../routing/AppRouter"
import Storage from "../common/storage"

export namespace Api {
  export const BASE_URL = '/api'
  
  export type TCreateRecord = {
    recordId:string
  }

  type QueryParamValue = string | number | boolean | null | undefined;

  export const composeQueryParams = <T extends Record<string, QueryParamValue>>(
    params: T
  ): Record<string, string> => {
    return Object.entries(params).reduce((acc, [key, value]) => {
      if (value != null) {
        acc[key] = String(value);
      }
      return acc;
    }, {} as Record<string, string>);
  };

  const defaultUnauthorizedHandler = () => {
    Storage.clearAuthToken()
    window.location.href = ROUTE_PATH.LOGIN
  }

  export namespace Request {

    const authenticatedRequest = async <TReturn>(fn:(token:string) => Promise<TReturn>) => {
      const token = Storage.getAuthToken()
      if (token) {
        return await fn(token)
      } else {
        defaultUnauthorizedHandler()
        throw "Unable to complete authenticated request"
      }
    }

    const composedTokenHeader = (token:string) => ({'Authorization':`Bearer ${token}`})

    export const Get = async <TResponse extends {}>(
      params:BaseRequestParams<TResponse>,
    ) => {
      await authenticatedRequest(token =>
        getRequest({
          ...params,
          headers:{...composedTokenHeader(token)},
          errorHandler:e => {
            if ([401].includes(e.status)) {
              defaultUnauthorizedHandler()
            } else if (params.errorHandler) {
              params.errorHandler(e)
            }
          },
        })
      )
    }
    export const GetReturn = async <TResponse extends {}>(
      params:Omit<BaseRequestParams<TResponse>, 'success'>,
    ) => 
      await authenticatedRequest(token =>
        getRequestWithReturn({
          ...params,
          headers:{...composedTokenHeader(token)},
          errorHandler:e => {
            if ([401].includes(e.status)) {
              defaultUnauthorizedHandler()
            } else if (params.errorHandler) {
              params.errorHandler(e)
            }
          },
        })
      )
    

    export const Post = async <TRequest extends {},TResponse extends {}>(
      params:PostRequestMethodArgs<TRequest, TResponse> & BaseRequestParams<TResponse>,
    ) => {
      await authenticatedRequest(token =>
        postRequest({
          ...params,
          headers:{...composedTokenHeader(token)},
          errorHandler:e => {
            if ([401].includes(e.status)) {
              defaultUnauthorizedHandler()
            } else if (params.errorHandler) {
              params.errorHandler(e)
            }
          },
        })
      )
    }

    export const Put = async <TRequest extends {},TResponse extends {}>(
      params:PostRequestMethodArgs<TRequest, TResponse> & BaseRequestParams<TResponse>,
    ) => {
      await authenticatedRequest(token =>
        putRequest({
          ...params,
          headers:{...composedTokenHeader(token)},
          errorHandler:e => {
            if ([401].includes(e.status)) {
              defaultUnauthorizedHandler()
            } else if (params.errorHandler) {
              params.errorHandler(e)
            }
          },
        })
      )
    }
    export const Delete = async <TRequest extends {}>(
      params:BaseRequestParams<TRequest>,
    ) => {
      await authenticatedRequest(token =>
        deleteRequest({
          ...params,
          headers:{...composedTokenHeader(token)},
          errorHandler:e => {
            if ([401].includes(e.status)) {
              defaultUnauthorizedHandler()
            } else if (params.errorHandler) {
              params.errorHandler(e)
            }
          },
        })
      )
    }
    export const PostForm = async <TRequest extends {},TResponse extends {}>(
      params:PostRequestMethodArgs<TRequest, TResponse> & BaseRequestParams<TResponse>,
    ) => {
      await authenticatedRequest(token =>
        postFormDataRequest({
          ...params,
          headers:{...composedTokenHeader(token)},
          errorHandler:e => {
            if ([401].includes(e.status)) {
              defaultUnauthorizedHandler()
            } else if (params.errorHandler) {
              params.errorHandler(e)
            }
          },
        })
      )
    }
    export const PutForm = async <TRequest extends {},TResponse extends {}>(
      params:PostRequestMethodArgs<TRequest, TResponse> & BaseRequestParams<TResponse>,
    ) => {
      await authenticatedRequest(token =>
        putFormDataRequest({
          ...params,
          headers:{...composedTokenHeader(token)},
          errorHandler:e => {
            if ([401].includes(e.status)) {
              defaultUnauthorizedHandler()
            } else if (params.errorHandler) {
              params.errorHandler(e)
            }
          },
        })
      )
    }
  }


}