import React from 'react';
import { BaseInput } from '@app/components/common/inputs/BaseInput/BaseInput';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';

export const ApiKeyItem: React.FC = () => {
  return (
    <BaseForm.Item name='apiKey' label='API Key'>
      <BaseInput />
    </BaseForm.Item>
  );
};
