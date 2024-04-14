import React, { useState } from 'react';
import { Col, Row } from 'antd';
import { useTranslation } from 'react-i18next';
import { BaseButtonsForm } from '@app/components/common/forms/BaseButtonsForm/BaseButtonsForm';
import { TwoFactorOptions } from '@app/components/profile/profileCard/profileFormNav/nav/SecuritySettings/twoFactorAuth/TwoFactorOptions/TwoFactorOptions';
import { useAppDispatch, useAppSelector } from '@app/hooks/reduxHooks';
import { TwoFactorAuthOption } from '@app/interfaces/interfaces';

export interface CurrentOption {
  value: 'phone' | 'email';
  isVerified: boolean;
}

export type TwoFactorAuthOptionState = TwoFactorAuthOption | null;

export const TwoFactorAuth: React.FC = () => {
  const user = useAppSelector((state) => state.user.user);


  const [isFieldsChanged, setFieldsChanged] = useState(Boolean(false));
  const [isLoading, setLoading] = useState(false);

  const [selectedOption, setSelectedOption] = useState<TwoFactorAuthOptionState>('email');
  const [isClickedVerify, setClickedVerify] = useState(false);

  const dispatch = useAppDispatch();

  const { t } = useTranslation();

  const onClickVerify = () => {
    setClickedVerify(true);
  };

  return (
    <>
      <BaseButtonsForm
        name="twoFactorAuth"
        requiredMark="optional"
        isFieldsChanged={isFieldsChanged}
        onFieldsChange={() => setFieldsChanged(true)}
        initialValues={{
          email: user?.email
        }}
        footer={<span />}
        onFinish={onClickVerify}
      >
        <Row>
          <Col span={24}>
              <TwoFactorOptions selectedOption={selectedOption} setSelectedOption={setSelectedOption} />
            </Col>
        </Row>
      </BaseButtonsForm>
      
    </>
  );
};
