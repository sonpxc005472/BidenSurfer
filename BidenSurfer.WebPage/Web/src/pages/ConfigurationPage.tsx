import React, { useCallback, useEffect, useState } from 'react';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import * as S from '@app/pages/uiComponentsPages//UIComponentsPage.styles';
import { ConfigurationTable } from '@app/components/tables/ConfigurationTable/ConfigurationTable';
import { useMounted } from '@app/hooks/useMounted';
import { getGeneralSetting } from '@app/api/user.api';

const ConfigurationPage: React.FC = () => {
  const { isMounted } = useMounted();
  const [isStop, setStop] = useState(false);

  const fetch = useCallback(
    () => {
      getGeneralSetting().then((res) => {             
        setStop(res.stop ?? false)
      });      
    },
    [isMounted],
  );

  useEffect(() => {
    fetch();
  }, [fetch]);

  return (
    <>
      <PageTitle>Configurations</PageTitle>
      <BaseCol>        
        <S.Card title={isStop ? <span style={{color: "red"}}>Configurations - Bot stopped!!!</span> : "Configurations"}>
          <BaseSpace direction="vertical" style={{width: "100%"}} size={24}>      
            <ConfigurationTable />
          </BaseSpace>
        </S.Card>
      </BaseCol>      
    </>
  );
};

export default ConfigurationPage;
