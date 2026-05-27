import { useState, useEffect } from "react";
import { Stack, IconButton } from "@mui/material";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import DeleteIcon from "@mui/icons-material/Delete";
import toast from "react-hot-toast";

import {
  useGetUsersQuery,
  useGetUserByIdQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useToggleUserStatusMutation,
  useBulkUploadUsersMutation,
  useDeleteUserMutation,
} from "../../../store/api/user.endpoints";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { useGetManagersQuery } from "../../../store/api/lookup.endpoints";

import AppPageHeader from "../../../components/common/layout/AppPageHeader";
import AppButton from "../../../components/common/buttons/AppButton";
import AppSearchInput from "../../../components/common/filter/AppSearchInput";
import AppFilterBar from "../../../components/common/filter/AppFilterBar";
import AppDataGrid from "../../../components/common/table/AppDataGrid";
import AppPagination from "../../../components/common/table/AppPagination";
import AppModal from "../../../components/common/modal/AppModal";
import AppConfirmDialog from "../../../components/common/modal/AppConfirmDialog";
import AppBulkUploadDialog from "../../../components/common/modal/AppBulkUploadDialog";
import AppStatusBadge from "../../../components/common/table/AppStatusBadge";
import AppTableActions from "../../../components/common/table/AppTableActions";
import AppLoader from "../../../components/common/feedback/AppLoader";
import AppErrorState from "../../../components/common/feedback/AppErrorState";
import AppLabel from "../../../components/common/form/AppLabel";
import AppInput from "../../../components/common/form/AppInput";
import AppFormError from "../../../components/common/form/AppFormError";
import AppSelect from "../../../components/common/form/AppSelect";
import AppPasswordInput from "../../../components/common/form/AppPasswordInput";
import { userBulkUploadConfig } from "../../../constants/bulkUpload.constants";

import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "../../../utils/appJoi";
import { Box } from "@mui/material";

import type {
  UserListDto,
  CreateUserDto,
  UpdateUserDto,
  UserQueryParams,
} from "../../../types/user.types";
import type { GridColumn } from "../../../types/common.types";
import { ROLES } from "../../../constants/role.constants";

// ─── Role options (hardcoded as per SRS) ─────────────────────────────────────
const ROLE_OPTIONS = [
  { label: ROLES.ADMIN, value: "90A5EB56-0DDC-4187-9F32-8870F8FC7043" },
  { label: ROLES.MANAGER, value: "90A5EB56-0DDC-4187-9F32-8870F8FC7044" },
  { label: ROLES.EMPLOYEE, value: "90A5EB56-0DDC-4187-9F32-8870F8FC7045" },
  { label: ROLES.SUPPORT_USER, value: "90A5EB56-0DDC-4187-9F32-8870F8FC7046" },
];

// ─── Validation Schemas ───────────────────────────────────────────────────────
const createSchema = Joi.object({
  fullName: Joi.string().min(2).max(90).required().messages({
    "string.empty": "Full name is required",
    "string.max": "Full name must not exceed 90 characters",
  }),
  email: Joi.string()
    .email({ tlds: { allow: false } })
    .max(140)
    .required()
    .messages({
      "string.empty": "Email is required",
      "string.email": "Enter a valid email",
      "string.max": "Email must not exceed 140 characters",
    }),
  password: Joi.string().min(6).max(255).required().messages({
    "string.empty": "Password is required",
    "string.min": "Password must be at least 6 characters",
    "string.max": "Password must not exceed 255 characters",
  }),
  roleId: Joi.string()
    .uuid()
    .required()
    .messages({ "string.empty": "Role is required" }),
  managerId: Joi.string().uuid().allow(null, "").optional(),
});

const updateSchema = Joi.object({
  fullName: Joi.string().min(2).max(90).required().messages({
    "string.empty": "Full name is required",
    "string.max": "Full name must not exceed 90 characters",
  }),
  email: Joi.string()
    .email({ tlds: { allow: false } })
    .max(140)
    .required()
    .messages({
      "string.empty": "Email is required",
      "string.email": "Enter a valid email",
      "string.max": "Email must not exceed 140 characters",
    }),
  roleId: Joi.string()
    .uuid()
    .required()
    .messages({ "string.empty": "Role is required" }),
  managerId: Joi.string().uuid().allow(null, "").optional(),
});

// ─── Component ────────────────────────────────────────────────────────────────
const UserPage = () => {
  const [params, setParams] = useState<UserQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: "createdOn",
    sortAscending: false,
  });
  const { roleName, userId: currentUserId } = useAppSelector(
    (state) => state.auth,
  );
  const isAdmin = roleName === ROLES.ADMIN;

  const [createOpen, setCreateOpen] = useState(false);
  const [bulkOpen, setBulkOpen] = useState(false);
  const [editUser, setEditUser] = useState<UserListDto | null>(null);
  const [toggleUser, setToggleUser] = useState<UserListDto | null>(null);
  const [deleteUserRecord, setDeleteUserRecord] = useState<UserListDto | null>(
    null,
  );

  const { data: response, isLoading, isError } = useGetUsersQuery(params);
  const { data: managersResponse } = useGetManagersQuery();
  const [createUser, { isLoading: creating }] = useCreateUserMutation();
  const [updateUser, { isLoading: updating }] = useUpdateUserMutation();
  const [toggleUserStatus, { isLoading: toggling }] =
    useToggleUserStatusMutation();
  const [deleteUser, { isLoading: deleting }] = useDeleteUserMutation();

  const managers =
    managersResponse?.data?.map((m) => ({
      label: m.fullName,
      value: m.userId,
    })) ?? [];

  // ─── Create Form ────────────────────────────────────────────────────────────
  const {
    control: createControl,
    handleSubmit: handleCreateSubmit,
    reset: resetCreate,
    formState: { errors: createErrors },
  } = useForm<CreateUserDto>({ resolver: joiResolver(createSchema) });

  const {
    control: editControl,
    handleSubmit: handleEditSubmit,
    reset: resetEdit,
    formState: { errors: editErrors },
  } = useForm<UpdateUserDto>({ resolver: joiResolver(updateSchema) });

  const { data: userDetailResponse } = useGetUserByIdQuery(
    editUser?.userId ?? "",
    { skip: !editUser },
  );

  useEffect(() => {
    if (userDetailResponse?.success && userDetailResponse.data && editUser) {
      const detail = userDetailResponse.data;
      const matchedRole = ROLE_OPTIONS.find(
        (r) => r.label.toLowerCase() === detail.roleName.toLowerCase(),
      );
      resetEdit({
        fullName: detail.fullName,
        email: detail.email,
        roleId: matchedRole ? matchedRole.value : "",
        managerId: detail.managerId || "",
      });
    }
  }, [userDetailResponse, editUser, resetEdit]);

  // ─── Handlers ───────────────────────────────────────────────────────────────
  const handleCreate = async (data: CreateUserDto) => {
    try {
      const res = await createUser(data).unwrap();
      if (res.success) {
        toast.success("User created successfully");
        setCreateOpen(false);
        resetCreate();
      } else {
        toast.error(res.message ?? "Failed to create user");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to create user");
    }
  };

  const handleEdit = async (data: UpdateUserDto) => {
    if (!editUser) return;
    try {
      const res = await updateUser({
        id: editUser.userId,
        body: data,
      }).unwrap();
      if (res.success) {
        toast.success("User updated successfully");
        setEditUser(null);
        resetEdit();
      } else {
        toast.error(res.message ?? "Failed to update user");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to update user");
    }
  };

  const handleSortChange = (sortBy: string, sortAscending: boolean) => {
    setParams((prev) => ({
      ...prev,
      sortBy,
      sortAscending,
      pageNumber: 1,
    }));
  };

  const handleToggle = async () => {
    if (!toggleUser) return;
    try {
      const res = await toggleUserStatus(toggleUser.userId).unwrap();
      if (res.success) {
        toast.success(
          `User ${toggleUser.isActive ? "deactivated" : "activated"} successfully`,
        );
        setToggleUser(null);
      } else {
        toast.error(res.message ?? "Failed to update status");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to update status");
    }
  };

  const handleDelete = async () => {
    if (!deleteUserRecord) return;
    try {
      const res = await deleteUser(deleteUserRecord.userId).unwrap();
      if (res.success) {
        toast.success("User deleted successfully");
        setDeleteUserRecord(null);
      } else {
        toast.error(res.message ?? "Failed to delete user");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to delete user");
    }
  };

  const openEdit = (user: UserListDto) => {
    setEditUser(user);
    const matchedRole = ROLE_OPTIONS.find(
      (r) => r.label.toLowerCase() === user.roleName.toLowerCase(),
    );
    resetEdit({
      fullName: user.fullName,
      email: user.email,
      roleId: matchedRole ? matchedRole.value : "",
      managerId: "",
    });
  };

  // ─── Table Columns ───────────────────────────────────────────────────────────
  const columns: GridColumn<UserListDto>[] = [
    { key: "fullName", label: "Full Name" },
    { key: "email", label: "Email" },
    { key: "roleName", label: "Role", sortable: false },
    {
      key: "isActive",
      label: "Status",
      sortable: false,
      render: (row) => (
        <AppStatusBadge status={row.isActive ? "Active" : "Inactive"} />
      ),
    },
    {
      key: "actions",
      label: "Actions",
      sortable: false,
      render: (row: UserListDto) => (
        <Stack sx={{ flexDirection: "row", alignItems: "center" }} spacing={1}>
          <AppTableActions
            actions={[
              { label: "Edit", onClick: () => openEdit(row) },
              {
                label: row.isActive ? "Deactivate" : "Activate",
                onClick: () => setToggleUser(row),
                color: row.isActive ? "error" : "success",
              },
            ]}
          />
          {isAdmin && row.userId !== currentUserId && (
            <IconButton
              size="small"
              color="error"
              onClick={() => setDeleteUserRecord(row)}
            >
              <DeleteIcon fontSize="small" />
            </IconButton>
          )}
        </Stack>
      ),
    },
  ];

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success)
    return <AppErrorState message="Failed to load users" />;

  const users = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="User Management"
        subtitle="Manage system users and their roles"
        actions={
          <Stack direction="row" spacing={2}>
            <AppButton
              label="Bulk upload"
              variant="outlined"
              startIcon={<FileUploadIcon />}
              onClick={() => setBulkOpen(true)}
            />
            <AppButton
              label="Add User"
              startIcon={<PersonAddIcon />}
              onClick={() => setCreateOpen(true)}
            />
          </Stack>
        }
      />

      {/* Filters */}
      <AppFilterBar>
        <AppSearchInput
          onSearch={(val) =>
            setParams((p) => ({ ...p, search: val, pageNumber: 1 }))
          }
          placeholder="Search by name or email"
        />
      </AppFilterBar>

      {/* Table */}
      <AppDataGrid
        columns={columns}
        rows={users}
        keyField="userId"
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
        onPageChange={(page) => setParams((p) => ({ ...p, pageNumber: page }))}
        onPageSizeChange={(size) =>
          setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }))
        }
      />

      {/* Create Modal */}
      <AppModal
        open={createOpen}
        onClose={() => {
          setCreateOpen(false);
          resetCreate();
        }}
        title="Add New User"
      >
        <Stack
          component="form"
          onSubmit={handleCreateSubmit(handleCreate)}
          spacing={2}
          noValidate
        >
          <UserFormFields
            control={createControl}
            errors={createErrors}
            managers={managers}
            showPassword
          />
          <AppButton
            label={creating ? "Creating..." : "Create User"}
            type="submit"
            fullWidth
            disabled={creating}
          />
        </Stack>
      </AppModal>

      {/* Edit Modal */}
      <AppModal
        open={!!editUser}
        onClose={() => {
          setEditUser(null);
          resetEdit();
        }}
        title="Edit User"
      >
        <Stack
          component="form"
          onSubmit={handleEditSubmit(handleEdit)}
          spacing={2}
          noValidate
        >
          <UserFormFields
            control={editControl}
            errors={editErrors}
            managers={managers}
            showPassword={false}
          />
          <AppButton
            label={updating ? "Saving..." : "Save Changes"}
            type="submit"
            fullWidth
            disabled={updating}
          />
        </Stack>
      </AppModal>

      {/* Toggle Confirm Dialog */}
      <AppConfirmDialog
        open={!!toggleUser}
        title={toggleUser?.isActive ? "Deactivate User" : "Activate User"}
        message={`Are you sure you want to ${toggleUser?.isActive ? "deactivate" : "activate"} ${toggleUser?.fullName}?`}
        confirmLabel={toggleUser?.isActive ? "Deactivate" : "Activate"}
        confirmColor={toggleUser?.isActive ? "error" : "success"}
        onConfirm={handleToggle}
        onClose={() => setToggleUser(null)}
        isSubmitting={toggling}
      />

      {/* Delete Confirm Dialog */}
      <AppConfirmDialog
        open={!!deleteUserRecord}
        title="Delete User"
        message="Are you sure you want to delete this user? This action cannot be undone."
        confirmLabel="Delete"
        confirmColor="error"
        onConfirm={handleDelete}
        onClose={() => setDeleteUserRecord(null)}
        isSubmitting={deleting}
      />

      <AppBulkUploadDialog
        open={bulkOpen}
        onClose={() => setBulkOpen(false)}
        config={{
          ...userBulkUploadConfig,
          uploadMutation: useBulkUploadUsersMutation,
        }}
      />
    </Stack>
  );
};

// ─── Shared Form Fields ───────────────────────────────────────────────────────
interface UserFormFieldsProps {
  control: any;
  errors: any;
  managers: { label: string; value: string }[];
  showPassword: boolean;
}

const UserFormFields = ({
  control,
  errors,
  managers,
  showPassword,
}: UserFormFieldsProps) => {
  const roleOptions = ROLE_OPTIONS;

  return (
    <>
      <Box>
        <AppLabel label="Full Name" required />
        <Controller
          name="fullName"
          control={control}
          render={({ field }) => (
            <AppInput
              {...field}
              placeholder="Enter full name"
              fullWidth
              error={!!errors.fullName}
              maxLength={90}
            />
          )}
        />
        <AppFormError message={errors.fullName?.message} />
      </Box>

      <Box>
        <AppLabel label="Email" required />
        <Controller
          name="email"
          control={control}
          render={({ field }) => (
            <AppInput
              {...field}
              placeholder="Enter email"
              type="email"
              fullWidth
              error={!!errors.email}
              maxLength={140}
            />
          )}
        />
        <AppFormError message={errors.email?.message} />
      </Box>

      {showPassword && (
        <Box>
          <AppLabel label="Password" required />
          <Controller
            name="password"
            control={control}
            render={({ field }) => (
              <AppPasswordInput
                {...field}
                placeholder="Enter password"
                fullWidth
                error={!!errors.password}
                maxLength={255}
              />
            )}
          />
          <AppFormError message={errors.password?.message} />
        </Box>
      )}

      <Box>
        <AppLabel label="Role" required />
        <Controller
          name="roleId"
          control={control}
          render={({ field }) => (
            <AppSelect
              {...field}
              options={roleOptions}
              fullWidth
              error={!!errors.roleId}
            />
          )}
        />
        <AppFormError message={errors.roleId?.message} />
      </Box>

      <Box>
        <AppLabel label="Manager" />
        <Controller
          name="managerId"
          control={control}
          render={({ field }) => (
            <AppSelect
              {...field}
              options={managers}
              fullWidth
              placeholder="Select manager (Default will be logged in Admin)"
            />
          )}
        />
      </Box>
    </>
  );
};

export default UserPage;
