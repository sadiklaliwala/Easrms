// import { Grid, Stack, Typography } from "@mui/material";
// import DashboardIcon from "@mui/icons-material/Dashboard";
// import AssignmentIcon from "@mui/icons-material/Assignment";
// import PendingIcon from "@mui/icons-material/Pending";
// import CheckCircleIcon from "@mui/icons-material/CheckCircle";
// import CancelIcon from "@mui/icons-material/Cancel";
// import BuildIcon from "@mui/icons-material/Build";
// import ArchiveIcon from "@mui/icons-material/Archive";

// import { useGetDashboardSummaryQuery } from "../../../store/api/dashboard.endpoints";
// import { useAppSelector } from "../../../hooks/useAppSelector";

// import AppPageHeader from "../../../components/common/layout/AppPageHeader";
// import AppCard from "../../../components/common/layout/AppCard";
// import AppMetricCard from "../../../components/common/dashboard/AppMetricCard";
// import AppStatusChart from "../../../components/common/dashboard/AppStatusChart";
// import AppPriorityChart from "../../../components/common/dashboard/AppPriorityChart";
// import AppCategoryChart from "../../../components/common/dashboard/AppCategoryChart";
// import AppLoader from "../../../components/common/feedback/AppLoader";
// import AppErrorState from "../../../components/common/feedback/AppErrorState";
// import { STATUS } from "../../../constants/status.constants";
// import { PRIORITY_LABEL } from "../../../constants/priority.constants";

// const DashboardPage = () => {
//   const { roleName } = useAppSelector((state) => state.auth);
//   const { data: response, isLoading, isError } = useGetDashboardSummaryQuery();

//   if (isLoading) return <AppLoader />;
//   if (isError || !response?.success)
//     return <AppErrorState message="Failed to load dashboard data" />;

//   const data = response.data;
//   const priorityChartData = data.byPriority.map((p) => ({
//     priority: PRIORITY_LABEL[p.priority],
//     count: p.count,
//   }));

//   const statusChartData = data
//     ? [
//         {
//           status: STATUS.OPEN,
//           count: data.openCount,
//         },
//         {
//           status: STATUS.PENDING_APPROVAL,
//           count: data.pendingApprovalCount,
//         },
//         {
//           status: STATUS.APPROVED,
//           count: data.approvedCount,
//         },
//         {
//           status: STATUS.REJECTED,
//           count: data.rejectedCount,
//         },
//         {
//           status: STATUS.ASSIGNED,
//           count: data.assignedCount,
//         },
//         {
//           status: STATUS.IN_PROGRESS,
//           count: data.inProgressCount,
//         },
//         {
//           status: STATUS.RESOLVED,
//           count: data.resolvedCount,
//         },
//         {
//           status: STATUS.CLOSED,
//           count: data.closedCount,
//         },
//       ]
//     : [];

//   const metrics = [
//     {
//       title: "Total Requests",
//       count: data.totalRequests,
//       icon: <DashboardIcon />,
//       color: "#4f46e5", // Modern Indigo
//     },
//     {
//       title: "Open",
//       count: data.openCount,
//       icon: <AssignmentIcon />,
//       color: "#3b82f6", // Modern Blue
//     },
//     {
//       title: "Pending Approval",
//       count: data.pendingApprovalCount,
//       icon: <PendingIcon />,
//       color: "#f59e0b", // Modern Warning
//     },
//     {
//       title: "Approved",
//       count: data.approvedCount,
//       icon: <CheckCircleIcon />,
//       color: "#10b981", // Modern Success
//     },
//     {
//       title: "Assigned",
//       count: data.assignedCount,
//       icon: <BuildIcon />,
//       color: "#6366f1",
//     },
//     {
//       title: "In Progress",
//       count: data.inProgressCount,
//       icon: <BuildIcon />,
//       color: "#d97706",
//     },
//     {
//       title: "Resolved",
//       count: data.resolvedCount,
//       icon: <CheckCircleIcon />,
//       color: "#059669",
//     },
//     {
//       title: "Rejected",
//       count: data.rejectedCount,
//       icon: <CancelIcon />,
//       color: "#ef4444", // Modern Error
//     },
//     {
//       title: "Closed",
//       count: data.closedCount,
//       icon: <ArchiveIcon />,
//       color: "#64748b", // Modern Slate/Grey
//     },
//   ];
//   return (
//     <Stack spacing={4}>
//       <AppPageHeader
//         title="Dashboard"
//         subtitle={`Welcome, ${roleName} — here's your overview`}
//       />

//       {/* Metric Cards */}
//       <Grid container spacing={3}>
//         {metrics.map((metric) => (
//           <Grid
//             size={{
//               xs: 12,
//               sm: 6,
//               md: 4,
//               lg: 3,
//             }}
//             key={metric.title}
//           >
//             <AppMetricCard
//               title={metric.title}
//               count={metric.count}
//               icon={metric.icon}
//               color={metric.color}
//             />
//           </Grid>
//         ))}
//       </Grid>

//       {/* Charts */}
//       <Grid container spacing={3}>
//         <Grid size={{ xs: 12, md: 6 }}>
//           <AppCard sx={{ height: "100%", p: 1 }}>
//             <Typography
//               variant="subtitle1"
//               sx={{
//                 fontWeight: 600,
//                 mb: 3,
//                 color: "text.primary",
//               }}
//             >
//               Requests by Status
//             </Typography>

//             <AppStatusChart data={statusChartData} />
//           </AppCard>
//         </Grid>

//         <Grid size={{ xs: 12, md: 6 }}>
//           <AppCard sx={{ height: "100%", p: 1 }}>
//             <Typography
//               variant="subtitle1"
//               sx={{
//                 fontWeight: 600,
//                 mb: 3,
//                 color: "text.primary",
//               }}
//             >
//               Requests by Priority
//             </Typography>

//             <AppPriorityChart data={priorityChartData} />
//           </AppCard>
//         </Grid>

//         <Grid size={{ xs: 12 }}>
//           <AppCard sx={{ p: 1 }}>
//             <Typography
//               variant="subtitle1"
//               sx={{
//                 fontWeight: 600,
//                 mb: 3,
//                 color: "text.primary",
//               }}
//             >
//               Requests by Category
//             </Typography>

//             <AppCategoryChart data={data.byCategory} />
//           </AppCard>
//         </Grid>
//       </Grid>
//     </Stack>
//   );
// };

// export default DashboardPage;

import { Grid, Stack, Typography } from "@mui/material";
import DashboardIcon from "@mui/icons-material/Dashboard";
import AssignmentIcon from "@mui/icons-material/Assignment";
import PendingIcon from "@mui/icons-material/Pending";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import BuildIcon from "@mui/icons-material/Build";
import ArchiveIcon from "@mui/icons-material/Archive";

import {
  useGetDashboardSummaryQuery,
  useGetSLADashboardQuery,
} from "../../../store/api/dashboard.endpoints";
import { useAppSelector } from "../../../hooks/useAppSelector";

import AppPageHeader from "../../../components/common/layout/AppPageHeader";
import AppCard from "../../../components/common/layout/AppCard";
import AppMetricCard from "../../../components/common/dashboard/AppMetricCard";
import AppStatusChart from "../../../components/common/dashboard/AppStatusChart";
import AppPriorityChart from "../../../components/common/dashboard/AppPriorityChart";
import AppCategoryChart from "../../../components/common/dashboard/AppCategoryChart";
import AppSLACard from "../../../components/common/dashboard/AppSLACard";
import AppLoader from "../../../components/common/feedback/AppLoader";
import AppErrorState from "../../../components/common/feedback/AppErrorState";
import { STATUS } from "../../../constants/status.constants";
import { PRIORITY_LABEL } from "../../../constants/priority.constants";
import { ROLES } from "../../../constants/role.constants";

const DashboardPage = () => {
  const { roleName } = useAppSelector((state) => state.auth);
  const { data: response, isLoading, isError } = useGetDashboardSummaryQuery();
  const isSlaRole = roleName === ROLES.ADMIN || roleName === ROLES.MANAGER;
  const {
    data: slaResponse,
    isLoading: slaLoading,
    isError: slaError,
  } = useGetSLADashboardQuery(undefined, { skip: !isSlaRole });

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success)
    return <AppErrorState message="Failed to load dashboard data" />;

  const data = response.data;

  const priorityChartData = data.byPriority.map((p) => ({
    priority: PRIORITY_LABEL[p.priority],
    count: p.count,
  }));

  const statusChartData = [
    { status: STATUS.OPEN, count: data.openCount },
    { status: STATUS.PENDING_APPROVAL, count: data.pendingApprovalCount },
    { status: STATUS.APPROVED, count: data.approvedCount },
    { status: STATUS.REJECTED, count: data.rejectedCount },
    { status: STATUS.ASSIGNED, count: data.assignedCount },
    { status: STATUS.IN_PROGRESS, count: data.inProgressCount },
    { status: STATUS.RESOLVED, count: data.resolvedCount },
    { status: STATUS.CLOSED, count: data.closedCount },
  ];

  // Assigned user chart data — map to { name, count } for bar chart
  const assignedUserChartData = (data.byAssignedUser || []).map((u) => ({
    name: u.fullName,
    count: u.count,
  }));

  const metrics = [
    {
      title: "Total Requests",
      count: data.totalRequests,
      icon: <DashboardIcon />,
      color: "#4f46e5",
    },
    {
      title: "Open",
      count: data.openCount,
      icon: <AssignmentIcon />,
      color: "#3b82f6",
    },
    {
      title: "Pending Approval",
      count: data.pendingApprovalCount,
      icon: <PendingIcon />,
      color: "#f59e0b",
    },
    {
      title: "Approved",
      count: data.approvedCount,
      icon: <CheckCircleIcon />,
      color: "#10b981",
    },
    {
      title: "Assigned",
      count: data.assignedCount,
      icon: <BuildIcon />,
      color: "#6366f1",
    },
    {
      title: "In Progress",
      count: data.inProgressCount,
      icon: <BuildIcon />,
      color: "#d97706",
    },
    {
      title: "Resolved",
      count: data.resolvedCount,
      icon: <CheckCircleIcon />,
      color: "#059669",
    },
    {
      title: "Rejected",
      count: data.rejectedCount,
      icon: <CancelIcon />,
      color: "#ef4444",
    },
    {
      title: "Closed",
      count: data.closedCount,
      icon: <ArchiveIcon />,
      color: "#64748b",
    },
  ];

  return (
    <Stack spacing={4}>
      <AppPageHeader
        title="Dashboard"
        subtitle={`Welcome, ${roleName} — here's your overview`}
      />

      {/* SLA Card Row */}
      {(roleName === ROLES.ADMIN || roleName === ROLES.MANAGER) && (
        <Grid container spacing={3}>
          <Grid size={{ xs: 12 }}>
            {slaLoading ? (
              <AppLoader />
            ) : slaError || !slaResponse?.success ? (
              <AppErrorState message="Failed to load SLA data" />
            ) : (
              <AppSLACard
                withinSLA={slaResponse.data.withinSLACount}
                nearingBreach={slaResponse.data.nearingBreachCount}
                breached={slaResponse.data.breachedCount}
                escalated={slaResponse.data.escalatedCount}
              />
            )}
          </Grid>
        </Grid>
      )}

      {/* Metric Cards */}
      <Grid container spacing={3}>
        {metrics.map((metric) => (
          <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={metric.title}>
            <AppMetricCard
              title={metric.title}
              count={metric.count}
              icon={metric.icon}
              color={metric.color}
            />
          </Grid>
        ))}
      </Grid>

      {/* Charts Row 1 — Status + Priority */}
      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 6 }}>
          <AppCard sx={{ height: "100%", p: 1 }}>
            <Typography
              variant="subtitle1"
              sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
            >
              Requests by Status Checks
            </Typography>
            <AppStatusChart data={statusChartData} />
          </AppCard>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <AppCard sx={{ height: "100%", p: 1 }}>
            <Typography
              variant="subtitle1"
              sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
            >
              Requests by Priority
            </Typography>
            <AppPriorityChart data={priorityChartData} />
          </AppCard>
        </Grid>
      </Grid>

      {/* Charts Row 2 — Category + Assigned User */}
      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: roleName === ROLES.EMPLOYEE ? 12 : 6 }}>
          <AppCard sx={{ p: 1 }}>
            <Typography
              variant="subtitle1"
              sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
            >
              Requests by Category
            </Typography>
            <AppCategoryChart data={data.byCategory} />
          </AppCard>
        </Grid>

        {roleName !== ROLES.EMPLOYEE && (
          <Grid size={{ xs: 12, md: 6 }}>
            <AppCard sx={{ p: 1 }}>
              <Typography
                variant="subtitle1"
                sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
              >
                Requests by Assigned User
              </Typography>
              {assignedUserChartData.length === 0 ? (
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ textAlign: "center", py: 4 }}
                >
                  No assigned requests yet
                </Typography>
              ) : (
                <AppCategoryChart
                  data={assignedUserChartData.map((u) => ({
                    categoryName: u.name,
                    count: u.count,
                  }))}
                />
              )}
            </AppCard>
          </Grid>
        )}
      </Grid>
    </Stack>
  );
};

export default DashboardPage;

// import { useMemo } from "react";
// import { Grid, Stack, Typography } from "@mui/material";
// import DashboardIcon from "@mui/icons-material/Dashboard";
// import AssignmentIcon from "@mui/icons-material/Assignment";
// import PendingIcon from "@mui/icons-material/Pending";
// import CheckCircleIcon from "@mui/icons-material/CheckCircle";
// import CancelIcon from "@mui/icons-material/Cancel";
// import BuildIcon from "@mui/icons-material/Build";
// import ArchiveIcon from "@mui/icons-material/Archive";

// import { useGetDashboardSummaryQuery } from "../../../store/api/dashboard.endpoints";
// import { useAppSelector } from "../../../hooks/useAppSelector";

// import AppPageHeader from "../../../components/common/layout/AppPageHeader";
// import AppCard from "../../../components/common/layout/AppCard";
// import AppMetricCard from "../../../components/common/dashboard/AppMetricCard";
// import AppStatusChart from "../../../components/common/dashboard/AppStatusChart";
// import AppPriorityChart from "../../../components/common/dashboard/AppPriorityChart";
// import AppCategoryChart from "../../../components/common/dashboard/AppCategoryChart";
// import AppLoader from "../../../components/common/feedback/AppLoader";
// import AppErrorState from "../../../components/common/feedback/AppErrorState";
// import { STATUS } from "../../../constants/status.constants";
// import { PRIORITY_LABEL } from "../../../constants/priority.constants";
// import { ROLES } from "../../../constants/role.constants";

// const DashboardPage = () => {
//   const { roleName } = useAppSelector((state) => state.auth);
//   const { data: response, isLoading, isError } = useGetDashboardSummaryQuery();

//   if (isLoading) return <AppLoader />;
//   if (isError || !response?.success)
//     return <AppErrorState message="Failed to load dashboard data" />;

//   const data = response.data;

//   // useMemo — prevents recomputing chart data on every render
//   const priorityChartData = useMemo(
//     () =>
//       data.byPriority.map((p) => ({
//         priority: PRIORITY_LABEL[p.priority],
//         count: p.count,
//       })),
//     [data.byPriority],
//   );

//   // useMemo — status chart array rebuilt only when data changes
//   const statusChartData = useMemo(
//     () => [
//       { status: STATUS.OPEN, count: data.openCount },
//       { status: STATUS.PENDING_APPROVAL, count: data.pendingApprovalCount },
//       { status: STATUS.APPROVED, count: data.approvedCount },
//       { status: STATUS.REJECTED, count: data.rejectedCount },
//       { status: STATUS.ASSIGNED, count: data.assignedCount },
//       { status: STATUS.IN_PROGRESS, count: data.inProgressCount },
//       { status: STATUS.RESOLVED, count: data.resolvedCount },
//       { status: STATUS.CLOSED, count: data.closedCount },
//     ],
//     [
//       data.openCount,
//       data.pendingApprovalCount,
//       data.approvedCount,
//       data.rejectedCount,
//       data.assignedCount,
//       data.inProgressCount,
//       data.resolvedCount,
//       data.closedCount,
//     ],
//   );

//   // useMemo — assigned user chart data rebuilt only when byAssignedUser changes
//   const assignedUserChartData = useMemo(
//     () =>
//       (data.byAssignedUser || []).map((u) => ({
//         categoryName: u.fullName,
//         count: u.count,
//       })),
//     [data.byAssignedUser],
//   );

//   // useMemo — metrics array rebuilt only when counts change
//   const metrics = useMemo(
//     () => [
//       {
//         title: "Total Requests",
//         count: data.totalRequests,
//         icon: <DashboardIcon />,
//         color: "#4f46e5",
//       },
//       {
//         title: "Open",
//         count: data.openCount,
//         icon: <AssignmentIcon />,
//         color: "#3b82f6",
//       },
//       {
//         title: "Pending Approval",
//         count: data.pendingApprovalCount,
//         icon: <PendingIcon />,
//         color: "#f59e0b",
//       },
//       {
//         title: "Approved",
//         count: data.approvedCount,
//         icon: <CheckCircleIcon />,
//         color: "#10b981",
//       },
//       {
//         title: "Assigned",
//         count: data.assignedCount,
//         icon: <BuildIcon />,
//         color: "#6366f1",
//       },
//       {
//         title: "In Progress",
//         count: data.inProgressCount,
//         icon: <BuildIcon />,
//         color: "#d97706",
//       },
//       {
//         title: "Resolved",
//         count: data.resolvedCount,
//         icon: <CheckCircleIcon />,
//         color: "#059669",
//       },
//       {
//         title: "Rejected",
//         count: data.rejectedCount,
//         icon: <CancelIcon />,
//         color: "#ef4444",
//       },
//       {
//         title: "Closed",
//         count: data.closedCount,
//         icon: <ArchiveIcon />,
//         color: "#64748b",
//       },
//     ],
//     [
//       data.totalRequests,
//       data.openCount,
//       data.pendingApprovalCount,
//       data.approvedCount,
//       data.assignedCount,
//       data.inProgressCount,
//       data.resolvedCount,
//       data.rejectedCount,
//       data.closedCount,
//     ],
//   );

//   return (
//     <Stack spacing={4}>
//       <AppPageHeader
//         title="Dashboard"
//         subtitle={`Welcome, ${roleName} — here's your overview`}
//       />

//       {/* Metric Cards */}
//       <Grid container spacing={3}>
//         {metrics.map((metric) => (
//           <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={metric.title}>
//             <AppMetricCard
//               title={metric.title}
//               count={metric.count}
//               icon={metric.icon}
//               color={metric.color}
//             />
//           </Grid>
//         ))}
//       </Grid>

//       {/* Charts Row 1 — Status + Priority */}
//       <Grid container spacing={3}>
//         <Grid size={{ xs: 12, md: 6 }}>
//           <AppCard sx={{ height: "100%", p: 1 }}>
//             <Typography
//               variant="subtitle1"
//               sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
//             >
//               Requests by Status
//             </Typography>
//             <AppStatusChart data={statusChartData} />
//           </AppCard>
//         </Grid>

//         <Grid size={{ xs: 12, md: 6 }}>
//           <AppCard sx={{ height: "100%", p: 1 }}>
//             <Typography
//               variant="subtitle1"
//               sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
//             >
//               Requests by Priority
//             </Typography>
//             <AppPriorityChart data={priorityChartData} />
//           </AppCard>
//         </Grid>
//       </Grid>

//       {/* Charts Row 2 — Category + Assigned User */}
//       <Grid container spacing={3}>
//         <Grid size={{ xs: 12, md: roleName === ROLES.EMPLOYEE ? 12 : 6 }}>
//           <AppCard sx={{ p: 1 }}>
//             <Typography
//               variant="subtitle1"
//               sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
//             >
//               Requests by Category
//             </Typography>
//             <AppCategoryChart data={data.byCategory} />
//           </AppCard>
//         </Grid>

//         {roleName !== ROLES.EMPLOYEE && (
//           <Grid size={{ xs: 12, md: 6 }}>
//             <AppCard sx={{ p: 1 }}>
//               <Typography
//                 variant="subtitle1"
//                 sx={{ fontWeight: 600, mb: 3, color: "text.primary" }}
//               >
//                 Requests by Assigned User
//               </Typography>
//               {assignedUserChartData.length === 0 ? (
//                 <Typography
//                   variant="body2"
//                   color="text.secondary"
//                   sx={{ textAlign: "center", py: 4 }}
//                 >
//                   No assigned requests yet
//                 </Typography>
//               ) : (
//                 <AppCategoryChart data={assignedUserChartData} />
//               )}
//             </AppCard>
//           </Grid>
//         )}
//       </Grid>
//     </Stack>
//   );
// };

// export default DashboardPage;
