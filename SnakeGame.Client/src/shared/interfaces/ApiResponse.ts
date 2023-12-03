export interface ApiError {
  propertyName: string;
  errorCode: string;
  errorMessage: string;
}

export type ApiErrorResponse = ApiError | ApiError[];

export interface ApiResponse {
    apiData: any;
    apiError: ApiErrorResponse;
    httpStatusCode: number;
}
