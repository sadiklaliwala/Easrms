import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Stack } from "@mui/material";
import toast from "react-hot-toast";

import {
  useGetRequestsQuery,
  useAssignRequestMutation,
} from "../../../store/api/request.endpoints";
import { useGetSupportUsersQuery } from "../../../store/api/lookup.endpoints";

import AppPageHeader from "../../../components/common/layout/AppPageHeader";
import AppSearchInput from "../../../components/common/filter/AppSearchInput";
import AppFilterBar from "../../../components/common/filter/AppFilterBar";
import AppSelect from "../../../components/common/form/AppSelect";
import AppDataGrid from "../../../components/common/table/AppDataGrid";
import AppPagination from "../../../components/common/table/AppPagination";
import AppStatusBadge from "../../../components/common/table/AppStatusBadge";
import AppPriorityBadge from "../../../components/common/table/AppPriorityBadge";
import AppTableActions from "../../../components/common/table/AppTableActions";
import AppLoader from "../../../components/common/feedback/AppLoader";
import AppErrorState from "../../../components/common/feedback/AppErrorState";
import AssignSupportDialog from "../../../components/common/modal/AssignSupportDialog";

import {
  STATUS,
  // STATUS_ENUM,
  STATUS_ENUM_REVERSE,
  STATUS_OPTIONS,
  type StatusType,
} from "../../../constants/status.constants";
import { formatDate } from "../../../utils/formatDate";

import type {
  RequestListDto,
  RequestQueryParams,
} from "../../../types/request.types";
import type { GridColumn } from "../../../types/common.types";
import {
  PRIORITY_LABEL,
  // PRIORITY_ENUM,
  // PRIORITY_ENUM_REVERSE,
} from "../../../constants/priority.constants";

// ─── Eligible statuses for assignment ─────────────────────────────────────────
const ASSIGNABLE_STATUSES: StatusType[] = [STATUS.OPEN, STATUS.APPROVED];

const assignableStatusOptions = [
  { label: "Open & Approved", value: "" },
  ...STATUS_OPTIONS.filter((s) => ASSIGNABLE_STATUSES.includes(s.value)),
];

// ─── Component ────────────────────────────────────────────────────────────────
const AssignmentPage = () => {
  const navigate = useNavigate();

  const [params, setParams] = useState<RequestQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    status: STATUS.OPEN,
    sortBy: "createdOn",
    sortAscending: false,
  });

  const [selectedRequest, setSelectedRequest] = useState<RequestListDto | null>(
    null,
  );
  const [assignOpen, setAssignOpen] = useState(false);

  const { data: response, isLoading, isError } = useGetRequestsQuery(params);
  const { data: supportUsersResponse } = useGetSupportUsersQuery();
  const [assignRequest, { isLoading: assigning }] = useAssignRequestMutation();

  const supportUsers = supportUsersResponse?.data ?? [];

  // ─── Handlers ─────────────────────────────────────────────────────────────────
  const handleAssign = async (supportUserId: string) => {
    if (!selectedRequest) return;
    try {
      const res = await assignRequest({
        id: selectedRequest.requestId,
        body: { supportUserId },
      }).unwrap();
      if (res.success) {
        toast.success("Request assigned successfully");
        setAssignOpen(false);
        setSelectedRequest(null);
      } else {
        toast.error(res.message ?? "Assignment failed");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Assignment failed");
    }
  };

  const openAssignDialog = (row: RequestListDto) => {
    setSelectedRequest(row);
    setAssignOpen(true);
  };

  const handleSortChange = (sortBy: string, sortAscending: boolean) => {
    setParams((prev) => ({
      ...prev,
      sortBy,
      sortAscending,
      pageNumber: 1,
    }));
  };

  // ─── Table Columns ────────────────────────────────────────────────────────────
  const columns: GridColumn<RequestListDto>[] = [
    {
      key: "requestNumber",
      label: "Request #",
      render: (row) => (
        <span style={{ fontFamily: "monospace", fontWeight: 600 }}>
          {row.requestNumber}
        </span>
      ),
    },
    { key: "title", label: "Title" },
    { key: "categoryName", label: "Category" },
    {
      key: "priority",
      label: "Priority",
      render: (row) => (
        <AppPriorityBadge priority={PRIORITY_LABEL[row.priority]} />
      ),
    },
    {
      key: "status",
      label: "Status",
      render: (row) => (
        <AppStatusBadge status={STATUS_ENUM_REVERSE[row.status].toString()} />
      ),
    },
    {
      key: "assigneeName",
      label: "Assignee",
      render: (row) => row.assigneeName ?? "—",
    },
    {
      key: "createdOn",
      label: "Created On",
      render: (row) => formatDate(row.createdOn),
    },
    {
      key: "actions",
      label: "Actions",
      sortable: false,
      render: (row) => (
        <AppTableActions
          actions={[
            {
              label: "View Details",
              onClick: () => navigate(`/requests/${row.requestId}`),
            },
            {
              label: "Assign",
              onClick: () => openAssignDialog(row),
            },
          ]}
        />
      ),
    },
  ];

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success)
    return <AppErrorState message="Failed to load requests for assignment" />;

  const requests = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Assignment"
        subtitle="Assign open and approved requests to support users"
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
          value={params.status ?? STATUS.OPEN}
          onChange={(e) =>
            setParams((p) => ({
              ...p,
              status: e.target.value as string,
              pageNumber: 1,
            }))
          }
          options={assignableStatusOptions}
          placeholder="Filter by status"
        />
      </AppFilterBar>

      {/* Table */}
      <AppDataGrid
        columns={columns}
        rows={requests}
        keyField="requestId"
        onRowClick={(row) => navigate(`/requests/${row.requestId}`)}
        onSortChange={handleSortChange}
        sortBy={params.sortBy}
        sortAscending={params.sortAscending}
      />

      {/* Pagination */}
      <AppPagination
        pageNumber={pagination.pageNumber}
        pageSize={pagination.pageSize}
        totalCount={pagination.totalCount}
        totalPages={pagination.totalPages}
        onPageChange={(page) => setParams((p) => ({ ...p, pageNumber: page }))}
        onPageSizeChange={(size) =>
          setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }))
        }
      />

      {/* Assign Dialog */}
      <AssignSupportDialog
        open={assignOpen}
        onClose={() => {
          setAssignOpen(false);
          setSelectedRequest(null);
        }}
        supportUsers={supportUsers}
        onAssign={handleAssign}
        isSubmitting={assigning}
      />
    </Stack>
  );
};

export default AssignmentPage;
