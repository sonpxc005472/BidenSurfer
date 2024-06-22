import React, { useEffect, useCallback , useState } from 'react';
import { BaseTable } from '@app/components/common/BaseTable/BaseTable';
import { ColumnsType } from 'antd/es/table';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { useTranslation } from 'react-i18next';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { DeleteOutlined, EditFilled, PlusOutlined, PoweroffOutlined, SettingOutlined } from '@ant-design/icons';
import { BaseSwitch } from '@app/components/common/BaseSwitch/BaseSwitch';
import { ScannerForm, ScannerSettingForm, ScannerTableRow, SymbolData, deleteScanner, getScannerData, getScannerSetting, getSymbolData, setScannerActive, startStopScanner } from 'api/table.api';
import { useMounted } from '@app/hooks/useMounted';
import { BaseModal } from '@app/components/common/BaseModal/BaseModal';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { BaseSelect } from '@app/components/common/selects/BaseSelect/BaseSelect';
import { InputNumber, Typography } from 'antd';
import { doSaveScanner, doSaveScannerSetting } from '@app/store/slices/userSlice';
import { useAppDispatch } from '@app/hooks/reduxHooks';
import { notificationController } from '@app/controllers/notificationController';
import { BaseButtonsForm } from '@app/components/common/forms/BaseButtonsForm/BaseButtonsForm';
import { BaseTag } from '@app/components/common/BaseTag/BaseTag';
import { BaseInput } from '@app/components/common/inputs/BaseInput/BaseInput';
import { AddScannerButton, AddScannerSettingButton } from './Scanner.styles';
import { NumberInput } from '@app/components/header/dropdowns/settingsDropdown/settingsOverlay/nightModeSettings/NightTimeSlider/NightTimeSlider.styles';
import { startStopBot } from '@app/api/user.api';

const initialFormValues: ScannerForm = {
  id: undefined,
  title: '',
  positionSide: 'short',
  orderChange: undefined,
  elastic: undefined,
  amount: undefined,
  turnover: undefined,
  ocNumber: undefined,
  configExpire: undefined,
  amountLimit: undefined,
  onlyPairs: [],
  amountExpire: undefined,
  autoAmount: undefined,
  orderType: 1,
  userId: 0,  
  isActive: true
};

const initialSettingFormValues: ScannerSettingForm = {
  id: undefined,
  userId: 0,
  maxOpen: undefined,
  blackList: [],
};

export const ScannerTable: React.FC = () => {
  const dispatch = useAppDispatch();
  const { Text } = Typography;

  const [tableData, setTableData] = useState<{ data: ScannerTableRow[] }>({
    data: [],
  });
  const [selectOptions, setSelectOptions] = useState<any[]>([]);

  const { t } = useTranslation();
  const { isMounted } = useMounted();
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [isSettingModalOpen, setIsSettingModalOpen] = useState<boolean>(false);

  const [isEditConfig, setIsEditConfig] = useState<boolean>(false);

  const [form] = BaseForm.useForm();  
  const [settingForm] = BaseForm.useForm();  
  const [isStop, setStop] = useState(false);
  const [isStopping, setStopping] = useState(false);

  const fetch = () => {
    getScannerData().then((res) => {
      if (isMounted.current) {          
        setTableData({data: res});
      }
    });      
  };

  useEffect(() => {
    fetch();
  }, [isMounted]);

  const handleOpenScannerSetting = () => {
    getSymbolData().then((res) => {
      if (isMounted.current) {          
        setSelectOptions(res.map((item) => ({ value: item.label, label: item.label })));
      }
    });
    getScannerSetting().then((res) => {
      if (isMounted.current) {          
        settingForm.setFieldsValue({
          ...initialSettingFormValues,
          id: res.id,
          maxOpen: res.maxOpen,
          blackList: res.blackList
        });
        setStop(res.stop ?? false);
        setIsSettingModalOpen(true);
      }      
    });
  };

  const handleOpenAddEdit = (rowId?: number) => {   
    getSymbolData().then((res) => {
      if (isMounted.current) {          
        setSelectOptions(res.map((item) => ({ value: item.label, label: item.label })));
      }
    });
    if(rowId)
      {        
        setIsEditConfig(true);
        let cloneData = {...tableData };
        cloneData.data.forEach(function(part, index, theArray) {
          if(theArray[index].id === rowId)
          {            
            form.setFieldsValue({
              ...initialFormValues,
              title: theArray[index].title,
              positionSide: theArray[index].positionSide,
              amount: theArray[index].amount,
              amountLimit: theArray[index].amountLimit,
              configExpire: theArray[index].configExpire,
              amountExpire: theArray[index].amountExpire,
              autoAmount: theArray[index].autoAmount,
              elastic: theArray[index].elastic,
              isActive: theArray[index].isActive,
              id: theArray[index].id,
              orderChange: theArray[index].orderChange,
              ocNumber: theArray[index].ocNumber,
              orderType: theArray[index].orderType,
              turnover: theArray[index].turnover,
              onlyPairs: theArray[index].onlyPairs
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
    deleteScanner(rowId).then((res) => {
      if (!res) {          
        notificationController.error({ message: "something went wrong!" });        
        fetch();
      }
    }).catch((err) => {
      notificationController.error({ message: err.message });        
    });
    
  };

  const handleActiveRow = (rowId: number, isActive: boolean) => {
    setTableData({
      ...tableData,
      data: tableData.data.map((item) => 
        item.id === rowId ? { ...item, isActive: isActive } : item
      )
    });
    setScannerActive(rowId, isActive).then((res) => {
      if (!res) {      
        notificationController.error({ message: "something went wrong!" });    
        fetch();
      }
    }).catch((err) => {
      notificationController.error({ message: err.message });        
    });
  };

  const columns: ColumnsType<ScannerTableRow> = [
    {
      title: () => (<div>Title ({tableData.data.filter((item) => item.isActive).length}/{tableData.data.length})</div>),
      dataIndex: 'title',
      render: (text: string, record: { id: number; isActive: boolean }) => {
        return (
          <BaseSpace>            
            <BaseTooltip title="edit">
              <BaseButton type="default" size='small' icon={<EditFilled />} onClick={() => handleOpenAddEdit(record.id)} />
            </BaseTooltip>
            <BaseSwitch size='small' checked={record.isActive} onChange={() => handleActiveRow(record.id, !record.isActive)} />
            <div>{text}</div>
          </BaseSpace>
        );
      },
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

  const onSwitchChange = (checked: boolean) => {
    form.setFieldsValue({ isActive: checked });
  };
  
  const handleSaveScanner = () => {
    const title = form.getFieldValue('title');
    const positionSide = form.getFieldValue('positionSide');
    const id = form.getFieldValue('id');
    const orderChange = form.getFieldValue('orderChange');
    const ocNumber = form.getFieldValue('ocNumber');
    const autoAmount = form.getFieldValue('autoAmount');
    const amount = form.getFieldValue('amount');
    const amountLimit = form.getFieldValue('amountLimit');
    const amountExpire = form.getFieldValue('amountExpire');
    const configExpire = form.getFieldValue('configExpire');
    const elastic = form.getFieldValue('elastic');
    const onlyPairs = form.getFieldValue('onlyPairs');
    const turnover = form.getFieldValue('turnover');
    const isActive = form.getFieldValue('isActive');
    const formValues: ScannerForm = {
      id: id,
      title: title,
      userId: 0,
      orderType: 1,
      positionSide: positionSide,
      orderChange: orderChange,
      autoAmount: autoAmount ?? 0,
      amount: amount,
      amountExpire: amountExpire ?? 0,
      amountLimit: amountLimit ?? 0,
      configExpire: configExpire ?? 0,
      isActive: isActive,
      elastic: elastic,
      onlyPairs: onlyPairs,
      turnover: turnover,
      ocNumber: ocNumber      
    };
    dispatch(doSaveScanner(formValues))
      .unwrap()
      .then(() => {
        fetch();
        setIsModalOpen(false);
      })
      .catch((err) => {
        notificationController.error({ message: err.message });        
      });
  };
  const onFinish = async (values = {}) => {
    handleSaveScanner();
  };

  const onSettingFinish = async (values = {}) => {
    handleSaveScannerSetting();
  };

  const handleSaveScannerSetting = () => {
    const maxOpen = settingForm.getFieldValue('maxOpen');
    const id = settingForm.getFieldValue('id');
    const blackList = settingForm.getFieldValue('blackList');
    
    const formValues: ScannerSettingForm = {
      id: id,
      userId: 0,
      maxOpen: maxOpen ?? 15,
      blackList: blackList      
    };
    dispatch(doSaveScannerSetting(formValues))
      .unwrap()
      .then(() => {
        setIsSettingModalOpen(false);
      })
      .catch((err) => {
        notificationController.error({ message: err.message });        
      });
  };
  const handleStartStop = (checked: boolean) => {
    setStopping(true);
    startStopScanner({
      id : 0,
      userId : 0,
      blackList : [],      
      stop : !checked
    }).then((res) => {     
      setStop(!checked);      
    }).finally(()=>{
      setStopping(false);
    });
  }
  
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
            title={ isEditConfig ? 'Edit Scanner': 'Add Scanner'}
            centered
            open={isModalOpen}            
            size="large"
            footer={<></>}
            onCancel={()=>setIsModalOpen(false)}
          >
            <BaseButtonsForm
              name="editForm"
              form={form}      
              isFieldsChanged={true}
              initialValues={initialFormValues}    
              footer={
                <>
                  <BaseButtonsForm.Item style={{marginTop: '30px'}}>
                    <BaseButton type="primary" htmlType="submit" style={{float: 'right', width: '100px', height: '40px'}}>
                      Save
                    </BaseButton>
                    <BaseButton type="primary" htmlType="button" style={{float: 'right', width: '100px', height: '40px', marginRight: '20px'}}
                      onClick={() => {              
                        setIsModalOpen(false);
                      }}>
                      Cancel
                    </BaseButton>
                  </BaseButtonsForm.Item>                
                </>
                
              }    
              onFinish={onFinish}
            >
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={16} md={12}>
                    <BaseButtonsForm.Item name='title' label='Name' style={{marginBottom: '5px'}}
                      rules={[{ required: true, message: 'Name is required' }]}
                    >
                      <BaseInput disabled={isEditConfig} placeholder='Name of scanner' />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={8} md={12}>
                    <BaseButtonsForm.Item name='positionSide' label='Position' style={{marginBottom: '5px'}}
                      rules={[{ required: true, message: 'Position is required' }]}>
                      <BaseSelect disabled={isEditConfig} placeholder='Select position' options={[{value: 'short'}, {value: 'long'}]} />
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
                    <BaseButtonsForm.Item name='elastic' label='Elastic' style={{marginBottom: '5px'}}
                        rules={[{ required: true, message: 'Elastic is required' }]}>
                      <InputNumber min={1} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='turnover' label='Turnover' style={{marginBottom: '5px'}}
                      rules={[{ required: true, message: 'Turnover is required' }]}>
                      <InputNumber min={1} addonAfter='$' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='amount' label='Amount' style={{marginBottom: '5px'}}>
                      <InputNumber min={1} addonAfter='$' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='ocNumber' label='Numbs' style={{marginBottom: '5px'}}
                      rules={[{ required: true, message: 'Number of OC is required' }]}>
                      <InputNumber min={1} style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='configExpire' label='Expire' style={{marginBottom: '5px'}}>
                      <InputNumber min={0} addonAfter='min' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow>
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='autoAmount' label='Auto Amount' style={{marginBottom: '5px'}}>
                      <InputNumber min={1} addonAfter='%' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={12} md={12}>
                    <BaseButtonsForm.Item name='amountLimit' label='Limit' style={{marginBottom: '5px'}}>
                      <InputNumber min={0} addonAfter='$' style={{ width: '100%' }} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
              </BaseRow> 
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={24} md={24}>
                    <BaseButtonsForm.Item name='onlyPairs' label='Only Pairs' style={{marginBottom: '5px'}}>
                      <BaseSelect mode='multiple' showArrow showSearch placeholder='Select symbol' options={selectOptions} />
                    </BaseButtonsForm.Item>
                  </BaseCol>                  
              </BaseRow>
              <BaseRow>
                  <BaseCol xs={18} md={19}>                    
                  </BaseCol>
                  <BaseCol xs={3} md={2}>
                    <BaseButtonsForm.Item name='isActive' style={{marginBottom: '5px'}} valuePropName="checked">
                      <BaseSwitch onChange={onSwitchChange} />
                    </BaseButtonsForm.Item>
                  </BaseCol>
                  <BaseCol xs={3} md={3}>
                    <BaseButtonsForm.Item style={{marginBottom: '5px'}}>
                      <Text>Active</Text>
                    </BaseButtonsForm.Item>                    
                  </BaseCol>
              </BaseRow>
                                          
            </BaseButtonsForm>            
      </BaseModal>
      <BaseModal
            title='Scanner Settings'
            centered
            open={isSettingModalOpen}            
            size="small"
            style={{fontSize: '12px'}}
            footer={<></>}
            onCancel={()=>setIsSettingModalOpen(false)}
          >
            <BaseButtonsForm
              name="settingForm"
              form={settingForm}      
              isFieldsChanged={true}
              initialValues={initialSettingFormValues}    
              footer={
                <>
                  <BaseButtonsForm.Item style={{marginTop: '30px'}}>
                    <BaseButton type="primary" htmlType="submit" style={{float: 'right', width: '80px'}}>
                      Save
                    </BaseButton>   
                    <BaseButton type="primary" htmlType="button" style={{float: 'right', width: '80px', marginRight: '10px'}}
                      onClick={() => {              
                        setIsSettingModalOpen(false);
                      }}>
                      Cancel
                    </BaseButton>                 
                    <BaseSwitch style={{ float: 'left', marginTop: '10px'}} loading={isStopping} size='default' checkedChildren="Stop" unCheckedChildren="Start" checked={!isStop} onChange={handleStartStop} />                    
                  </BaseButtonsForm.Item>                
                </>
                
              }    
              onFinish={onSettingFinish}
            >
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={24} md={24}>
                    <BaseButtonsForm.Item name='maxOpen' label='Open limit'>
                      <NumberInput min={1} max={15} placeholder='OC open limit' />
                    </BaseButtonsForm.Item>
                  </BaseCol>                  
              </BaseRow>
              
              <BaseRow gutter={{ xs: 5, md: 5, xl: 10 }}>
                  <BaseCol xs={24} md={24}>
                    <BaseButtonsForm.Item name='blackList' label='Blacklist'>
                      <BaseSelect mode='multiple' showArrow showSearch placeholder='Select symbol' options={selectOptions} />
                    </BaseButtonsForm.Item>
                  </BaseCol>                  
              </BaseRow>                                                        
            </BaseButtonsForm>            
      </BaseModal>
      <AddScannerSettingButton type="primary" shape="circle" icon={<SettingOutlined />} size="large" onClick={()=> handleOpenScannerSetting()}></AddScannerSettingButton>      
      <AddScannerButton type="primary" shape="circle" icon={<PlusOutlined />} size="large" onClick={()=> handleOpenAddEdit()}></AddScannerButton>      
    </>    
  );
};
