import { getRequest } from "kdg-react";
import { Api } from "./_common";

export const getTest = async () => {
    return await getRequest({
        url: Api.BASE_URL + "/test",
        success: (result) => {
            return result
        }
    });
}