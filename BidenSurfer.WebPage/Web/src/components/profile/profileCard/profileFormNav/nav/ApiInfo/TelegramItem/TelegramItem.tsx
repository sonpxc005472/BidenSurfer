import React from 'react';
import { BaseInput } from '@app/components/common/inputs/BaseInput/BaseInput';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';

export const TelegramItem: React.FC = () => {
  return (
    <BaseForm.Item name='teleChannel' label='Telegram channel'>
      <BaseInput />
    </BaseForm.Item>
  );
};
