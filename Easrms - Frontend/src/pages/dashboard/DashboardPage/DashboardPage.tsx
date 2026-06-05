import { useState, useMemo } from "react";
import {
  Grid,
  Stack,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Avatar,
  TextField,
  InputAdornment,
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import SearchIcon from "@mui/icons-material/Search";
import DashboardIcon from "@mui/icons-material/Dashboard";
import AssignmentIcon from "@mui/icons-material/Assignment";
import PendingIcon from "@mui/icons-material/Pending";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import BuildIcon from "@mui/icons-material/Build";
import ArchiveIcon from "@mui/icons-material/Archive";
import PeopleIcon from "@mui/icons-material/People";

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
  const navigate = useNavigate();
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

  const [employeeSearch, setEmployeeSearch] = useState("");

  const filteredEmployees = useMemo(() => {
    if (!data.managedEmployees) return [];
    return data.managedEmployees.filter(
      (emp) =>
        emp.fullName?.toLowerCase().includes(employeeSearch.toLowerCase()) ||
        emp.email?.toLowerCase().includes(employeeSearch.toLowerCase()),
    );
  }, [data.managedEmployees, employeeSearch]);

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

  if (roleName === ROLES.MANAGER && data.managedEmployeesCount !== undefined) {
    metrics.push({
      title: "Managed Employees",
      count: data.managedEmployeesCount ?? 0,
      icon: <PeopleIcon />,
      color: "#ec4899",
    });
  }

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

      {/* Managed Employees List for Manager */}
      {roleName === ROLES.MANAGER &&
        data.managedEmployees &&
        data.managedEmployees.length > 0 && (
          <AppCard sx={{ p: 3 }}>
            <Stack
              direction={{ xs: "column", sm: "row" }}
              sx={{
                spacing: 2,
                justifyContent: "space-between",
                alignItems: { xs: "stretch", sm: "center" },
                mb: 2,
              }}
            >
              <Typography
                variant="h6"
                sx={{ fontWeight: 600, color: "text.primary" }}
              >
                My Team / Managed Employees
              </Typography>

              <TextField
                size="small"
                placeholder="Search employees..."
                value={employeeSearch}
                onChange={(e) => setEmployeeSearch(e.target.value)}
                sx={{ width: { xs: "100%", sm: 250 } }}
                slotProps={{
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon fontSize="small" />
                      </InputAdornment>
                    ),
                  },
                }}
              />
            </Stack>
            <TableContainer sx={{ mt: 1 }}>
              <Table>
                <TableHead>
                  <TableRow sx={{ borderBottom: 1, borderColor: "divider" }}>
                    <TableCell sx={{ fontWeight: 600, px: 2, py: 1.5 }}>
                      Name
                    </TableCell>
                    <TableCell sx={{ fontWeight: 600, px: 2, py: 1.5 }}>
                      Email
                    </TableCell>
                    <TableCell sx={{ fontWeight: 600, px: 2, py: 1.5 }}>
                      Status
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredEmployees.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={3}
                        align="center"
                        sx={{ py: 3, color: "text.secondary" }}
                      >
                        No employees found matching "{employeeSearch}"
                      </TableCell>
                    </TableRow>
                  ) : (
                    filteredEmployees.map((emp) => (
                      <TableRow
                        key={emp.userId}
                        hover
                        onClick={() =>
                          navigate("/requests", {
                            state: { filterEmployeeName: emp.fullName },
                          })
                        }
                        sx={{
                          cursor: "pointer",
                          "&:hover": {
                            backgroundColor: "action.hover",
                          },
                        }}
                      >
                        <TableCell sx={{ px: 2, py: 1.5 }}>
                          <Stack
                            sx={{
                              alignItems: "center",
                            }}
                            direction="row"
                            spacing={2}
                          >
                            <Avatar
                              sx={{
                                width: 32,
                                height: 32,
                                fontSize: "0.875rem",
                                fontWeight: 600,
                                bgcolor: "primary.main",
                                color: "primary.contrastText",
                              }}
                            >
                              {emp.fullName
                                ? emp.fullName
                                    .split(" ")
                                    .filter(Boolean)
                                    .map((n: string) => n[0])
                                    .join("")
                                    .toUpperCase()
                                : "U"}
                            </Avatar>
                            <Typography
                              variant="body2"
                              sx={{ fontWeight: 500, color: "text.primary" }}
                            >
                              {emp.fullName}
                            </Typography>
                          </Stack>
                        </TableCell>
                        <TableCell
                          sx={{ px: 2, py: 1.5, color: "text.secondary" }}
                        >
                          {emp.email}
                        </TableCell>
                        <TableCell sx={{ px: 2, py: 1.5 }}>
                          <span
                            style={{
                              display: "inline-block",
                              padding: "4px 8px",
                              borderRadius: "4px",
                              fontSize: "0.75rem",
                              fontWeight: 600,
                              backgroundColor: emp.isActive
                                ? "rgba(16, 185, 129, 0.15)"
                                : "rgba(239, 68, 68, 0.15)",
                              color: emp.isActive ? "#10b981" : "#ef4444",
                            }}
                          >
                            {emp.isActive ? "Active" : "Inactive"}
                          </span>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </AppCard>
        )}
    </Stack>
  );
};

export default DashboardPage;
