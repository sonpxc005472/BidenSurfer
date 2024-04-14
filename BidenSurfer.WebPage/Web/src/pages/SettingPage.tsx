import React from 'react';
import { useTranslation } from 'react-i18next';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import * as S from '@app/pages/uiComponentsPages//UIComponentsPage.styles';
import { BaseTabs } from '@app/components/common/BaseTabs/BaseTabs';

const SettingPage: React.FC = () => {
  const { t } = useTranslation();
  return (
    <>
      <PageTitle>Settings</PageTitle>
      <BaseCol>        
        <S.Card title='Settings'>
          <BaseSpace direction="vertical" size={20}>            
            
          </BaseSpace>
        </S.Card>
      </BaseCol>
    </>
  );
};

export default SettingPage;
