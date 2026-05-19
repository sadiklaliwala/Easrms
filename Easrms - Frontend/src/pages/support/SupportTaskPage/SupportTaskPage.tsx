import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Stack } from "@mui/material";
import toast from "react-hot-toast";

import {
  useGetRequestsQuery,
  useUpdateRequestStatusMutation,
} from "../../../store/api/request.endpoints";
// import { useAppSelector } from "../../../hooks/useAppSelector";

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
import UpdateStatusDialog from "../../../components/common/modal/UpdateStatusDialog";

import {
  STATUS,
  STATUS_ENUM,
  STATUS_ENUM_REVERSE,
} from "../../../constants/status.constants";
import { formatDate } from "../../../utils/formatDate";

import type {
  RequestListDto,
  RequestQueryParams,
} from "../../../types/request.types";
import type { GridColumn } from "../../../types/common.types";
import {
  // PRIORITY_ENUM, PRIORITY_ENUM_REVERSE,
  PRIORITY_LABEL,
} from "../../../constants/priority.constants";

// ─── Status filter options for support user ───────────────────────────────────
const SUPPORT_STATUS_OPTIONS = [
  { label: "All My Tasks", value: "" },
  { label: "Assigned", value: STATUS.ASSIGNED },
  { label: "In Progress", value: STATUS.IN_PROGRESS },
  { label: "Resolved", value: STATUS.RESOLVED },
];

// ─── Component ────────────────────────────────────────────────────────────────
const SupportTaskPage = () => {
  const navigate = useNavigate();
  // const { userId } = useAppSelector((state) => state.auth);

  const [params, setParams] = useState<RequestQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    status: STATUS.ASSIGNED,
  });

  const [selectedRequest, setSelectedRequest] = useState<RequestListDto | null>(
    null,
  );
  const [statusOpen, setStatusOpen] = useState(false);

  const { data: response, isLoading, isError } = useGetRequestsQuery(params);
  const [updateStatus, { isLoading: updatingStatus }] =
    useUpdateRequestStatusMutation();

  // ─── Handlers ─────────────────────────────────────────────────────────────────
  const handleStatusUpdate = async (newStatus: number, remarks: string) => {
    if (!selectedRequest) return;
    try {
      const res = await updateStatus({
        id: selectedRequest.requestId,
        body: { newStatus, remarks },
      }).unwrap();
      if (res.success) {
        toast.success("Status updated successfully");
        setStatusOpen(false);
        setSelectedRequest(null);
      } else {
        toast.error(res.message ?? "Status update failed");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Status update failed");
    }
  };

  const openStatusDialog = (row: RequestListDto) => {
    setSelectedRequest(row);
    setStatusOpen(true);
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
      key: "createdOn",
      label: "Created On",
      render: (row) => formatDate(row.createdOn),
    },
    {
      key: "actions",
      label: "Actions",
      render: (row) => (
        <AppTableActions
          actions={[
            {
              label: "View Details",
              onClick: () => navigate(`/requests/${row.requestId}`),
            },
            ...(row.status === STATUS_ENUM.ASSIGNED ||
            row.status === STATUS_ENUM.IN_PROGRESS
              ? [
                  {
                    label: "Update Status",
                    onClick: () => openStatusDialog(row),
                  },
                ]
              : []),
          ]}
        />
      ),
    },
  ];

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success)
    return <AppErrorState message="Failed to load your tasks" />;

  const requests = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="My Tasks"
        subtitle="View and update your assigned requests"
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
          value={params.status ?? STATUS.ASSIGNED}
          onChange={(e) =>
            setParams((p) => ({
              ...p,
              status: e.target.value as string,
              pageNumber: 1,
            }))
          }
          options={SUPPORT_STATUS_OPTIONS}
          placeholder="Filter by status"
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
        totalPages={pagination.totalPages}
        pageNumber={pagination.pageNumber}
        pageSize={pagination.pageSize}
        totalCount={pagination.totalCount}
        onPageChange={(page) => setParams((p) => ({ ...p, pageNumber: page }))}
        onPageSizeChange={(size) =>
          setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }))
        }
      />

      {/* Update Status Dialog */}
      <UpdateStatusDialog
        open={statusOpen}
        onClose={() => {
          setStatusOpen(false);
          setSelectedRequest(null);
        }}
        currentStatus={selectedRequest ? STATUS_ENUM_REVERSE[selectedRequest.status] : ""}
        onUpdate={handleStatusUpdate}
        isSubmitting={updatingStatus}
      />
    </Stack>
  );
};

export default SupportTaskPage;
