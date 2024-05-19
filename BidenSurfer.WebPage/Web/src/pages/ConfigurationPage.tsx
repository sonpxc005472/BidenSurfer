import React from 'react';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import * as S from '@app/pages/uiComponentsPages//UIComponentsPage.styles';
import { ConfigurationTable } from '@app/components/tables/ConfigurationTable/ConfigurationTable';
import { useAppSelector } from '@app/hooks/reduxHooks';

const ConfigurationPage: React.FC = () => {
  const isBotStopped = useAppSelector((state) => state.user.isBotStopped);
 
  return (
    <>
      <PageTitle>Configurations</PageTitle>
      <BaseCol>        
        <S.Card title={isBotStopped ? <span style={{color: "red"}}>Configurations - Bot stopped!!!</span> : "Configurations"}>
          <BaseSpace direction="vertical" style={{width: "100%"}} size={24}>      
            <ConfigurationTable />
          </BaseSpace>
        </S.Card>
      </BaseCol>      
    </>
  );
};

export default ConfigurationPage;
