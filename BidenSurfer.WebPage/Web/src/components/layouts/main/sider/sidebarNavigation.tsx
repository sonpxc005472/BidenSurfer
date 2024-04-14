import React from 'react';
import {
  CompassOutlined,
  DashboardOutlined,
  FormOutlined,
  HomeOutlined,
  LayoutOutlined,
  LineChartOutlined,
  TableOutlined,
  UserOutlined,
  BlockOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { ReactComponent as NftIcon } from '@app/assets/icons/nft-icon.svg';

export interface SidebarNavigationItem {
  title: string;
  key: string;
  url?: string;
  children?: SidebarNavigationItem[];
  icon?: React.ReactNode;
}

export const sidebarNavigation: SidebarNavigationItem[] = [  
  {
    title: 'common.apps',
    key: 'apps',
    icon: <HomeOutlined />,
    url: '/apps'
  },
  {
    title: 'Configurations',
    key: 'configurations',
    icon: <TableOutlined />,
    url: '/configurations'
  },
  {
    title: 'Settings',
    key: 'settings',
    icon: <SettingOutlined />,
    url: '/api-settings'
  }
]