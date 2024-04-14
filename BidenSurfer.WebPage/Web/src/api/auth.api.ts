import { httpApi } from '@app/api/http.api';
import { UserModel } from '@app/domain/UserModel';

export interface AuthData {
  email: string;
  password: string;
}

export interface SignUpRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface ResetPasswordRequest {
  email: string;
}

export interface SecurityCodePayload {
  code: string;
}

export interface NewPasswordData {
  newPassword: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: UserModel;
}

export const login = (loginPayload: LoginRequest): Promise<LoginResponse> =>
  httpApi.post<LoginResponse>('user/login', { ...loginPayload }).then(({ data }) => data);

export const signUp = (signUpData: SignUpRequest): Promise<undefined> =>
  httpApi.post<undefined>('user/signUp', { ...signUpData }).then(({ data }) => data);

export const resetPassword = (resetPasswordPayload: ResetPasswordRequest): Promise<undefined> =>
  httpApi.post<undefined>('user/forgotPassword', { ...resetPasswordPayload }).then(({ data }) => data);

export const verifySecurityCode = (securityCodePayload: SecurityCodePayload): Promise<undefined> =>
  httpApi.post<undefined>('verifySecurityCode', { ...securityCodePayload }).then(({ data }) => data);

export const setNewPassword = (newPasswordData: NewPasswordData): Promise<undefined> =>
  httpApi.post<undefined>('user/setNewPassword', { ...newPasswordData }).then(({ data }) => data);
