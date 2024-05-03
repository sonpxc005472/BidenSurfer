import React from 'react';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import * as S from '@app/pages/uiComponentsPages//UIComponentsPage.styles';
import { ConfigurationTable } from '@app/components/tables/ConfigurationTable/ConfigurationTable';

const ConfigurationPage: React.FC = () => {
   
  return (
    <>
      <PageTitle>Configurations</PageTitle>
      <BaseCol>        
        <S.Card title='Configurations'>
          <BaseSpace direction="vertical" style={{width: "100%"}} size={24}>      
            <ConfigurationTable />
          </BaseSpace>
        </S.Card>
      </BaseCol>      
    </>
  );
};

export default ConfigurationPage;
