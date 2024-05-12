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
  SettingOutlined,
  ScanOutlined,
  ApiOutlined
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
    title: 'Configurations',
    key: 'configurations',
    icon: <TableOutlined />,
    url: '/configurations'
  },
  {
    title: 'Scanners',
    key: 'scanners',
    icon: <ScanOutlined />,
    url: '/scanners'
  },
  {
    title: 'Api Settings',
    key: 'api-settings',
    icon: <ApiOutlined />,
    url: '/api-settings'
  },
  {
    title: 'General Settings',
    key: 'general-settings',
    icon: <SettingOutlined />,
    url: '/general-settings'
  }
]