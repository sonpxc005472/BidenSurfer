import { httpApi } from '@app/api/http.api';

export interface ApiData {
  id: string;
  userId: string;
  apiKey: string;
  secretKey: string;
  passPhrase: string;
  teleChannel: string;
}


export const getApiData = (userid?: string): Promise<ApiData> =>
  httpApi.get<ApiData>('user/api-setting').then(({ data }) => data);

export const saveApi = (apiData: ApiData): Promise<boolean> =>
  httpApi.post<boolean>('user/save-api-setting', { ...apiData }).then(({ data }) => data);
