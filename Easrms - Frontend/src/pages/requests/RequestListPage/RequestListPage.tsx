// import { useState } from "react";
// import { useNavigate } from "react-router-dom";
// import { Stack } from "@mui/material";
// import AddIcon from "@mui/icons-material/Add";

// import { useGetRequestsQuery } from "../../../store/api/request.endpoints";
// import { useGetCategoriesQuery } from "../../../store/api/category.endpoints";
// import { useAppSelector } from "../../../hooks/useAppSelector";

// import AppPageHeader from "../../../components/common/layout/AppPageHeader";
// import AppButton from "../../../components/common/buttons/AppButton";
// import AppSearchInput from "../../../components/common/filter/AppSearchInput";
// import AppFilterBar from "../../../components/common/filter/AppFilterBar";
// import AppSelect from "../../../components/common/form/AppSelect";
// import AppDataGrid from "../../../components/common/table/AppDataGrid";
// import AppPagination from "../../../components/common/table/AppPagination";
// import AppStatusBadge from "../../../components/common/table/AppStatusBadge";
// import AppPriorityBadge from "../../../components/common/table/AppPriorityBadge";
// import AppLoader from "../../../components/common/feedback/AppLoader";
// import AppErrorState from "../../../components/common/feedback/AppErrorState";

// import { ROLES } from "../../../constants/role.constants";
// import {
//   // STATUS_ENUM,
//   STATUS_ENUM_REVERSE,
//   STATUS_OPTIONS,
// } from "../../../constants/status.constants";
// import {
//   // PRIORITY_ENUM,
//   // PRIORITY_ENUM_REVERSE,
//   PRIORITY_LABEL,
//   PRIORITY_OPTIONS,
// } from "../../../constants/priority.constants";
// import { formatDate } from "../../../utils/formatDate";

// import type {
//   RequestListDto,
//   RequestQueryParams,
// } from "../../../types/request.types";
// import type { GridColumn } from "../../../types/common.types";

// // ─── Component ────────────────────────────────────────────────────────────────
// const RequestListPage = () => {
//   const navigate = useNavigate();
//   const { roleName } = useAppSelector((state) => state.auth);

//   const [params, setParams] = useState<RequestQueryParams>({
//     pageNumber: 1,
//     pageSize: 10,
//   });

//   const { data: response, isLoading, isError } = useGetRequestsQuery(params);
//   const { data: categoriesResponse } = useGetCategoriesQuery({
//     pageNumber: 1,
//     pageSize: 100,
//     isActive: true,
//   });

//   const categoryOptions = [
//     { label: "All Categories", value: "" },
//     ...(categoriesResponse?.data?.items?.map((c) => ({
//       label: c.categoryName,
//       value: c.categoryId,
//     })) ?? []),
//   ];

//   const statusOptions = [
//     { label: "All Statuses", value: "" },
//     ...STATUS_OPTIONS,
//   ];
//   const priorityOptions = [
//     { label: "All Priorities", value: "" },
//     ...PRIORITY_OPTIONS,
//   ];

//   // ─── Table Columns ────────────────────────────────────────────────────────────
//   const columns: GridColumn<RequestListDto>[] = [
//     {
//       key: "requestNumber",
//       label: "Request #",
//       render: (row) => (
//         <span style={{ fontFamily: "monospace", fontWeight: 600 }}>
//           {row.requestNumber}
//         </span>
//       ),
//     },
//     { key: "title", label: "Title" },
//     { key: "categoryName", label: "Category" },
//     {
//       key: "priority",
//       label: "Priority",
//       render: (row) => (
//         <AppPriorityBadge priority={PRIORITY_LABEL[row.priority]} />
//       ),
//     },
//     {
//       key: "status",
//       label: "Status",
//       render: (row) => (
//         <AppStatusBadge status={STATUS_ENUM_REVERSE[row.status].toString()} />
//       ),
//     },
//     { key: "assigneeName", label: "Assignee" },
//     {
//       key: "createdOn",
//       label: "Created On",
//       render: (row) => formatDate(row.createdOn),
//     },
//   ];

//   if (isLoading) return <AppLoader />;
//   if (isError || !response?.success)
//     return <AppErrorState message="Failed to load requests" />;

//   const requests = response.data.items;
//   const pagination = response.data.pagination;

//   return (
//     <Stack spacing={3}>
//       <AppPageHeader
//         title="Service Requests"
//         subtitle="View and manage all requests"
//         actions={
//           roleName === ROLES.EMPLOYEE ? (
//             <AppButton
//               label="New Request"
//               startIcon={<AddIcon />}
//               onClick={() => navigate("/requests/create")}
//             />
//           ) : undefined
//         }
//       />

//       {/* Filters */}
//       <AppFilterBar>
//         <AppSearchInput
//           onSearch={(val) =>
//             setParams((p) => ({ ...p, search: val, pageNumber: 1 }))
//           }
//           placeholder="Search by request number or title"
//         />
//         <AppSelect
//           value={params.status ?? ""}
//           onChange={(e) =>
//             setParams((p) => ({
//               ...p,
//               status: e.target.value as string,
//               pageNumber: 1,
//             }))
//           }
//           options={statusOptions}
//           placeholder="Status"
//         />
//         <AppSelect
//           value={params.priority ?? ""}
//           onChange={(e) =>
//             setParams((p) => ({
//               ...p,
//               priority: e.target.value ? Number(e.target.value) : undefined,
//               pageNumber: 1,
//             }))
//           }
//           options={priorityOptions}
//           placeholder="Priority"
//         />
//         <AppSelect
//           value={params.categoryId ?? ""}
//           onChange={(e) =>
//             setParams((p) => ({
//               ...p,
//               categoryId: e.target.value as string,
//               pageNumber: 1,
//             }))
//           }
//           options={categoryOptions}
//           placeholder="Category"
//         />
//       </AppFilterBar>

//       {/* Table */}
//       <AppDataGrid
//         columns={columns}
//         rows={requests}
//         keyField="requestId"
//         onRowClick={(row) => navigate(`/requests/${row.requestId}`)}
//       />

//       {/* Pagination */}
//       <AppPagination
//         totalPages={pagination.totalPages}
//         pageNumber={pagination.pageNumber}
//         pageSize={pagination.pageSize}
//         totalCount={pagination.totalCount}
//         onPageChange={(page) => setParams((p) => ({ ...p, pageNumber: page }))}
//         onPageSizeChange={(size) =>
//           setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }))
//         }
//       />
//     </Stack>
//   );
// };

// export default RequestListPage;

import { useState, useMemo, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { Box, Stack } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";


import { useGetRequestsQuery } from "../../../store/api/request.endpoints";
import { useGetCategoriesQuery } from "../../../store/api/category.endpoints";
import { useAppSelector } from "../../../hooks/useAppSelector";

import AppPageHeader from "../../../components/common/layout/AppPageHeader";
import AppButton from "../../../components/common/buttons/AppButton";
import AppSearchInput from "../../../components/common/filter/AppSearchInput";
import AppFilterBar from "../../../components/common/filter/AppFilterBar";
import AppSelect from "../../../components/common/form/AppSelect";
import AppDataGrid from "../../../components/common/table/AppDataGrid";
import AppPagination from "../../../components/common/table/AppPagination";
import AppStatusBadge from "../../../components/common/table/AppStatusBadge";
import AppPriorityBadge from "../../../components/common/table/AppPriorityBadge";
import AppLoader from "../../../components/common/feedback/AppLoader";
import AppErrorState from "../../../components/common/feedback/AppErrorState";
import AppTableActions from "../../../components/common/table/AppTableActions";
import AppSLABadge from "../../../components/common/table/AppSLABadge";
import EscalateRequestDialog from "../../../components/common/modal/EscalateRequestDialog";
import AppBulkUploadDialog from "../../../components/common/modal/AppBulkUploadDialog";
import {
  useEscalateRequestMutation,
  useLazyExportRequestListExcelQuery,
  useLazyExportRequestListPdfQuery,
  useBulkUploadRequestsMutation,
} from "../../../store/api/request.endpoints";
import toast from "react-hot-toast";
import { downloadBlob } from "../../../utils/exportFile";
import { STATUS } from "../../../constants/status.constants";
import { requestBulkUploadConfig } from "../../../constants/bulkUpload.constants";

import { ROLES } from "../../../constants/role.constants";
import {
  STATUS_ENUM_REVERSE,
  STATUS_OPTIONS,
} from "../../../constants/status.constants";
import {
  PRIORITY_LABEL,
  PRIORITY_OPTIONS,
} from "../../../constants/priority.constants";
import { formatDate } from "../../../utils/formatDate";

import type {
  RequestListDto,
  RequestQueryParams,
} from "../../../types/request.types";
import type { GridColumn } from "../../../types/common.types";

// ─── Component ────────────────────────────────────────────────────────────────
const RequestListPage = () => {
  const navigate = useNavigate();
  const { roleName } = useAppSelector((state) => state.auth);

  const [params, setParams] = useState<RequestQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: "createdOn",
    sortAscending: false,
  });

  const { data: response, isLoading, isError } = useGetRequestsQuery(params);
  const { data: categoriesResponse } = useGetCategoriesQuery({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const [escalateRow, setEscalateRow] = useState<RequestListDto | null>(null);
  const [bulkOpen, setBulkOpen] = useState(false);
  const [escalateRequest, { isLoading: escalating }] =
    useEscalateRequestMutation();

  const [exportExcel, { isFetching: excelLoading }] =
    useLazyExportRequestListExcelQuery();
  const [exportPdf, { isFetching: pdfLoading }] =
    useLazyExportRequestListPdfQuery();

  const handleExportExcel = async () => {
    try {
      const blob = await exportExcel(params).unwrap();
      downloadBlob(blob, `Requests_${Date.now()}.xlsx`);
      toast.success("Excel exported successfully");
    } catch (error) {
      toast.error("Failed to export Excel");
    }
  };

  const handleExportPdf = async () => {
    try {
      const blob = await exportPdf(params).unwrap();
      downloadBlob(blob, `Requests_${Date.now()}.pdf`);
      toast.success("PDF exported successfully");
    } catch (error) {
      toast.error("Failed to export PDF");
    }
  };

  // useMemo — category options rebuilt only when categories response changes
  const categoryOptions = useMemo(
    () => [
      { label: "All Categories", value: "" },
      ...(categoriesResponse?.data?.items?.map((c) => ({
        label: c.categoryName,
        value: c.categoryId,
      })) ?? []),
    ],
    [categoriesResponse?.data?.items],
  );

  // useMemo — static-ish options, no need to recreate every render
  const statusOptions = useMemo(
    () => [{ label: "All Statuses", value: "" }, ...STATUS_OPTIONS],
    [],
  );

  const priorityOptions = useMemo(
    () => [{ label: "All Priorities", value: "" }, ...PRIORITY_OPTIONS],
    [],
  );

  // ─── Table Columns ─────────────────────────────────────────────────────────
  // useMemo — columns array won't recreate on every render
  const columns: GridColumn<RequestListDto>[] = useMemo(
    () => [
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
      { key: "assigneeName", label: "Assignee" },
      {
        key: "createdOn",
        label: "Created On",
        render: (row) => formatDate(row.createdOn),
      },
      {
        key: "dueDate",
        label: "Due Date",
        render: (row) => (row.dueDate ? formatDate(row.dueDate) : "N/A"),
      },
      {
        key: "slaStatus",
        label: "SLA Status",
        sortable: false,
        render: (row) => <AppSLABadge slaStatus={row.slaStatus} />,
      },
      {
        key: "isEscalated",
        label: "Escalated",
        render: (row) => (row.isEscalated ? "Yes" : "No"),
      },
      {
        key: "actions",
        label: "Actions",
        sortable: false,
        render: (row) => {
          const statusLabel = STATUS_ENUM_REVERSE[row.status].toString();
          const canEscalate =
            roleName === ROLES.ADMIN &&
            statusLabel !== STATUS.CLOSED &&
            statusLabel !== STATUS.REJECTED &&
            !row.isEscalated;
          if (!canEscalate) return null;
          return (
            <AppTableActions
              actions={[
                {
                  label: "Escalate",
                  onClick: (e: any) => {
                    e.stopPropagation();
                    setEscalateRow(row);
                  },
                  color: "error",
                },
              ]}
            />
          );
        },
      },
    ],
    [roleName],
  );

  // useCallback — stable handler for search, won't recreate on every render
  // const handleSearch = useCallback((val: string) => {
  //   setParams((p) => ({ ...p, search: val, pageNumber: 1 }));
  // }, []);

  // // useCallback — stable handler for status filter
  // const handleStatusChange = useCallback(
  //   (e: React.ChangeEvent<HTMLInputElement>) => {
  //     setParams((p) => ({
  //       ...p,
  //       status: e.target.value as string,
  //       pageNumber: 1,
  //     }));
  //   },
  //   [],
  // );

  // // useCallback — stable handler for priority filter
  // const handlePriorityChange = useCallback(
  //   (e: React.ChangeEvent<HTMLInputElement>) => {
  //     setParams((p) => ({
  //       ...p,
  //       priority: e.target.value ? Number(e.target.value) : undefined,
  //       pageNumber: 1,
  //     }));
  //   },
  //   [],
  // );

  // // useCallback — stable handler for category filter
  // const handleCategoryChange = useCallback(
  //   (e: React.ChangeEvent<HTMLInputElement>) => {
  //     setParams((p) => ({
  //       ...p,
  //       categoryId: e.target.value as string,
  //       pageNumber: 1,
  //     }));
  //   },
  //   [],
  // );

  // useCallback — stable handler for row click
  const handleRowClick = useCallback(
    (row: RequestListDto) => {
      navigate(`/requests/${row.requestId}`);
    },
    [navigate],
  );

  // useCallback — stable handlers for pagination
  const handlePageChange = useCallback((page: number) => {
    setParams((p) => ({ ...p, pageNumber: page }));
  }, []);

  const handlePageSizeChange = useCallback((size: number) => {
    setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }));
  }, []);

  const handleSortChange = useCallback(
    (sortBy: string, sortAscending: boolean) => {
      setParams((p) => ({
        ...p,
        sortBy,
        sortAscending,
        pageNumber: 1,
      }));
    },
    [],
  );

  const handleEscalate = async (reason: string) => {
    if (!escalateRow) return;
    try {
      const res = await escalateRequest({
        id: escalateRow.requestId,
        body: { escalationReason: reason },
      }).unwrap();
      if (res.success) {
        toast.success("Request escalated successfully");
        setEscalateRow(null);
      } else {
        toast.error(res.message ?? "Failed to escalate request");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to escalate request");
    }
  };

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success)
    return <AppErrorState message="Failed to load requests" />;

  const requests = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Service Requests"
        subtitle="View and manage all requests"
        actions={
          <Stack direction="row" spacing={2}>
            {/* <AppButton
              label="Bulk upload"
              variant="outlined"
              startIcon={<FileUploadIcon />}
              onClick={() => setBulkOpen(true)}
            /> */}
            {roleName === ROLES.EMPLOYEE && (
              <AppButton
                label="New Request"
                startIcon={<AddIcon />}
                onClick={() => navigate("/requests/create")}
              />
            )}
          </Stack>
        }
      />

      {/* Filters */}
      {/* <AppFilterBar>
        <AppSearchInput
          onSearch={handleSearch}
          placeholder="Search by request number or title"
        />
        <AppSelect
          value={params.status ?? ""}
          onChange={handleStatusChange}
          options={statusOptions}
          placeholder="Status"
        />
        <AppSelect
          value={params.priority ?? ""}
          onChange={handlePriorityChange}
          options={priorityOptions}
          placeholder="Priority"
        />
        <AppSelect
          value={params.categoryId ?? ""}
          onChange={handleCategoryChange}
          options={categoryOptions}
          placeholder="Category"
        />
      </AppFilterBar> */}

      {/* Filters */}
      <AppFilterBar>
        {roleName === ROLES.ADMIN && (
          <Box sx={{ display: "flex", gap: 1, minWidth: "max-content" }}>
            <AppButton
              label={excelLoading ? "Exporting..." : "Export Excel"}
              onClick={handleExportExcel}
              disabled={excelLoading || pdfLoading}
              variant="outlined"
            />
            <AppButton
              label={pdfLoading ? "Exporting..." : "Export PDF"}
              onClick={handleExportPdf}
              disabled={excelLoading || pdfLoading}
              variant="outlined"
            />
          </Box>
        )}
        <Box sx={{ flex: 1, minWidth: 200 }}>
          <AppSearchInput
            onSearch={(val) =>
              setParams((p) => ({ ...p, search: val, pageNumber: 1 }))
            }
            placeholder="Search by request number or title"
          />
        </Box>
        <Box sx={{ minWidth: 160 }}>
          <AppSelect
            value={params.status ?? ""}
            onChange={(e) =>
              setParams((p) => ({
                ...p,
                status: e.target.value as string,
                pageNumber: 1,
              }))
            }
            options={statusOptions}
            placeholder="Status"
          />
        </Box>
        <Box sx={{ minWidth: 160 }}>
          <AppSelect
            value={params.priority ?? ""}
            onChange={(e) =>
              setParams((p) => ({
                ...p,
                priority: e.target.value ? Number(e.target.value) : undefined,
                pageNumber: 1,
              }))
            }
            options={priorityOptions}
            placeholder="Priority"
          />
        </Box>
        <Box sx={{ minWidth: 160 }}>
          <AppSelect
            value={params.categoryId ?? ""}
            onChange={(e) =>
              setParams((p) => ({
                ...p,
                categoryId: e.target.value as string,
                pageNumber: 1,
              }))
            }
            options={categoryOptions}
            placeholder="Category"
          />
        </Box>
      </AppFilterBar>

      {/* Table */}
      <AppDataGrid
        columns={columns}
        rows={requests}
        keyField="requestId"
        onRowClick={handleRowClick}
        onSortChange={handleSortChange}
        sortBy={params.sortBy}
        sortAscending={params.sortAscending}
      />

      {/* Pagination */}
      <AppPagination
        totalPages={pagination.totalPages}
        pageNumber={pagination.pageNumber}
        pageSize={pagination.pageSize}
        totalCount={pagination.totalCount}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />

      <EscalateRequestDialog
        open={!!escalateRow}
        onClose={() => setEscalateRow(null)}
        requestNumber={escalateRow?.requestNumber || ""}
        dueDate={escalateRow?.dueDate ? formatDate(escalateRow.dueDate) : null}
        onEscalate={handleEscalate}
        isSubmitting={escalating}
      />

      <AppBulkUploadDialog
        open={bulkOpen}
        onClose={() => setBulkOpen(false)}
        config={{
          ...requestBulkUploadConfig,
          uploadMutation: useBulkUploadRequestsMutation,
        }}
      />
    </Stack>
  );
};

export default RequestListPage;
