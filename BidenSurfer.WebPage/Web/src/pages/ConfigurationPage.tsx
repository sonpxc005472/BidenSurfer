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
import { ConfigurationTable } from '@app/components/tables/ConfigurationTable/ConfigurationTable';
import { AddConfigurationButton } from '@app/components/tables/ConfigurationTable/Configuration.styles';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { PlusOutlined } from '@ant-design/icons';
import { BaseModal } from '@app/components/common/BaseModal/BaseModal';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { mergeBy } from '@app/utils/utils';
import { SymbolItem } from '@app/components/tables/ConfigurationTable/symbolItem';

interface FieldData {
  name: string | number;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  value?: any;
}
const ConfigurationPage: React.FC = () => {
  const { t } = useTranslation();
  const { isMounted } = useMounted();
  const user = useAppSelector((state) => state.user.user);
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [form] = BaseForm.useForm();
  const [fields, setFields] = useState<FieldData[]>([
    { name: 'symbol', value: '' },
    { name: 'secretKey', value: '' },
    { name: 'passPhrase', value: '' },
    { name: 'teleChannel', value: '' }    
  ]);
  const [tableData, setTableData] = useState<{ [key:string] : ConfigurationTableRow[]}>({
    data: []
  });
  const fetch = useCallback(
    () => {
      getConfigurationData(user?.id).then((res) => {
        if (isMounted.current) {
          const groupedData = res.reduce((group: {[key: string]: ConfigurationTableRow[]}, item) => {
            if (!group[item.symbol]) {
             group[item.symbol] = [];
            }
            group[item.symbol].push(item);
            return group;
           }, {});

          setTableData(groupedData);
        }
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
        <S.Card title='Configurations'>
          <BaseSpace direction="vertical" style={{width: "100%"}} size={24}>            
            <BaseTabs
              size='small'
              tabPosition='top'
              items={Object.entries(tableData).map(([grpId, tlbRows])=>({
                key: `${grpId}`,
                label: `${grpId}`,
                children: <ConfigurationTable configData={tlbRows} />,
              }))}
            />
          </BaseSpace>
        </S.Card>
      </BaseCol>
      <BaseTooltip title='Add configurations'>
        <AddConfigurationButton type="primary" shape="circle" icon={<PlusOutlined />} size="large" onClick={()=> setIsModalOpen(true)}></AddConfigurationButton>
      </BaseTooltip>
      <BaseModal
            title='Add/Edit configurations'
            centered
            open={isModalOpen}
            onOk={() => setIsModalOpen(false)}
            onCancel={() => setIsModalOpen(false)}
            size="large"
          >
            <BaseForm
              name="addForm"
              form={form}
              fields={fields}
              onFieldsChange={(_, allFields) => {
                const currentFields = allFields.map((item) => ({
                  name: Array.isArray(item.name) ? item.name[0] : '',
                  value: item.value,
                }));
                const uniqueData = mergeBy(fields, currentFields, 'name');
                setFields(uniqueData);
              }}
            >

              <BaseRow>                 

                  <BaseCol xs={24} md={24}>
                    <SymbolItem />
                  </BaseCol>
              </BaseRow>
              
            </BaseForm>
      </BaseModal>
    </>
  );
};

export default ConfigurationPage;
