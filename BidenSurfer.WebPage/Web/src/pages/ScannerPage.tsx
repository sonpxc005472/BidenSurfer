import React from 'react';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import * as S from '@app/pages/uiComponentsPages//UIComponentsPage.styles';
import { ScannerTable } from '@app/components/tables/Scanners/ScannerTable';

const ScannerPage: React.FC = () => {
   
  return (
    <>
      <PageTitle>Scanners</PageTitle>
      <BaseCol>        
        <S.Card title='Scanners'>
          <BaseSpace direction="vertical" style={{width: "100%"}} size={24}>      
            <ScannerTable />
          </BaseSpace>
        </S.Card>
      </BaseCol>      
    </>
  );
};

export default ScannerPage;
