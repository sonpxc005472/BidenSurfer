import React, { useEffect, useCallback , useState } from 'react';
import { BaseTable } from '@app/components/common/BaseTable/BaseTable';
import { ColumnsType } from 'antd/es/table';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { useTranslation } from 'react-i18next';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons';
import { BaseRadio } from '@app/components/common/BaseRadio/BaseRadio';
import { BaseSwitch } from '@app/components/common/BaseSwitch/BaseSwitch';
import { ConfigurationTableRow, getConfigurationData } from 'api/table.api';
import { useMounted } from '@app/hooks/useMounted';
import { BaseModal } from '@app/components/common/BaseModal/BaseModal';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { SymbolItem } from './symbolItem';
import { AddConfigurationButton } from './Configuration.styles';
interface AddEditFormValues {
  id?: number;
  symbol?: string;
  positionSide?: string;
  oc?: number;
  autoAmount?: number;
  amount?: number;
  amountLimit?: number;
  amountExpire?: number;
  expire?: number;
  autoOc?: number;
  isActive?: boolean;  
}

const initialFormValues: AddEditFormValues = {
  id: undefined,
  symbol: '',
  positionSide: 'short',
  oc: undefined,
  autoAmount: undefined,
  amount: undefined,
  amountExpire: undefined,
  amountLimit: undefined,
  expire: undefined,
  autoOc: undefined,
  isActive: true
};

export const ConfigurationTable: React.FC = () => {
  const [tableData, setTableData] = useState<{ data: ConfigurationTableRow[] }>({
    data: [],
  });
  const { t } = useTranslation();
  const { isMounted } = useMounted();
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [isEditConfig, setIsEditConfig] = useState<boolean>(false);

  const [form] = BaseForm.useForm();  

  const fetch = useCallback(
    () => {
      getConfigurationData().then((res) => {
        if (isMounted.current) {          
          setTableData({data: res});
        }
      });
    },
    [isMounted],
  );

  useEffect(() => {
    fetch();
  }, [fetch]);
 
  const handleOpenAddEdit = (rowId?: number) => {
    if(rowId)
      {
        setIsEditConfig(true);
        var cloneData = {...tableData };
        cloneData.data.forEach(function(part, index, theArray) {
          if(theArray[index].id === rowId)
          {
            form.setFieldsValue({
              ...initialFormValues,
              symbol: theArray[index].symbol
            })
          }
        });        
      }
    else
      {
        setIsEditConfig(false);
        form.setFieldsValue({
          ...initialFormValues
        })
      }
      setIsModalOpen(true);
  };
    
  const handleDeleteRow = (rowId: number) => {
    setTableData({
      ...tableData,
      data: tableData.data.filter((item) => item.id !== rowId)
    });
  };

  const handleActiveRow = (rowId: number, isActive: boolean) => {
    var cloneData = {...tableData };
    cloneData.data.forEach(function(part, index, theArray) {
      if(theArray[index].id === rowId)
      {
        theArray[index].isActive = isActive
      }
    });
    setTableData(cloneData);
  };

  const columns: ColumnsType<ConfigurationTableRow> = [
    {
      title: t('tables.actions'),
      dataIndex: 'actions',
      width: '10px',
      render: (text: string, record: { id: number; isActive: boolean }) => {
        return (
          <BaseSpace>
            {/* <BaseButton
              type="ghost"
              onClick={() => {
                notificationController.info({ message: t('tables.inviteMessage', { name: record.name }) });
              }}
            >
              {t('tables.invite')}
            </BaseButton> */}
            <BaseTooltip title="edit">
              <BaseButton type="default" size='small' icon={<EditOutlined />} onClick={() => handleOpenAddEdit(record.id)} />
            </BaseTooltip>
            <BaseSwitch size='small' checked={record.isActive} onChange={() => handleActiveRow(record.id, !record.isActive)} />
          </BaseSpace>
        );
      },
    },
    {
      title: 'Symbol',
      dataIndex: 'symbol',
      width: '10px'
    },
    {
      title: 'Position',
      dataIndex: 'positionSide',
      width: '10px',
    }, 
    {
      title: 'Amount',
      dataIndex: 'amount'
    },
    {
      title: 'OC',
      dataIndex: 'orderChange',
    },
    {
      title: 'Auto Amount',
      dataIndex: 'increaseAmountPercent',
    },
    {
      title: 'Amount Expire',
      dataIndex: 'increaseAmountExpire',
    },
    {
      title: 'Amount Limit',
      dataIndex: 'amountLimit',
    },  
    {
      title: 'Expire',
      dataIndex: 'expire',
    },
    {
      title: '',
      dataIndex: 'delete',
      width: '5%',
      render: (text: string, record: { id: number }) => {
        return (
          <BaseSpace>            
            <BaseTooltip title="Delete">
              <BaseButton type="primary" shape="circle" icon={<DeleteOutlined />} size="small" onClick={() => handleDeleteRow(record.id)}/>
            </BaseTooltip>
          </BaseSpace>
        );
      },
    }
  ];

  return (
    <>
      <BaseTable
      columns={columns}
      dataSource={tableData.data}
      rowKey="id"
      pagination={false}
      scroll={{ x: 800 }}
      />
      <BaseModal
            title={ isEditConfig ? 'Edit configuration': 'Add configuration'}
            centered
            open={isModalOpen}
            onOk={() => setIsModalOpen(false)}
            onCancel={() => setIsModalOpen(false)}
            size="large"
          >
            <BaseForm
              name="editForm"
              form={form}      
              initialValues={initialFormValues}        
            >
              <BaseRow>
                  <BaseCol xs={24} md={24}>
                    <SymbolItem />
                  </BaseCol>
              </BaseRow>              
            </BaseForm>
      </BaseModal>
      <BaseTooltip title='Add configurations'>
        <AddConfigurationButton type="primary" shape="circle" icon={<PlusOutlined />} size="large" onClick={()=> handleOpenAddEdit()}></AddConfigurationButton>
      </BaseTooltip>      
    </>    
  );
};
