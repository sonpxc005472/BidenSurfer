import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import * as S from '@app/pages/uiComponentsPages//UIComponentsPage.styles';
import { ConfigurationTable } from '@app/components/tables/ConfigurationTable/ConfigurationTable';
import { AddConfigurationButton } from '@app/components/tables/ConfigurationTable/Configuration.styles';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { PlusOutlined } from '@ant-design/icons';
import { BaseModal } from '@app/components/common/BaseModal/BaseModal';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { SymbolItem } from '@app/components/tables/ConfigurationTable/symbolItem';

const ConfigurationPage: React.FC = () => {
  const { t } = useTranslation();
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [form] = BaseForm.useForm();  
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
