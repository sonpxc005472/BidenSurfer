import React, {  } from 'react';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { ApiInfo } from '@app/components/profile/profileCard/profileFormNav/nav/ApiInfo/ApiInfo';

const ApiSettingPage: React.FC = () => {  
  
  return (
    <>
      <PageTitle>Api Settings</PageTitle>
      <ApiInfo />
    </>
  );
};

export default ApiSettingPage;
