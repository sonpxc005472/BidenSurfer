import React, { useState, useEffect, useCallback } from 'react';
import { PageTitle } from '@app/components/common/PageTitle/PageTitle';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { useMounted } from '@app/hooks/useMounted';
import { useAppDispatch, useAppSelector } from '@app/hooks/reduxHooks';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { notificationController } from '@app/controllers/notificationController';
import { doSaveGeneralSetting, setBotStatus } from '@app/store/slices/userSlice';
import { BaseButtonsForm } from '@app/components/common/forms/BaseButtonsForm/BaseButtonsForm';
import { getGeneralSetting, startStopBot } from '@app/api/user.api';
import { mergeBy } from '@app/utils/utils';
import { Button, InputNumber } from 'antd';
import { PoweroffOutlined } from '@ant-design/icons';
interface FieldData {
  name: string | number;
  //  
  value?: any;
}
const GeneralSettingPage: React.FC = () => {
  const dispatch = useAppDispatch();

  const [isLoading, setLoading] = useState(false);

  const [isStop, setStop] = useState(false);
  const [isStopping, setStopping] = useState(false);

  const { isMounted } = useMounted();
  const [fields, setFields] = useState<FieldData[]>([
    { name: 'budget', value: '' },
    { name: 'assetTracking', value: '' }  
  ]);
  const fetch = useCallback(
    () => {
      setLoading(true);
      getGeneralSetting().then((res) => {     
        let fs : FieldData[] = [];  
        if (isMounted.current) {       
          const keys = Object.keys(res) as (keyof typeof res)[];
          keys.forEach((key) => {            
            fs.push({name: key, value: res[key]})
          });
          setFields(fs);
          setStop(res.stop ?? false)
        }
      }).finally(()=>{
        setLoading(false);
      });
    },
    [isMounted],
  );

  useEffect(() => {
    fetch();
  }, [fetch]);  
  
  const [form] = BaseButtonsForm.useForm();
  const handleStartStop = () => {
      setStopping(true);
      startStopBot({
        id : 0,
        userId : 0,
        assetTracking : 0,
        budget : 0,
        stop : !isStop
      }).then((res) => {     
        setStop(!isStop);
        dispatch(setBotStatus(!isStop));
      }).finally(()=>{
        setStopping(false);
      });
  }
  const handleSubmit = () => {
    const fv = [...fields];
    setLoading(true);
    
    let budget = undefined;
    let assetTracking = undefined;
    let userId = 0;
    let id = 0;
    for (let api of fv) {
      if (api.name === 'budget') {
         budget = api.value;
      }
      else if(api.name ==='assetTracking')
      {
          assetTracking = api.value;
      }
      
      else if(api.name ==='userId')
      {
        userId = api.value;
      }
      else if(api.name ==='id')
      {
        id = api.value;
      }
   }
    dispatch(doSaveGeneralSetting({
      budget: budget,
      assetTracking : assetTracking,
      userId : userId,
      id : id
    }))
      .unwrap()
      .then(() => {
        notificationController.info({ message: 'save general setting successfully' });
        setLoading(false);
      })
      .catch((err) => {
        notificationController.error({ message: err.message });
        setLoading(false);
      });
  };  
  
  return (
    <>
      <PageTitle>General Settings</PageTitle>
      <BaseForm
      name="apiForm"
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
        <BaseCol span={24}>
          <BaseForm.Item>
            <BaseForm.Title>General Setting</BaseForm.Title>
          </BaseForm.Item>
        </BaseCol>

        <BaseCol xs={24} md={24}>
          <BaseForm.Item name='budget' label='Budget'>
            <InputNumber min={0} addonAfter='$' style={{ width: '100%' }} />
          </BaseForm.Item>
        </BaseCol>

        <BaseCol xs={24} md={24}>
          <BaseForm.Item name='assetTracking' label='Asset Tracking'>
            <InputNumber min={0} addonAfter='$' style={{ width: '100%' }} />
          </BaseForm.Item>
        </BaseCol>
      </BaseRow>
      <BaseRow>
          <BaseButton type="primary" loading={isLoading} onClick={handleSubmit}>
            Save
          </BaseButton>
      </BaseRow>
    </BaseForm>    
    <BaseRow style={{marginTop: "20px"}}>
      <h1>Bot Setting</h1>
    </BaseRow>
    <BaseRow style={{marginTop: "10px"}}>
        <Button
          type="primary"
          loading={isStopping}
          icon={<PoweroffOutlined />}
          onClick={handleStartStop}
        >
          {isStop ? (<>Start</>) : (<>Stop</>)}
        </Button>      
    </BaseRow>
    </>
  );
};

export default GeneralSettingPage;
