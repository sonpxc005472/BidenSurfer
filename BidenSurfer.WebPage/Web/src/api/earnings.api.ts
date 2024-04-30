import { CurrencyTypeEnum } from '@app/interfaces/interfaces';
import { httpApi } from './http.api';

/* eslint-disable @typescript-eslint/no-unused-vars */
export interface Balance {
  Total: number;
  Available: number;
}

export interface Earning {
  date: number;
  usd_value: number;
}

export interface TotalEarning {
  total: number;
  prevTotal: number;
  currency: CurrencyTypeEnum;
  timeline: Earning[];
}

export const getBalance = (): Promise<Balance> => 
  httpApi.get<Balance>('user/get-balance').then(({ data }) => data);


export const getTotalEarning = (id: number, currency: CurrencyTypeEnum): Promise<TotalEarning> => {
  return new Promise((res) => {
    setTimeout(() => {
      res({
        total: 1520,
        prevTotal: 1320,
        currency: CurrencyTypeEnum.USD,
        timeline: [
          {
            date: Date.now() - 1000 * 60 * 60 * 24 * 7,
            usd_value: 100,
          },
          {
            date: Date.now() - 1000 * 60 * 60 * 24 * 6,
            usd_value: 200,
          },
          {
            date: Date.now() - 1000 * 60 * 60 * 24 * 5,
            usd_value: 180,
          },
          {
            date: Date.now() - 1000 * 60 * 60 * 24 * 4,
            usd_value: 350,
          },
          {
            date: Date.now() - 1000 * 60 * 60 * 24 * 3,
            usd_value: 280,
          },
          {
            date: Date.now() - 1000 * 60 * 60 * 24 * 2,
            usd_value: 300,
          },
          {
            date: Date.now() - 1000 * 60 * 60 * 24,
            usd_value: 320,
          },
        ],
      });
    }, 0);
  });
};
