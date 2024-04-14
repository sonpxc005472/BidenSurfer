import React from 'react';
import { BaseInput } from '@app/components/common/inputs/BaseInput/BaseInput';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';

export const PassPhraseItem: React.FC = () => {
  return (
    <BaseForm.Item name='passPhrase' label='Passphrase'>
      <BaseInput />
    </BaseForm.Item>
  );
};
