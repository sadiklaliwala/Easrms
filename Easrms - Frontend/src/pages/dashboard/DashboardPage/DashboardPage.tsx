import { Grid, Stack, Typography } from "@mui/material";
import DashboardIcon from "@mui/icons-material/Dashboard";
import AssignmentIcon from "@mui/icons-material/Assignment";
import PendingIcon from "@mui/icons-material/Pending";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import BuildIcon from "@mui/icons-material/Build";
import ArchiveIcon from "@mui/icons-material/Archive";

import { useGetDashboardSummaryQuery } from "../../../store/api/dashboard.endpoints";
import { useAppSelector } from "../../../hooks/useAppSelector";

import AppPageHeader from "../../../components/common/layout/AppPageHeader";
import AppMetricCard from "../../../components/common/dashboard/AppMetricCard";
import AppStatusChart from "../../../components/common/dashboard/AppStatusChart";
import AppPriorityChart from "../../../components/common/dashboard/AppPriorityChart";
import AppCategoryChart from "../../../components/common/dashboard/AppCategoryChart";
import AppLoader from "../../../components/common/feedback/AppLoader";
import AppErrorState from "../../../components/common/feedback/AppErrorState";
import { STATUS } from "../../../constants/status.constants";
import { PRIORITY_ENUM_REVERSE } from "../../../constants/priority.constants";

const DashboardPage = () => {
  const { roleName } = useAppSelector((state) => state.auth);
  const { data: response, isLoading, isError } = useGetDashboardSummaryQuery();

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success)
    return <AppErrorState message="Failed to load dashboard data" />;

  const data = response.data;
  const priorityChartData = data.byPriority.map((p) => ({
    priority: PRIORITY_ENUM_REVERSE[p.priority],
    count: p.count,
  }));

  const statusChartData = data
    ? [
        {
          status: STATUS.OPEN,
          count: data.openCount,
        },
        {
          status: STATUS.PENDING_APPROVAL,
          count: data.pendingApprovalCount,
        },
        {
          status: STATUS.APPROVED,
          count: data.approvedCount,
        },
        {
          status: STATUS.REJECTED,
          count: data.rejectedCount,
        },
        {
          status: STATUS.ASSIGNED,
          count: data.assignedCount,
        },
        {
          status: STATUS.IN_PROGRESS,
          count: data.inProgressCount,
        },
        {
          status: STATUS.RESOLVED,
          count: data.resolvedCount,
        },
        {
          status: STATUS.CLOSED,
          count: data.closedCount,
        },
      ]
    : [];

  const metrics = [
    {
      title: "Total Requests",
      count: data.totalRequests,
      icon: <DashboardIcon />,
      color: "#1976d2",
    },
    {
      title: "Open",
      count: data.openCount,
      icon: <AssignmentIcon />,
      color: "#ed6c02",
    },
    {
      title: "Pending Approval",
      count: data.pendingApprovalCount,
      icon: <PendingIcon />,
      color: "#9c27b0",
    },
    {
      title: "Approved",
      count: data.approvedCount,
      icon: <CheckCircleIcon />,
      color: "#2e7d32",
    },
    {
      title: "Assigned",
      count: data.assignedCount,
      icon: <BuildIcon />,
      color: "#0288d1",
    },
    {
      title: "In Progress",
      count: data.inProgressCount,
      icon: <BuildIcon />,
      color: "#f57c00",
    },
    {
      title: "Resolved",
      count: data.resolvedCount,
      icon: <CheckCircleIcon />,
      color: "#388e3c",
    },
    {
      title: "Rejected",
      count: data.rejectedCount,
      icon: <CancelIcon />,
      color: "#d32f2f",
    },
    {
      title: "Closed",
      count: data.closedCount,
      icon: <ArchiveIcon />,
      color: "#616161",
    },
  ];

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Dashboard"
        subtitle={`Welcome, ${roleName} — here's your overview`}
      />

      {/* Metric Cards */}
      <Grid container spacing={2}>
        {metrics.map((metric) => (
          <Grid
            sx={{ item: { xs: 12, sm: 6, md: 4, lg: 3 } }}
            key={metric.title}
          >
            <AppMetricCard
              title={metric.title}
              count={metric.count}
              icon={metric.icon}
              color={metric.color}
            />
          </Grid>
        ))}
      </Grid>

      {/* Charts */}
      <Grid container spacing={3}>
        <Grid sx={{ item: { xs: 12, md: 6 } }}>
          <Typography sx={{ fontWeight: 600, mb: 1 }} variant="subtitle1">
            Requests by Status
          </Typography>
          <AppStatusChart data={statusChartData} />
        </Grid>

        <Grid sx={{ item: { xs: 12, md: 6 } }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
            Requests by Priority
          </Typography>
          <AppPriorityChart data={priorityChartData} />
        </Grid>

        <Grid sx={{ item: { xs: 12 } }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
            Requests by Category
          </Typography>
          <AppCategoryChart data={data.byCategory} />
        </Grid>
      </Grid>
    </Stack>
  );
};

export default DashboardPage;
