import { httpApiMock } from '@app/api/mocks/http.api.mock';
import { AuthData } from '@app/api/auth.api';
import { initValues } from '@app/components/auth/LoginForm/LoginForm';

const avatarImg = process.env.REACT_APP_ASSETS_BUCKET + '/avatars/avatar5.webp';

httpApiMock.onPost('login').reply((config) => {
  const data: AuthData = JSON.parse(config.data || '');
  if (data.password === initValues.password) {
    return [
      200,
      {
        token: 'bearerToken',
        user: {
          id: 1,
          fullName: 'Chris Johnson',
          userName: '@john1989',
          email: 'chris.johnson@altence.com',    
          role: 1      
        },
      },
    ];
  } else return [401, { message: 'Invalid Credentials' }];
});

httpApiMock.onPost('signUp').reply(200);

httpApiMock.onPost('forgotPassword').reply(200);

httpApiMock.onPost('verifySecurityCode').reply(200);

httpApiMock.onPost('setNewPassword').reply(200);
