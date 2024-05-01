import React, { useEffect, useState } from 'react';
import { useAppSelector } from '@app/hooks/reduxHooks';
import { formatNumberWithCommas, getCurrencyPrice } from '@app/utils/utils';
import { Balance as IBalance, getBalance } from '@app/api/earnings.api';
import { CurrencyTypeEnum } from '@app/interfaces/interfaces';
import * as S from './Balance.styles';
import { BaseRow } from '@app/components/common/BaseRow/BaseRow';
import { BaseCol } from '@app/components/common/BaseCol/BaseCol';

export const Balance: React.FC = () => {
  const [balance, setBalance] = useState<IBalance>({
    total: 0,
    available: 0
  });

  const userId = useAppSelector((state) => state.user.user?.id);

  useEffect(() => {
    userId && getBalance().then((res) => {
      setBalance(res)
    });
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
              {getCurrencyPrice(formatNumberWithCommas(balance.total), CurrencyTypeEnum['USD'])}
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
              {getCurrencyPrice(formatNumberWithCommas(balance.available), CurrencyTypeEnum['USD'])}
            </S.TitleBalanceText>
          </BaseCol>
        </BaseRow>
      </BaseCol>
    </BaseRow>
    
  );
};
