import { ConfigurationForm } from '@app/api/table.api';
import { UserModel } from '@app/domain/UserModel';

export const persistToken = (token: string): void => {
  localStorage.setItem('accessToken', token);
};

export const readToken = (): string | null => {
  return localStorage.getItem('accessToken');
};

export const persistUser = (user: UserModel): void => {
  localStorage.setItem('user', JSON.stringify(user));
};

export const readUser = (): UserModel | null => {
  const userStr = localStorage.getItem('user');

  return userStr != null && userStr != undefined && userStr != "undefined" ? JSON.parse(userStr) : null;
};

export const persistRememberConfig = (config: ConfigurationForm): void => {
  localStorage.setItem('rememberConfig', JSON.stringify(config));
};

export const readRememberConfig = (): ConfigurationForm | null => {
  const configStr = localStorage.getItem('rememberConfig');

  return configStr != null && configStr != undefined && configStr != "undefined" ? JSON.parse(configStr) : null;
};

export const deleteToken = (): void => localStorage.removeItem('accessToken');
export const deleteUser = (): void => localStorage.removeItem('user');