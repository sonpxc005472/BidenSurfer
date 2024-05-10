import React, { useCallback, useEffect, useState } from 'react';
import { BaseButtonsForm } from '@app/components/common/forms/BaseButtonsForm/BaseButtonsForm';
import { BaseCard } from '@app/components/common/BaseCard/BaseCard';
import { useAppDispatch, useAppSelector } from '@app/hooks/reduxHooks';
import { notificationController } from '@app/controllers/notificationController';
import { PaymentCard } from '@app/interfaces/interfaces';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';
import { ApiKeyItem } from './ApiKeyItem/ApiKeyItem';
import { SecretKeyItem } from './SecretKeyItem/SecretKeyItem';
import { PassPhraseItem } from './PassPhraseItem/PassPhraseItem';
import { TelegramItem } from './TelegramItem/TelegramItem';
import { ApiData, getApiData, saveApi } from 'api/user.api';
import { doSaveApi } from '@app/store/slices/userSlice';
import { useMounted } from '@app/hooks/useMounted';
import * as S from '../SecuritySettings/passwordForm/PasswordForm/PasswordForm.styles';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { mergeBy } from '@app/utils/utils';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';

interface FieldData {
  name: string | number;
  //  
  value?: any;
}
export const ApiInfo: React.FC = () => {
  const user = useAppSelector((state) => state.user.user);
  const dispatch = useAppDispatch();

  const [isLoading, setLoading] = useState(false);
  const { isMounted } = useMounted();
  const [fields, setFields] = useState<FieldData[]>([
    { name: 'apiKey', value: '' },
    { name: 'secretKey', value: '' },
    { name: 'passPhrase', value: '' },
    { name: 'teleChannel', value: '' }    
  ]);
  const fetch = useCallback(
    () => {
      setLoading(true);
      getApiData(user?.id).then((res) => {     
        let fs : FieldData[] = [];  
        if (isMounted.current) {       
          const keys = Object.keys(res) as (keyof typeof res)[];
          keys.forEach((key) => {            
            fs.push({name: key, value: res[key]})
          });
          setFields(fs);
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
  const handleSubmit = () => {
    const fv = [...fields];
    setLoading(true);
    
    let aKey = '';
    let aSecret = '';
    let aPass = '';
    let aTele = '';
    let aUserId = '';
    let aid = 0;
    for (let api of fv) {
      if (api.name === 'apiKey') {
         aKey = api.value;
      }
      else if(api.name ==='secretKey')
      {
          aSecret = api.value;
      }
      else if(api.name ==='passPhrase')
      {
        aPass = api.value;
      }
      else if(api.name ==='teleChannel')
      {
        aTele = api.value;
      }
      else if(api.name ==='userId')
      {
        aUserId = api.value;
      }
      else if(api.name ==='id')
      {
        aid = api.value;
      }
   }
    dispatch(doSaveApi({
      apiKey: aKey,
      secretKey : aSecret,
      passPhrase : "",
      teleChannel : aTele,
      userId : aUserId,
      id : aid
    }))
      .unwrap()
      .then(() => {
        notificationController.info({ message: 'save APIs successfully' });
        setLoading(false);
      })
      .catch((err) => {
        notificationController.error({ message: err.message });
        setLoading(false);
      });
  };  

  
  return (
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
              <BaseForm.Title>API</BaseForm.Title>
            </BaseForm.Item>
          </BaseCol>

          <BaseCol xs={24} md={24}>
            <ApiKeyItem />
          </BaseCol>
          <BaseCol xs={24} md={24}>
            <SecretKeyItem />
          </BaseCol>
          
          <BaseCol span={24}>
            <BaseForm.Item>
              <BaseForm.Title>Telegram</BaseForm.Title>
            </BaseForm.Item>
          </BaseCol>

          <BaseCol xs={24} md={24}>
            <TelegramItem />
          </BaseCol>
          
      </BaseRow>
      <BaseRow>
          <BaseButton type="primary" loading={isLoading} onClick={handleSubmit}>
            Save
          </BaseButton>
      </BaseRow>
    </BaseForm>
  );
};
