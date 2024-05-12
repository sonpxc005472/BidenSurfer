import { createAction, createAsyncThunk, createSlice, PrepareAction } from '@reduxjs/toolkit';
import { UserModel } from '@app/domain/UserModel';
import {
  ApiData, GeneralSettingData, saveApi,
  saveGeneralSetting
} from '@app/api/user.api';
import { persistUser, readUser } from '@app/services/localStorage.service';
import { ConfigurationForm, saveConfiguration, saveScanner, saveScannerSetting, ScannerForm, ScannerSettingForm, setConfigActive } from '@app/api/table.api';

export interface UserState {
  user: UserModel | null;
}

const initialState: UserState = {
  user: readUser(),
};

export const setUser = createAction<PrepareAction<UserModel>>('user/setUser', (newUser) => {
  persistUser(newUser);

  return {
    payload: newUser,
  };
});

export const doSaveApi = createAsyncThunk(
  'user/doSaveApi',
  async (saveApiPayload: ApiData) => saveApi(saveApiPayload),
);
export const doSaveGeneralSetting = createAsyncThunk(
  'user/doSaveGeneralSetting',
  async (savePayload: GeneralSettingData) => saveGeneralSetting(savePayload),
);


export const doSaveConfiguration = createAsyncThunk(
  'user/doSaveConfiguration',
  async (savePayload: ConfigurationForm) => saveConfiguration(savePayload),
);

export const doSaveScanner = createAsyncThunk(
  'user/doSaveScanner',
  async (savePayload: ScannerForm) => saveScanner(savePayload),
);

export const doSaveScannerSetting = createAsyncThunk(
  'user/doSaveScannerSetting',
  async (savePayload: ScannerSettingForm) => saveScannerSetting(savePayload),
);


export const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {}
});

export default userSlice.reducer;
