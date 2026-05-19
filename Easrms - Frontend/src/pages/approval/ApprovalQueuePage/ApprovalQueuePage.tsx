import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Stack } from "@mui/material";
import toast from "react-hot-toast";

import {
  useApproveOrRejectRequestMutation,
  useGetRequestsQuery,
} from "../../../store/api/request.endpoints";

import AppPageHeader from "../../../components/common/layout/AppPageHeader";
import AppSearchInput from "../../../components/common/filter/AppSearchInput";
import AppFilterBar from "../../../components/common/filter/AppFilterBar";
import AppDataGrid from "../../../components/common/table/AppDataGrid";
import AppPagination from "../../../components/common/table/AppPagination";
import AppStatusBadge from "../../../components/common/table/AppStatusBadge";
import AppPriorityBadge from "../../../components/common/table/AppPriorityBadge";
import AppTableActions from "../../../components/common/table/AppTableActions";
import AppLoader from "../../../components/common/feedback/AppLoader";
import AppErrorState from "../../../components/common/feedback/AppErrorState";
import ApprovalDialog from "../../../components/common/modal/ApprovalDialog";

import {
  STATUS,
  STATUS_ENUM_REVERSE,
} from "../../../constants/status.constants";
import { formatDate } from "../../../utils/formatDate";

import type {
  RequestListDto,
  RequestQueryParams,
  ApprovalRequestDto,
} from "../../../types/request.types";
import type { GridColumn } from "../../../types/common.types";
import { PRIORITY_LABEL } from "../../../constants/priority.constants";

// ─── Component ────────────────────────────────────────────────────────────────
const ApprovalQueuePage = () => {
  const navigate = useNavigate();

  const [params, setParams] = useState<RequestQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    status: STATUS.PENDING_APPROVAL,
  });

  const [selectedRequest, setSelectedRequest] = useState<RequestListDto | null>(
    null,
  );
  const [approvalOpen, setApprovalOpen] = useState(false);

  const { data: response, isLoading, isError } = useGetRequestsQuery(params);
  const [approveOrReject, { isLoading: approving }] =
    useApproveOrRejectRequestMutation();

  // ─── Handlers ─────────────────────────────────────────────────────────────────
  const handleApproval = async (data: ApprovalRequestDto) => {
    if (!selectedRequest) return;
    try {
      const res = await approveOrReject({
        id: selectedRequest.requestId,
        body: data,
      }).unwrap();
      if (res.success) {
        toast.success(
          `Request ${data.action === "Approve" ? "approved" : "rejected"} successfully`,
        );
        setApprovalOpen(false);
        setSelectedRequest(null);
      } else {
        toast.error(res.message ?? "Action failed");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Action failed");
    }
  };

  const openApprovalDialog = (row: RequestListDto) => {
    setSelectedRequest(row);
    setApprovalOpen(true);
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
        <AppStatusBadge
          status={STATUS_ENUM_REVERSE[row.status]?.toString() ?? "Unknown"}
        />
      ),
    },
    {
      key: "createdOn",
      label: "Submitted On",
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
            {
              label: "Approve / Reject",
              onClick: () => openApprovalDialog(row),
            },
          ]}
        />
      ),
    },
  ];

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success)
    return <AppErrorState message="Failed to load approval queue" />;

  const requests = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Approval Queue"
        subtitle="Review and approve or reject pending requests"
      />

      {/* Filters */}
      <AppFilterBar>
        <AppSearchInput
          onSearch={(val) =>
            setParams((p) => ({ ...p, search: val, pageNumber: 1 }))
          }
          placeholder="Search by request number or title"
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
        pageNumber={pagination.pageNumber}
        pageSize={pagination.pageSize}
        totalCount={pagination.totalCount}
        totalPages={pagination.totalPages}
        onPageChange={(page) => setParams((p) => ({ ...p, pageNumber: page }))}
        onPageSizeChange={(size) =>
          setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }))
        }
      />

      {/* Approval Dialog */}
      <ApprovalDialog
        open={approvalOpen}
        onClose={() => {
          setApprovalOpen(false);
          setSelectedRequest(null);
        }}
        onApprove={(comment) => handleApproval({ action: "Approve", comment })}
        onReject={(comment) => handleApproval({ action: "Reject", comment })}
        loading={approving}
      />
    </Stack>
  );
};

export default ApprovalQueuePage;
