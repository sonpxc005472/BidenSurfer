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
    USD: 0,
    ETH: 0,
    BTC: 0,
  });

  const userId = useAppSelector((state) => state.user.user?.id);

  useEffect(() => {
    userId && getBalance(userId).then((res) => setBalance(res));
  }, [userId]);

  const { t } = useTranslation();
  return (
    <BaseRow>
      <BaseCol span={24}>
        <S.TitleText level={2}>{t('nft.yourBalance')}</S.TitleText>
      </BaseCol>
      <BaseCol span={24}>
        <S.TitleBalanceText level={4}>
          {getCurrencyPrice(formatNumberWithCommas(balance.USD), CurrencyTypeEnum['USD'])}
        </S.TitleBalanceText>
      </BaseCol>
    </BaseRow>
  );
};
