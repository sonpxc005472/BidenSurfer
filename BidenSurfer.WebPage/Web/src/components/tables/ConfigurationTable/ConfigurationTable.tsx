import React, { useEffect, useCallback , useState } from 'react';
import { BaseTable } from '@app/components/common/BaseTable/BaseTable';
import { ColumnsType } from 'antd/es/table';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { useTranslation } from 'react-i18next';
import { BaseSpace } from '@app/components/common/BaseSpace/BaseSpace';
import { BaseTooltip } from '@app/components/common/BaseTooltip/BaseTooltip';
import { DeleteOutlined, EditOutlined } from '@ant-design/icons';
import { BaseRadio } from '@app/components/common/BaseRadio/BaseRadio';
import { BaseSwitch } from '@app/components/common/BaseSwitch/BaseSwitch';
import { ConfigurationTableRow, getConfigurationData } from 'api/table.api';
import { useMounted } from '@app/hooks/useMounted';

interface ConfigurationTableProps {
  configData: ConfigurationTableRow[];
}
export const ConfigurationTable: React.FC = () => {
  const [tableData, setTableData] = useState<{ data: ConfigurationTableRow[] }>({
    data: [],
  });
  const { t } = useTranslation();
  const { isMounted } = useMounted();
  const fetch = useCallback(
    () => {
      getConfigurationData().then((res) => {
        if (isMounted.current) {          
          setTableData({data: res});
        }
      });
    },
    [isMounted],
  );

  useEffect(() => {
    fetch();
  }, [fetch]);

    
  const handleDeleteRow = (rowId: number) => {
    setTableData({
      ...tableData,
      data: tableData.data.filter((item) => item.id !== rowId)
    });
  };

  const handleActiveRow = (rowId: number, isActive: boolean) => {
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
      render: (text: string, record: { id: number; isActive: boolean }) => {
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
      title: 'Symbol',
      dataIndex: 'symbol'
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
      title: 'Auto Amount',
      dataIndex: 'increaseAmountPercent',
    },
    {
      title: 'Amount Expire',
      dataIndex: 'increaseAmountExpire',
    },
    {
      title: 'Amount Limit',
      dataIndex: 'amountLimit',
    },  
    {
      title: 'Expire',
      dataIndex: 'expire',
    },
    {
      title: '',
      dataIndex: 'delete',
      width: '5%',
      render: (text: string, record: { id: number }) => {
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
