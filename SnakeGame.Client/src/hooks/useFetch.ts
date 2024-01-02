import { API_PREFIX } from "../shared/constants/Constants";
import { ApiErrorResponse, ApiResponse } from "../shared/interfaces";

export async function useFetch(
  endpoint: string,
  options: RequestInit,
  body?: any,
): Promise<ApiResponse> {
  let apiData: any = null;
  let apiError: ApiErrorResponse | null = null;
  let httpStatusCode: number = 0;

  await fetch(`${API_PREFIX}${endpoint}`, {
    ...options,
    headers: {
      ...options.headers,
      "Content-Type": "application/json",
    },
    body: body ? JSON.stringify(body) : body,
  })
    .then(async (res) => {
      let data;
      const contentType = res.headers.get("content-type");
      if (contentType && contentType.indexOf("application/json") !== -1) {
        data = await res.json();
      } else {
        data = await res.text();
      }
      httpStatusCode = res.status;

      if (!res.ok) {
        return Promise.reject(data);
      }

      return data;
    })
    .then((data) => {
      apiData = data;
      return data;
    })
    .catch((e) => {
      console.error("useFetch error: ", e);
      apiError = e;
      return e;
    });

  return { apiData, apiError, httpStatusCode };
}
