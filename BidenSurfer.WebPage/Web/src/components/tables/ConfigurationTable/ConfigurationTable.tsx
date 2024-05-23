import React, { useEffect, useCallback , useState } from 'react';
import { BaseTable } from '@app/components/common/BaseTable/BaseTable';
import { ColumnsType } from 'antd/es/table';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { useTranslation } from 'react-i18next';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { DeleteOutlined, EditFilled, PlusOutlined, ScanOutlined, StarFilled, StarOutlined } from '@ant-design/icons';
import { BaseSwitch } from '@app/components/common/BaseSwitch/BaseSwitch';
import formatNumber, { ConfigurationForm, ConfigurationTableRow, SymbolData, deleteConfig, getConfigurationData, getMaxBorrow, getSymbolData, setConfigActive } from 'api/table.api';
import { useMounted } from '@app/hooks/useMounted';
import { BaseModal } from '@app/components/common/BaseModal/BaseModal';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { AddConfigurationButton } from './Configuration.styles';
import { BaseSelect } from '@app/components/common/selects/BaseSelect/BaseSelect';
import { Badge, InputNumber, Typography } from 'antd';
import { doSaveConfiguration } from '@app/store/slices/userSlice';
import { useAppDispatch } from '@app/hooks/reduxHooks';
import { notificationController } from '@app/controllers/notificationController';
import { BaseButtonsForm } from '@app/components/common/forms/BaseButtonsForm/BaseButtonsForm';
import { getGeneralSetting } from '@app/api/user.api';

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
      title: () => (<div>Symbol ({tableData.data.filter((item) => item.isActive).length}/{tableData.data.length})</div>),
      dataIndex: 'symbol',
      width: '20%',
      render: (text: string, record: { id: number; isActive: boolean }) => {
        return (
          <BaseSpace>            
            <BaseTooltip title="edit">
              <BaseButton type="default" size='small' icon={<EditFilled />} onClick={() => handleOpenAddEdit(record.id)} />
            </BaseTooltip>
            <BaseSwitch size='small' checked={record.isActive} onChange={() => handleActiveRow(record.id, !record.isActive)} />
            {text}
          </BaseSpace>
        );
      },
    },    
    {
      title: 'Position',
      dataIndex: 'positionSide',
      width: '10%',
      render: (text: string, record: { createdBy: string }) => {
        return (
          <BaseSpace>
            {
              record.createdBy === 'scanner' ? 
              (<Badge count={<StarFilled style={{ color: 'red' }} />}>
                {<div style={{paddingRight: "10px"}}>{text}</div>}
              </Badge>) : (<div>{text}</div>)
            }            
          </BaseSpace>
        );
      }
    }, 
    {
      title: 'Amount',
      dataIndex: 'amount',
      width: '10%',
      render: (text: string, record: { amount: number }) => {
        return (
          formatNumber(record.amount, 2)
        );
      }
    },
    {
      title: 'OC',
      dataIndex: 'orderChange',
      width: '5%',
    },
    {
      title: 'Auto Amount',
      dataIndex: 'increaseAmountPercent',
      width: '5%',
    },
    {
      title: 'Amount Expire',
      dataIndex: 'increaseAmountExpire',
      width: '5%',
    },
    {
      title: 'Amount Limit',
      dataIndex: 'amountLimit',
      width: '10%',
      render: (text: string, record: { amountLimit: number }) => {
        return (
          formatNumber(record.amountLimit, 2)
        );
      }
    },  
    {
      title: 'Expire',
      dataIndex: 'expire',
      width: '5%',
    },
    {
      title: '',
      dataIndex: 'delete',
      render: (text: string, record: { id: number, isActive: boolean }) => {
        return (
          <BaseSpace>            
            <BaseTooltip title="Delete">
              <BaseButton type="primary" disabled={record.isActive} shape="circle" icon={<DeleteOutlined />} size="small" onClick={() => handleDeleteRow(record.id)}/>
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
            style={{ fontSize: "12px" }}
            footer={<></>}
            onCancel={()=>setIsModalOpen(false)}
          >
            <BaseButtonsForm
              name="editForm"
              form={form}      
              isFieldsChanged={isFieldsChanged}
              initialValues={initialFormValues}    
              footer={
                <>
                  <BaseButtonsForm.Item style={{marginTop: '30px'}}>
                    <BaseButton type="primary" htmlType="submit" style={{float: 'right', width: '100px', height: '40px'}}>
                      Save
                    </BaseButton>
                    <BaseButton type="primary" htmlType="button" style={{float: 'right', width: '100px', height: '40px', marginRight: '20px'}}
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
                    <BaseButtonsForm.Item name='symbol' label='Symbol' style={{marginBottom: '5px'}}
                      rules={[{ required: true, message: 'Symbol is required' }]}
                    >
                      <BaseSelect disabled={isEditConfig} onChange={handleSymbolChange} showSearch placeholder='Select symbol' options={selectOptions} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={8} md={12}>
                    <BaseButtonsForm.Item name='positionSide' label='Position' style={{marginBottom: '5px'}}
                      rules={[{ required: true, message: 'Position is required' }]}>
                      <BaseSelect disabled={isEditConfig} onChange={handlePositionChange} placeholder='Select position' options={[{value: 'short'}, {value: 'long'}]} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='orderChange' label='OC' style={{marginBottom: '5px'}}
                      rules={[{ required: true, message: 'OC is required' }]}>
                      <InputNumber min={0.1} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='amount' label='Amount' style={{marginBottom: '5px'}}
                        rules={[{ required: true, message: 'Amount is required' }]}>
                      <InputNumber min={1} addonAfter='$' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='increaseAmountPercent' label='Amount Increase' style={{marginBottom: '5px'}}>
                      <InputNumber min={0} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='amountLimit' label='Limit' style={{marginBottom: '5px'}}>
                      <InputNumber min={1} addonAfter='$' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='increaseAmountExpire' label='Amount Expire' style={{marginBottom: '5px'}}>
                      <InputNumber min={0} addonAfter='min' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='expire' label='Expire' style={{marginBottom: '5px'}}>
                      <InputNumber min={0} addonAfter='min' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow> 
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='increaseOcPercent' label='Auto OC' style={{marginBottom: '5px'}}>
                      <InputNumber min={0} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='isActive' label='Active' valuePropName="checked" style={{marginBottom: '5px'}}>
                      <BaseSwitch onChange={onSwitchChange} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              {maxBorrow ? (<BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
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
