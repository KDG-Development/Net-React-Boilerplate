import { postRequest, PostRequestMethodArgs } from "kdg-react"
import { Api } from "./_common"
import { TUserLoginForm } from "../types/user/user"

export const appLogin = (args:PostRequestMethodArgs<TUserLoginForm,string>) =>
  postRequest({
    url:Api.BASE_URL + '/auth/login',
    ...args,
  })