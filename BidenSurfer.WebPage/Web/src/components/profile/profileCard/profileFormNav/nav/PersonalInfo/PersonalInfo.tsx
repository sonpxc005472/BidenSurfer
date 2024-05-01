import React, { useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { BaseButtonsForm } from '@app/components/common/forms/BaseButtonsForm/BaseButtonsForm';
import { BaseCard } from '@app/components/common/BaseCard/BaseCard';
import { FirstNameItem } from '@app/components/profile/profileCard/profileFormNav/nav/PersonalInfo/FirstNameItem/FirstNameItem';
import { EmailItem } from '@app/components/profile/profileCard/profileFormNav/nav/PersonalInfo/EmailItem/EmailItem';
import { useAppSelector } from '@app/hooks/reduxHooks';
import { Dates } from '@app/constants/Dates';
import { notificationController } from '@app/controllers/notificationController';
import { PaymentCard } from '@app/interfaces/interfaces';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';

interface PersonalInfoFormValues {
  birthday?: string;
  fullName?: string;
  country?: string;
  website: string;
  city?: string;
  address2: string;
  nickName?: string;
  address1?: string;
  sex?: string;
  facebook?: string;
  language?: string;
  linkedin?: string;
  zipcode?: string;
  twitter?: string;
  phone?: string;
  email: string;
}

const initialPersonalInfoValues: PersonalInfoFormValues = {
  fullName: '',
  nickName: '',
  sex: undefined,
  birthday: undefined,
  language: undefined,
  phone: '',
  email: '',
  country: undefined,
  city: undefined,
  address1: '',
  address2: '',
  zipcode: '',
  website: '',
  twitter: '',
  linkedin: '',
  facebook: '',
};

export const PersonalInfo: React.FC = () => {
  const user = useAppSelector((state) => state.user.user);

  const [isFieldsChanged, setFieldsChanged] = useState(false);
  const [isLoading, setLoading] = useState(false);

  const userFormValues = useMemo(
    () =>
      user
        ? {
            fullName: user.fullName,
            email: user.email,
            nickname: user.username,
            sex: user.sex,
            language: user.lang,
            country: user.country,
            city: user.city,
            address1: user.address1,
            address2: user?.address2,
            zipcode: user.zipcode,
            website: user?.website,
            twitter: user?.socials?.twitter,
            linkedin: user?.socials?.linkedin,
            facebook: user?.socials?.facebook,
          }
        : initialPersonalInfoValues,
    [user],
  );

  const [form] = BaseButtonsForm.useForm();

  const { t } = useTranslation();

  const onFinish = useCallback(
    (values: PaymentCard) => {
      // todo dispatch an action here
      setLoading(true);
      setTimeout(() => {
        setLoading(false);
        setFieldsChanged(false);
        notificationController.success({ message: t('common.success') });
        console.log(values);
      }, 1000);
    },
    [t],
  );

  return (
    <BaseCard>
      <BaseButtonsForm
        form={form}
        name="info"
        loading={isLoading}
        initialValues={userFormValues}
        isFieldsChanged={isFieldsChanged}
        setFieldsChanged={setFieldsChanged}
        onFieldsChange={() => setFieldsChanged(true)}
        onFinish={onFinish}
      >
        <BaseRow gutter={{ xs: 10, md: 15, xl: 30 }}>
          <BaseCol span={24}>
            <BaseButtonsForm.Item>
              <BaseButtonsForm.Title>{t('profile.nav.personalInfo.title')}</BaseButtonsForm.Title>
            </BaseButtonsForm.Item>
          </BaseCol>

          <BaseCol xs={24} md={24}>
            <FirstNameItem />
          </BaseCol> 

          <BaseCol xs={24} md={24}>
            <EmailItem verified={true} />
          </BaseCol>          
        </BaseRow>
      </BaseButtonsForm>
    </BaseCard>
  );
};
