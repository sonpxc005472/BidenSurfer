import { httpApi } from '@app/api/http.api';

export interface ApiData {
  id: number;
  userId: number;
  apiKey: string;
  secretKey: string;
  passPhrase: string;
  teleChannel: string;
}

export interface GeneralSettingData {
  id: number;
  userId: number;
  budget: number;
  assetTracking: number;
  stop?: boolean;
}


export const getApiData = (userid?: string): Promise<ApiData> =>
  httpApi.get<ApiData>('user/api-setting').then(({ data }) => data);

export const saveApi = (apiData: ApiData): Promise<boolean> =>
  httpApi.post<boolean>('user/save-api-setting', { ...apiData }).then(({ data }) => data);

export const getGeneralSetting = (): Promise<GeneralSettingData> =>
  httpApi.get<GeneralSettingData>('user/general-setting').then(({ data }) => data);

export const saveGeneralSetting = (data: GeneralSettingData): Promise<boolean> =>
  httpApi.post<boolean>('user/save-general-setting', { ...data }).then(({ data }) => data);

export const startStopBot = (data: GeneralSettingData): Promise<boolean> =>
  httpApi.post<boolean>('user/start-stop-bot', { ...data }).then(({ data }) => data);

export const resetBot = (): Promise<boolean> =>
  httpApi.post<boolean>('user/reset').then(({ data }) => data);
