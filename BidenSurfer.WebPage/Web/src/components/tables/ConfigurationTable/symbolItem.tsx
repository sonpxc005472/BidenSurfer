import React from 'react';
import { BaseForm } from '@app/components/common/forms/BaseForm/BaseForm';
import { BaseSelect } from '@app/components/common/selects/BaseSelect/BaseSelect';
interface SelectInfoProps {
  options: {value: any}[];
  defaultValues: string[];
}
export const SymbolItem: React.FC<SelectInfoProps> = ({ options, defaultValues }) => {
  return (
    <BaseForm.Item name='symbol' label='Symbol'>
      <BaseSelect showSearch placeholder='Select a symbol' defaultValue={defaultValues} options={options} />
    </BaseForm.Item>
  );
};
