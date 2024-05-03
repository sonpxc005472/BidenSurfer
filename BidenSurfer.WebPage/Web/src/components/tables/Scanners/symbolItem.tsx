import React from 'react';
import { BaseInput } from '@app/components/common/inputs/BaseInput/BaseInput';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';

export const SymbolItem: React.FC = () => {
  return (
    <BaseForm.Item name='symbol' label='Symbol'>
      <BaseInput />
    </BaseForm.Item>
  );
};
