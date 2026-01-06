import { postRequest, PostRequestMethodArgs } from "kdg-react"
import { Api } from "./_common"
import { TUserLoginForm } from "../types/requests/auth/auth"

export const appLogin = (args:PostRequestMethodArgs<TUserLoginForm,string>) =>
  postRequest({
    url:Api.BASE_URL + '/auth/login',
    ...args,
  })