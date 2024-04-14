import React from 'react';
import { BaseInput } from '@app/components/common/inputs/BaseInput/BaseInput';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';

export const SecretKeyItem: React.FC = () => {
  return (
    <BaseForm.Item name='secretKey' label='Secret Key'>
      <BaseInput />
    </BaseForm.Item>
  );
};
