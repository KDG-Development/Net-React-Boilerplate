import { TEntityForm } from "kdg-react";

export type TUserLoginForm = TEntityForm<{
  email: string;
  password: string;
}>;

export const defaultUserLoginForm: TUserLoginForm = {
  email: null,
  password: null,
};

