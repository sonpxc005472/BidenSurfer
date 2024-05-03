import React, { useEffect, useCallback , useState } from 'react';
import { BaseTable } from '@app/components/common/BaseTable/BaseTable';
import { ColumnsType } from 'antd/es/table';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { useTranslation } from 'react-i18next';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { DeleteOutlined, EditFilled, EditOutlined, PlusOutlined } from '@ant-design/icons';
import { BaseSwitch } from '@app/components/common/BaseSwitch/BaseSwitch';
import { ScannerTableRow, getScannerData } from 'api/table.api';
import { useMounted } from '@app/hooks/useMounted';
import { BaseModal } from '@app/components/common/BaseModal/BaseModal';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { SymbolItem } from './symbolItem';
import { AddConfigurationButton } from './Scanner.styles';
import { BaseTag } from '@app/components/common/BaseTag/BaseTag';
interface AddEditFormValues {
  id?: number;
  title?: string;
  positionSide?: string;
  oc?: number;
  elastic?: number;
  turnover?: number;
  numberOc?: number;
  amount?: number;
  limit?: number;
  expire?: number;
  onlyPair?: string[];
  isActive?: boolean;  
}

const initialFormValues: AddEditFormValues = {
  id: undefined,
  title: '',
  positionSide: 'short',
  oc: undefined,
  elastic: undefined,
  amount: undefined,
  turnover: undefined,
  numberOc: undefined,
  expire: undefined,
  limit: undefined,
  onlyPair: [],
  isActive: true
};

export const ScannerTable: React.FC = () => {
  const [tableData, setTableData] = useState<{ data: ScannerTableRow[] }>({
    data: [],
  });
  const { t } = useTranslation();
  const { isMounted } = useMounted();
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [isEditConfig, setIsEditConfig] = useState<boolean>(false);

  const [form] = BaseForm.useForm();  

  const fetch = useCallback(
    () => {
      getScannerData().then((res) => {
        if (isMounted.current) {     
          debugger     
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
              title: theArray[index].title
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

  const columns: ColumnsType<ScannerTableRow> = [
    {
      title: t('tables.actions'),
      dataIndex: 'actions',
      width: '10px',
      render: (text: string, record: { id: number; isActive: boolean }) => {
        return (
          <BaseSpace>            
            <BaseTooltip title="edit">
              <BaseButton type="default" size='small' icon={<EditFilled />} onClick={() => handleOpenAddEdit(record.id)} />
            </BaseTooltip>
            <BaseSwitch size='small' checked={record.isActive} onChange={() => handleActiveRow(record.id, !record.isActive)} />
          </BaseSpace>
        );
      },
    },
    {
      title: 'Name',
      dataIndex: 'title',
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
      title: 'Elastic',
      dataIndex: 'elastic',
    },
    {
      title: 'Turnover',
      dataIndex: 'turnover',
    },
    {
      title: 'Numbs',
      dataIndex: 'ocNumber',
    },  
    {
      title: 'Limit',
      dataIndex: 'amountLimit',
    },
    {
      title: 'Expire',
      dataIndex: 'configExpire',
    },
    {
      title: 'Only Pairs',
      key: 'onlyPairs',
      dataIndex: 'onlyPairs',
      render: (onlyPairs: string[]) => onlyPairs ? (
        <BaseRow gutter={[5, 5]}>
          {onlyPairs.map((tag: string) => (
            <BaseTag color="green" key={tag}>
              {tag.toUpperCase()}
            </BaseTag>
          ))}
        </BaseRow>
      ) : <></>,
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
            title={ isEditConfig ? 'Edit Scanner': 'Add Scanner'}
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
      <BaseTooltip title='Add Scanner'>
        <AddConfigurationButton type="primary" shape="circle" icon={<PlusOutlined />} size="large" onClick={()=> handleOpenAddEdit()}></AddConfigurationButton>
      </BaseTooltip>      
    </>    
  );
};
