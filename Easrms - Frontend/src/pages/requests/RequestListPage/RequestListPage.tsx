import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';

import { useGetRequestsQuery } from '../../../store/api/request.endpoints';
import { useGetCategoriesQuery } from '../../../store/api/category.endpoints';
import { useAppSelector } from '../../../hooks/useAppSelector';

import AppPageHeader from '../../../components/common/layout/AppPageHeader';
import AppButton from '../../../components/common/buttons/AppButton';
import AppSearchInput from '../../../components/common/filter/AppSearchInput';
import AppFilterBar from '../../../components/common/filter/AppFilterBar';
import AppSelect from '../../../components/common/form/AppSelect';
import AppDataGrid from '../../../components/common/table/AppDataGrid';
import AppPagination from '../../../components/common/table/AppPagination';
import AppStatusBadge from '../../../components/common/table/AppStatusBadge';
import AppPriorityBadge from '../../../components/common/table/AppPriorityBadge';
import AppLoader from '../../../components/common/feedback/AppLoader';
import AppErrorState from '../../../components/common/feedback/AppErrorState';

import { ROLES } from '../../../constants/role.constants';
import { STATUS_OPTIONS } from '../../../constants/status.constants';
import { PRIORITY_OPTIONS } from '../../../constants/priority.constants';
import { formatDate } from '../../../utils/formatDate';

import type { RequestListDto, RequestQueryParams } from '../../../types/request.types';
import type { GridColumn } from '../../../types/common.types';

// ─── Component ────────────────────────────────────────────────────────────────
const RequestListPage = () => {
  const navigate = useNavigate();
  const { roleName } = useAppSelector((state) => state.auth);

  const [params, setParams] = useState<RequestQueryParams>({
    pageNumber: 1,
    pageSize: 10,
  });

  const { data: response, isLoading, isError } = useGetRequestsQuery(params);
  const { data: categoriesResponse } = useGetCategoriesQuery({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const categoryOptions = [
    { label: 'All Categories', value: '' },
    ...(categoriesResponse?.data?.items?.map((c) => ({
      label: c.categoryName,
      value: c.categoryId,
    })) ?? []),
  ];

  const statusOptions = [{ label: 'All Statuses', value: '' }, ...STATUS_OPTIONS];
  const priorityOptions = [{ label: 'All Priorities', value: '' }, ...PRIORITY_OPTIONS];

  // ─── Table Columns ────────────────────────────────────────────────────────────
  const columns: GridColumn<RequestListDto>[] = [
    {
      key: 'requestNumber',
      label: 'Request #',
      render: (row) => (
        <span style={{ fontFamily: 'monospace', fontWeight: 600 }}>
          {row.requestNumber}
        </span>
      ),
    },
    { key: 'title', label: 'Title' },
    { key: 'categoryName', label: 'Category' },
    {
      key: 'priority',
      label: 'Priority',
      render: (row) => <AppPriorityBadge priority={row.priority} />,
    },
    {
      key: 'status',
      label: 'Status',
      render: (row) => <AppStatusBadge status={row.status} />,
    },
    { key: 'assigneeName', label: 'Assignee' },
    {
      key: 'createdOn',
      label: 'Created On',
      render: (row) => formatDate(row.createdOn),
    },
  ];

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success) return <AppErrorState message="Failed to load requests" />;

  const requests = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Service Requests"
        subtitle="View and manage all requests"
        actions={
          roleName === ROLES.EMPLOYEE ? (
            <AppButton
              label="New Request"
              startIcon={<AddIcon />}
              onClick={() => navigate('/requests/create')}
            />
          ) : undefined
        }
      />

      {/* Filters */}
      <AppFilterBar>
        <AppSearchInput
          onSearch={(val) =>
            setParams((p) => ({ ...p, search: val, pageNumber: 1 }))
          }
          placeholder="Search by request number or title"
        />
        <AppSelect
          value={params.status ?? ''}
          onChange={(e) =>
            setParams((p) => ({ ...p, status: e.target.value as string, pageNumber: 1 }))
          }
          options={statusOptions}
          placeholder="Status"
        />
        <AppSelect
          value={params.priority ?? ''}
          onChange={(e) =>
            setParams((p) => ({ ...p, priority: e.target.value as string, pageNumber: 1 }))
          }
          options={priorityOptions}
          placeholder="Priority"
        />
        <AppSelect
          value={params.categoryId ?? ''}
          onChange={(e) =>
            setParams((p) => ({ ...p, categoryId: e.target.value as string, pageNumber: 1 }))
          }
          options={categoryOptions}
          placeholder="Category"
        />
      </AppFilterBar>

      {/* Table */}
      <AppDataGrid
        columns={columns}
        rows={requests}
        keyField="requestId"
        onRowClick={(row) => navigate(`/requests/${row.requestId}`)}
      />

      {/* Pagination */}
      <AppPagination
        page={pagination.pageNumber}
        pageSize={pagination.pageSize}
        total={pagination.totalCount}
        onPageChange={(page) => setParams((p) => ({ ...p, pageNumber: page }))}
        onPageSizeChange={(size) =>
          setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }))
        }
      />
    </Stack>
  );
};

export default RequestListPage;