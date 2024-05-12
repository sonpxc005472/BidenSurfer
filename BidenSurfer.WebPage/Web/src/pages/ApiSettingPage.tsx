import React, { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import * as S from '@app/pages/uiComponentsPages//UIComponentsPage.styles';
import { BaseTabs } from '@app/components/common/BaseTabs/BaseTabs';
import { ConfigurationTableRow, getConfigurationData } from 'api/table.api';
import { useMounted } from '@app/hooks/useMounted';
import { useAppSelector } from '@app/hooks/reduxHooks';
import { ApiInfo } from '@app/components/profile/profileCard/profileFormNav/nav/ApiInfo/ApiInfo';

const ApiSettingPage: React.FC = () => {
  const { t } = useTranslation();
  const { isMounted } = useMounted();
  const user = useAppSelector((state) => state.user.user);
  const [tableData, setTableData] = useState<{ [key:string] : ConfigurationTableRow[]}>({
    data: []
  });
  const fetch = useCallback(
    () => {
      // getConfigurationData(user?.id).then((res) => {
      //   if (isMounted.current) {
      //     const groupedData = res.reduce((group: {[key: string]: ConfigurationTableRow[]}, item) => {
      //       if (!group[item.symbol]) {
      //        group[item.symbol] = [];
      //       }
      //       group[item.symbol].push(item);
      //       return group;
      //      }, {});

      //     setTableData(groupedData);
      //   }
      // });
    },
    [isMounted],
  );

  useEffect(() => {
    fetch();
  }, [fetch]);
  
  return (
    <>
      <PageTitle>Api Settings</PageTitle>
      <ApiInfo />
    </>
  );
};

export default ApiSettingPage;
