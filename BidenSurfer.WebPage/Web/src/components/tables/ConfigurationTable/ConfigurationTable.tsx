import React, { useEffect, useCallback , useState } from 'react';
import { BaseTable } from '@app/components/common/BaseTable/BaseTable';
import { ColumnsType } from 'antd/es/table';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { useTranslation } from 'react-i18next';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { DeleteOutlined, EditFilled, PlusOutlined } from '@ant-design/icons';
import { BaseSwitch } from '@app/components/common/BaseSwitch/BaseSwitch';
import { ConfigurationForm, ConfigurationTableRow, SymbolData, deleteConfig, getConfigurationData, getMaxBorrow, getSymbolData, setConfigActive } from 'api/table.api';
import { useMounted } from '@app/hooks/useMounted';
import { BaseModal } from '@app/components/common/BaseModal/BaseModal';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { AddConfigurationButton } from './Configuration.styles';
import { BaseSelect } from '@app/components/common/selects/BaseSelect/BaseSelect';
import { InputNumber, Typography } from 'antd';
import { doSaveConfiguration } from '@app/store/slices/userSlice';
import { useAppDispatch } from '@app/hooks/reduxHooks';
import { notificationController } from '@app/controllers/notificationController';
import { BaseButtonsForm } from '@app/components/common/forms/BaseButtonsForm/BaseButtonsForm';

const initialFormValues: ConfigurationForm = {
  id: undefined,
  userId: undefined,
  symbol: undefined,
  positionSide: 'short',
  orderChange: undefined,
  increaseAmountPercent: undefined,
  amount: undefined,
  increaseAmountExpire: undefined,
  amountLimit: undefined,
  expire: undefined,
  increaseOcPercent: 10,
  isActive: true
};

export const ConfigurationTable: React.FC = () => {
  const dispatch = useAppDispatch();

  const [tableData, setTableData] = useState<{ data: ConfigurationTableRow[] }>({
    data: [],
  });
  const [selectOptions, setSelectOptions] = useState<SymbolData[]>([]);
  const [maxBorrow, setMaxBorrow] = useState<number>();

  const { t } = useTranslation();
  const { isMounted } = useMounted();
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [isEditConfig, setIsEditConfig] = useState<boolean>(false);
  const { Text } = Typography;

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
        let cloneData = {...tableData };
        cloneData.data.forEach(function(part, index, theArray) {
          if(theArray[index].id === rowId)
          {
            getMaxBorrow(theArray[index].symbol, theArray[index].positionSide).then((res) => {
              if (isMounted.current) {          
                setMaxBorrow(res);
              }
            });
            form.setFieldsValue({
              ...initialFormValues,
              symbol: theArray[index].symbol,
              positionSide: theArray[index].positionSide,
              amount: theArray[index].amount,
              amountLimit: theArray[index].amountLimit,
              expire: theArray[index].expire,
              increaseAmountExpire: theArray[index].increaseAmountExpire,
              increaseAmountPercent: theArray[index].increaseAmountPercent,
              increaseOcPercent: theArray[index].increaseOcPercent,
              isActive: theArray[index].isActive,
              id: theArray[index].id,
              orderChange: theArray[index].orderChange,
            })
          }
        });        
      }
    else
      {
        getSymbolData().then((res) => {
          if (isMounted.current) {          
            setSelectOptions(res);
          }
        });
        setIsEditConfig(false);
        form.setFieldsValue({
          ...initialFormValues
        })
      }
      setIsModalOpen(true);
  };
    
  const handleDeleteRow = (rowId: number) => {
    deleteConfig(rowId).then((res) => {
      if (res) {          
        setTableData({
          ...tableData,
          data: tableData.data.filter((item) => item.id !== rowId)
        });
      }
      else{
        notificationController.error({ message: "something went wrong!" });        
      }
    }).catch((err) => {
      notificationController.error({ message: err.message });        
    });
    
  };

  const handleActiveRow = (rowId: number, isActive: boolean) => {
    setConfigActive(rowId, isActive).then((res) => {
      if (res) {          
        fetch()
      }
      else{
        notificationController.error({ message: "something went wrong!" });        
      }
    }).catch((err) => {
      notificationController.error({ message: err.message });        
    });
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
              <BaseButton type="default" size='small' icon={<EditFilled />} onClick={() => handleOpenAddEdit(record.id)} />
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

  const onSwitchChange = (checked: boolean) => {
    form.setFieldsValue({ isActive: checked });
  };
  const handleSymbolChange = (value: unknown) => {
    const positionSide = form.getFieldValue('positionSide');
    if(value && positionSide)
      {
        getMaxBorrow(value as string, positionSide).then((res) => {
          if (isMounted.current) {          
            setMaxBorrow(res);
          }
        });
      }
  };

  const handlePositionChange = (value: unknown) => {
    const symbol = form.getFieldValue('symbol');
    if(value && symbol)
      {
        getMaxBorrow(symbol, value as string).then((res) => {
          if (isMounted.current) {          
            setMaxBorrow(res);
          }
        });
      }
  };

  const handleSaveConfig = () => {
    const symbol = form.getFieldValue('symbol');
    const positionSide = form.getFieldValue('positionSide');
    const id = form.getFieldValue('id');
    const orderChange = form.getFieldValue('orderChange');
    const increaseAmountPercent = form.getFieldValue('increaseAmountPercent');
    const amount = form.getFieldValue('amount');
    const amountLimit = form.getFieldValue('amountLimit');
    const increaseAmountExpire = form.getFieldValue('increaseAmountExpire');
    const expire = form.getFieldValue('expire');
    const increaseOcPercent = form.getFieldValue('increaseOcPercent');
    const isActive = form.getFieldValue('isActive');
    const formValues: ConfigurationForm = {
      id: id,
      symbol: symbol,
      customId: '',
      orderType: 1,
      userId: 0,
      positionSide: positionSide,
      orderChange: orderChange,
      increaseAmountPercent: increaseAmountPercent ?? 0,
      amount: amount,
      increaseAmountExpire: increaseAmountExpire ?? 0,
      amountLimit: amountLimit ?? 0,
      expire: expire ?? 0,
      increaseOcPercent: increaseOcPercent ?? 0,
      isActive: isActive
    };
    dispatch(doSaveConfiguration(formValues))
      .unwrap()
      .then(() => {
        fetch();
        setIsModalOpen(false);
      })
      .catch((err) => {
        notificationController.error({ message: err.message });        
      });
  };
  const [isFieldsChanged, setFieldsChanged] = useState(true);
  const onFinish = async (values = {}) => {
    handleSaveConfig();
  };
  
  return (
    <>
      <BaseTable
      columns={columns}
      dataSource={tableData.data}
      rowKey="id"
      pagination={false}
      scroll={{ x: 1000 }}
      />
      <BaseModal
            title={ isEditConfig ? 'Edit configuration': 'Add configuration'}
            centered
            open={isModalOpen}            
            size="large"
            footer={<></>}
          >
            <BaseButtonsForm
              name="editForm"
              form={form}      
              isFieldsChanged={isFieldsChanged}
              initialValues={initialFormValues}    
              footer={
                <>
                  <BaseButtonsForm.Item style={{marginTop: '30px'}}>
                    <BaseButton type="primary" htmlType="submit" style={{float: 'right', width: '100px'}}>
                      Save
                    </BaseButton>
                    <BaseButton type="primary" htmlType="button" style={{float: 'right', width: '100px', marginRight: '20px'}}
                      onClick={() => {              
                        setMaxBorrow(undefined);
                        setIsModalOpen(false);
                      }}>
                      Cancel
                    </BaseButton>
                  </BaseButtonsForm.Item>                
                </>
                
              }    
              onFinish={onFinish}
            >
              <BaseRow gutter={{ xs: 10, md: 15, xl: 20 }}>
                  <BaseCol xs={16} md={12}>
                    <BaseButtonsForm.Item name='symbol' label='Symbol'
                      rules={[{ required: true, message: 'Symbol is required' }]}
                    >
                      <BaseSelect disabled={isEditConfig} onChange={handleSymbolChange} showSearch placeholder='Select symbol' options={selectOptions} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={8} md={12}>
                    <BaseButtonsForm.Item name='positionSide' label='Position'
                      rules={[{ required: true, message: 'Position is required' }]}>
                      <BaseSelect disabled={isEditConfig} onChange={handlePositionChange} placeholder='Select position' options={[{value: 'short'}, {value: 'long'}]} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 10, md: 15, xl: 20 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='orderChange' label='OC'
                      rules={[{ required: true, message: 'OC is required' }]}>
                      <InputNumber min={0.1} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='amount' label='Amount'
                        rules={[{ required: true, message: 'Amount is required' }]}>
                      <InputNumber min={1} addonAfter='$' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 10, md: 15, xl: 20 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='increaseAmountPercent' label='Amount Increase'>
                      <InputNumber min={0} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='amountLimit' label='Limit'>
                      <InputNumber min={1} addonAfter='$' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 10, md: 15, xl: 20 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='increaseAmountExpire' label='Amount Expire'>
                      <InputNumber min={0} addonAfter='min' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='expire' label='Expire'>
                      <InputNumber min={0} addonAfter='min' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow> 
              <BaseRow gutter={{ xs: 10, md: 15, xl: 20 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='increaseOcPercent' label='Auto OC'>
                      <InputNumber min={0} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='isActive' label='Active' valuePropName="checked">
                      <BaseSwitch onChange={onSwitchChange} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              {maxBorrow ? (<BaseRow gutter={{ xs: 10, md: 15, xl: 20 }}>
                  <BaseCol xs={12} md={12}>
                    <Text strong>Max: {maxBorrow} USDT</Text>
                  </BaseCol>                  
              </BaseRow> ) : <></>}
                                          
            </BaseButtonsForm>            
      </BaseModal>
      <BaseTooltip title='Add configurations'>
        <AddConfigurationButton type="primary" shape="circle" icon={<PlusOutlined />} size="large" onClick={()=> handleOpenAddEdit()}></AddConfigurationButton>
      </BaseTooltip>      
    </>    
  );
};
