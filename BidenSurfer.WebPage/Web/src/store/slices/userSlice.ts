import { createAction, createAsyncThunk, createSlice, PrepareAction } from '@reduxjs/toolkit';
import { UserModel } from '@app/domain/UserModel';
import {
  ApiData, saveApi
} from '@app/api/user.api';
import { persistUser, readUser } from '@app/services/localStorage.service';

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
export const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {}
});

export default userSlice.reducer;
