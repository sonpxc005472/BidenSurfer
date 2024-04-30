import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAppSelector } from '@app/hooks/reduxHooks';
import { formatNumberWithCommas, getCurrencyPrice } from '@app/utils/utils';
import { Balance as IBalance, getBalance } from '@app/api/earnings.api';
import { CurrencyTypeEnum } from '@app/interfaces/interfaces';
import * as S from './Balance.styles';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';

export const Balance: React.FC = () => {
  const [balance, setBalance] = useState<IBalance>({
    Total: 0,
    Available: 0
  });

  const userId = useAppSelector((state) => state.user.user?.id);

  useEffect(() => {
    userId && getBalance().then((res) => setBalance(res));
  }, [userId]);

  return (
    <BaseRow justify="space-between" align="middle">
      <BaseCol>
        <BaseRow>
          <BaseCol span={24}>
            <S.TitleText level={2}>Balance</S.TitleText>
          </BaseCol>
          <BaseCol span={24}>
            <S.TitleBalanceText level={5}>
              {getCurrencyPrice(formatNumberWithCommas(balance.Total), CurrencyTypeEnum['USD'])}
            </S.TitleBalanceText>
          </BaseCol>          
        </BaseRow>
      </BaseCol>
      <BaseCol>
        <BaseRow>          
          <BaseCol span={24}>
            <S.TitleText level={2}>Available</S.TitleText>
          </BaseCol>
          <BaseCol span={24}>
            <S.TitleBalanceText level={5}>
              {getCurrencyPrice(formatNumberWithCommas(balance.Available), CurrencyTypeEnum['USD'])}
            </S.TitleBalanceText>
          </BaseCol>
        </BaseRow>
      </BaseCol>
    </BaseRow>
    
  );
};
