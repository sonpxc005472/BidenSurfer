import React, { useEffect, useState } from 'react';
import { ConfigurationTableRow } from 'api/table.api';
import { BaseTable } from '@app/components/common/BaseTable/BaseTable';
import { ColumnsType } from 'antd/es/table';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { useTranslation } from 'react-i18next';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { DeleteOutlined, EditOutlined } from '@ant-design/icons';
import { BaseRadio } from '@app/components/common/BaseRadio/BaseRadio';
import { BaseSwitch } from '@app/components/common/BaseSwitch/BaseSwitch';

interface ConfigurationTableProps {
  configData: ConfigurationTableRow[];
}
export const ConfigurationTable: React.FC<ConfigurationTableProps> = ({ configData }) => {
  const [tableData, setTableData] = useState<{ data: ConfigurationTableRow[] }>({
    data: [],
  });
  const { t } = useTranslation();

  useEffect(() => {
    setTableData({ data: configData });
  }, []);
  
  const handleDeleteRow = (rowId: string) => {
    setTableData({
      ...tableData,
      data: tableData.data.filter((item) => item.id !== rowId)
    });
  };

  const handleActiveRow = (rowId: string, isActive: boolean) => {
    var cloneData = {...tableData };
    cloneData.data.forEach(function(part, index, theArray) {
      if(theArray[index].id === rowId)
      {
        theArray[index].isActive = isActive
      }
    });
    setTableData(cloneData);
  };

  const columns: ColumnsType<ConfigurationTableRow> = [
    {
      title: t('tables.actions'),
      dataIndex: 'actions',
      width: '10%',
      render: (text: string, record: { id: string; isActive: boolean }) => {
        return (
          <BaseSpace>
            {/* <BaseButton
              type="ghost"
              onClick={() => {
                notificationController.info({ message: t('tables.inviteMessage', { name: record.name }) });
              }}
            >
              {t('tables.invite')}
            </BaseButton> */}
            <BaseTooltip title="edit">
              <BaseButton type="default" size='small' icon={<EditOutlined />} onClick={() => handleDeleteRow(record.id)} />
            </BaseTooltip>
            <BaseSwitch checked={record.isActive} onChange={() => handleActiveRow(record.id, !record.isActive)} />
          </BaseSpace>
        );
      },
    },
    {
      title: 'Position',
      dataIndex: 'positionSide'
    }, 
    {
      title: 'Amount',
      dataIndex: 'amount'
    },
    {
      title: 'OC',
      dataIndex: 'orderChange',
    },
    {
      title: 'Candle',
      dataIndex: 'candleStick',
    },
    {
      title: 'TP',
      dataIndex: 'takeProfit',
    },
    {
      title: 'Reduce',
      dataIndex: 'reduceTakeProfit',
    },  
    {
      title: 'Extend',
      dataIndex: 'extend',
    },
    {
      title: 'Stoploss',
      dataIndex: 'stopLoss',
    }, 
    {
      title: '',
      dataIndex: 'delete',
      width: '5%',
      render: (text: string, record: { id: string }) => {
        return (
          <BaseSpace>            
            <BaseTooltip title="Delete">
              <BaseButton type="primary" shape="circle" icon={<DeleteOutlined />} size="small" onClick={() => handleDeleteRow(record.id)}/>
            </BaseTooltip>
          </BaseSpace>
        );
      },
    }
  ];

  return (
    <BaseTable
      columns={columns}
      dataSource={tableData.data}
      pagination={false}
      scroll={{ x: 800 }}
      bordered
    />
  );
};
