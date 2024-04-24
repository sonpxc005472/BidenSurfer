import { createAction, createAsyncThunk, createSlice, PrepareAction } from '@reduxjs/toolkit';
import { UserModel } from '@app/domain/UserModel';
import {
  ApiData, saveApi
} from '@app/api/user.api';

export interface UserState {
  user: UserModel | null;
}

const initialState: UserState = {
  user: null,
};

export const doSaveApi = createAsyncThunk(
  'user/doSaveApi',
  async (saveApiPayload: ApiData) => saveApi(saveApiPayload),
);
export const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {}
});

export default userSlice.reducer;
