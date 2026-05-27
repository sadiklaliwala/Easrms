import { useState } from "react";
import { Stack, IconButton, Box } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import toast from "react-hot-toast";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "../../../utils/appJoi";

import {
  useGetCategoriesQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useToggleCategoryStatusMutation,
  useBulkUploadCategoriesMutation,
  useDeleteCategoryMutation,
} from "../../../store/api/category.endpoints";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { ROLES } from "../../../constants/role.constants";

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
import { categoryBulkUploadConfig } from "../../../constants/bulkUpload.constants";
import AppErrorState from "../../../components/common/feedback/AppErrorState";
import AppLabel from "../../../components/common/form/AppLabel";
import AppInput from "../../../components/common/form/AppInput";
import AppFormError from "../../../components/common/form/AppFormError";
import AppCheckbox from "../../../components/common/form/AppCheckbox";
import AppSelect from "../../../components/common/form/AppSelect";

import type {
  CategoryListDto,
  CategoryQueryParams,
} from "../../../types/category.types";
import type { GridColumn } from "../../../types/common.types";

export interface CategoryFormType {
  categoryName: string;
  isApprovalRequired: boolean;
  slaValue: number;
  slaUnit: "hours" | "days" | "months";
}

const parseSlaHours = (
  hours: number,
): { slaValue: number; slaUnit: "hours" | "days" | "months" } => {
  if (hours > 0 && hours % 720 === 0) {
    return { slaValue: hours / 720, slaUnit: "months" };
  }
  if (hours > 0 && hours % 24 === 0) {
    return { slaValue: hours / 24, slaUnit: "days" };
  }
  return { slaValue: hours, slaUnit: "hours" };
};

const convertToHours = (
  value: number,
  unit: "hours" | "days" | "months",
): number => {
  const multipliers = { hours: 1, days: 24, months: 720 };
  return value * multipliers[unit];
};

// ─── Validation Schema ────────────────────────────────────────────────────────
const schema = Joi.object({
  categoryName: Joi.string().min(2).max(90).required().messages({
    "string.empty": "Category name is required",
    "string.min": "Must be at least 2 characters",
    "string.max": "Category name must not exceed 90 characters",
  }),
  isApprovalRequired: Joi.boolean().required(),
  slaValue: Joi.number().integer().positive().required().messages({
    "number.base": "SLA Value must be a number",
    "number.integer": "SLA Value must be an integer",
    "number.positive": "SLA Value must be greater than 0",
    "any.required": "SLA Value is required",
  }),
  slaUnit: Joi.string().valid("hours", "days", "months").required().messages({
    "any.required": "SLA Unit is required",
  }),
});

// ─── Component ────────────────────────────────────────────────────────────────
const CategoryPage = () => {
  const [params, setParams] = useState<CategoryQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: "createdOn",
    sortAscending: false,
  });
  const { roleName } = useAppSelector((state) => state.auth);
  const isAdmin = roleName === ROLES.ADMIN;

  const [createOpen, setCreateOpen] = useState(false);
  const [bulkOpen, setBulkOpen] = useState(false);
  const [editCategory, setEditCategory] = useState<CategoryListDto | null>(
    null,
  );
  const [toggleCategory, setToggleCategory] = useState<CategoryListDto | null>(
    null,
  );
  const [deleteCategoryRecord, setDeleteCategoryRecord] =
    useState<CategoryListDto | null>(null);

  const { data: response, isLoading, isError } = useGetCategoriesQuery(params);
  const [createCategory, { isLoading: creating }] = useCreateCategoryMutation();
  const [updateCategory, { isLoading: updating }] = useUpdateCategoryMutation();
  const [toggleCategoryStatus, { isLoading: toggling }] =
    useToggleCategoryStatusMutation();
  const [deleteCategory, { isLoading: deleting }] = useDeleteCategoryMutation();

  // ─── Create Form ─────────────────────────────────────────────────────────────
  const {
    control: createControl,
    handleSubmit: handleCreateSubmit,
    reset: resetCreate,
    setError: setCreateError,
    formState: { errors: createErrors },
  } = useForm<CategoryFormType>({
    resolver: joiResolver(schema),
    defaultValues: {
      categoryName: "",
      isApprovalRequired: false,
      slaValue: 24,
      slaUnit: "hours",
    },
  });

  // ─── Edit Form ───────────────────────────────────────────────────────────────
  const {
    control: editControl,
    handleSubmit: handleEditSubmit,
    reset: resetEdit,
    setError: setEditError,
    formState: { errors: editErrors },
  } = useForm<CategoryFormType>({
    resolver: joiResolver(schema),
  });

  // ─── Handlers ────────────────────────────────────────────────────────────────
  const handleCreate = async (formData: CategoryFormType) => {
    const totalHours = convertToHours(formData.slaValue, formData.slaUnit);
    if (totalHours > 100000) {
      setCreateError("slaValue", {
        type: "manual",
        message:
          "Total SLA cannot exceed 100,000 hours (e.g. 138 months or 4,166 days)",
      });
      return;
    }

    try {
      const res = await createCategory({
        categoryName: formData.categoryName,
        isApprovalRequired: formData.isApprovalRequired,
        slaHours: totalHours,
      }).unwrap();
      if (res.success) {
        toast.success("Category created successfully");
        setCreateOpen(false);
        resetCreate();
      } else {
        toast.error(res.message ?? "Failed to create category");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to create category");
    }
  };

  const handleEdit = async (formData: CategoryFormType) => {
    if (!editCategory) return;
    const totalHours = convertToHours(formData.slaValue, formData.slaUnit);
    if (totalHours > 100000) {
      setEditError("slaValue", {
        type: "manual",
        message:
          "Total SLA cannot exceed 100,000 hours (e.g. 138 months or 4,166 days)",
      });
      return;
    }

    try {
      const res = await updateCategory({
        id: editCategory.categoryId,
        body: {
          categoryName: formData.categoryName,
          isApprovalRequired: formData.isApprovalRequired,
          slaHours: totalHours,
        },
      }).unwrap();
      if (res.success) {
        toast.success("Category updated successfully");
        setEditCategory(null);
        resetEdit();
      } else {
        toast.error(res.message ?? "Failed to update category");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to update category");
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
    if (!toggleCategory) return;
    try {
      const res = await toggleCategoryStatus(
        toggleCategory.categoryId,
      ).unwrap();
      if (res.success) {
        toast.success(
          `Category ${toggleCategory.isActive ? "deactivated" : "activated"} successfully`,
        );
        setToggleCategory(null);
      } else {
        toast.error(res.message ?? "Failed to update status");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to update status");
    }
  };

  const handleDelete = async () => {
    if (!deleteCategoryRecord) return;
    try {
      const res = await deleteCategory(
        deleteCategoryRecord.categoryId,
      ).unwrap();
      if (res.success) {
        toast.success("Category deleted successfully");
        setDeleteCategoryRecord(null);
      } else {
        toast.error(res.message ?? "Failed to delete category");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to delete category");
    }
  };

  const openEdit = (cat: CategoryListDto) => {
    setEditCategory(cat);
    const parsedSla = parseSlaHours(cat.slaHours);
    resetEdit({
      categoryName: cat.categoryName,
      isApprovalRequired: cat.isApprovalRequired,
      slaValue: parsedSla.slaValue,
      slaUnit: parsedSla.slaUnit,
    });
  };

  // ─── Table Columns ────────────────────────────────────────────────────────────
  const columns: GridColumn<CategoryListDto>[] = [
    { key: "categoryName", label: "Category Name" },
    {
      key: "isApprovalRequired",
      label: "Approval Required",
      sortable: false,
      render: (row) => (
        <AppStatusBadge status={row.isApprovalRequired ? "Yes" : "No"} />
      ),
    },
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
      render: (row) => (
        <Stack sx={{ flexDirection: "row", gap: 1, alignItems: "center" }}>
          <AppTableActions
            actions={[
              { label: "Edit", onClick: () => openEdit(row) },
              {
                label: row.isActive ? "Deactivate" : "Activate",
                onClick: () => setToggleCategory(row),
                color: row.isActive ? "error" : "success",
              },
            ]}
          />
          {isAdmin && (
            <IconButton
              size="small"
              color="error"
              onClick={() => setDeleteCategoryRecord(row)}
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
    return <AppErrorState message="Failed to load categories" />;

  const categories = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Category Management"
        subtitle="Manage request categories"
        actions={
          <Stack direction="row" spacing={2}>
            {/* <AppButton
              label="Bulk upload"
              variant="outlined"
              startIcon={<FileUploadIcon />}
              onClick={() => setBulkOpen(true)}
            /> */}
            <AppButton
              label="Add Category"
              startIcon={<AddIcon />}
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
          placeholder="Search categories"
        />
      </AppFilterBar>

      {/* Table */}
      <AppDataGrid
        columns={columns}
        rows={categories}
        keyField="categoryId"
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

      {/* Create Modal */}
      <AppModal
        open={createOpen}
        onClose={() => {
          setCreateOpen(false);
          resetCreate();
        }}
        title="Add New Category"
      >
        <Stack
          component="form"
          onSubmit={handleCreateSubmit(handleCreate)}
          spacing={2}
          noValidate
        >
          <CategoryFormFields control={createControl} errors={createErrors} />
          <AppButton
            label={creating ? "Creating..." : "Create Category"}
            type="submit"
            fullWidth
            disabled={creating}
          />
        </Stack>
      </AppModal>

      {/* Edit Modal */}
      <AppModal
        open={!!editCategory}
        onClose={() => {
          setEditCategory(null);
          resetEdit();
        }}
        title="Edit Category"
      >
        <Stack
          component="form"
          onSubmit={handleEditSubmit(handleEdit)}
          spacing={2}
          noValidate
        >
          <CategoryFormFields control={editControl} errors={editErrors} />
          <AppButton
            label={updating ? "Saving..." : "Save Changes"}
            type="submit"
            fullWidth
            disabled={updating}
          />
        </Stack>
      </AppModal>

      {/* Toggle Confirm */}
      <AppConfirmDialog
        open={!!toggleCategory}
        title={
          toggleCategory?.isActive ? "Deactivate Category" : "Activate Category"
        }
        message={`Are you sure you want to ${toggleCategory?.isActive ? "deactivate" : "activate"} "${toggleCategory?.categoryName}"?`}
        confirmLabel={toggleCategory?.isActive ? "Deactivate" : "Activate"}
        confirmColor={toggleCategory?.isActive ? "error" : "success"}
        onConfirm={handleToggle}
        onClose={() => setToggleCategory(null)}
        isSubmitting={toggling}
      />

      {/* Delete Confirm Dialog */}
      <AppConfirmDialog
        open={!!deleteCategoryRecord}
        title="Delete Category"
        message="Are you sure you want to delete this category? Categories with active requests cannot be deleted."
        confirmLabel="Delete"
        confirmColor="error"
        onConfirm={handleDelete}
        onClose={() => setDeleteCategoryRecord(null)}
        isSubmitting={deleting}
      />

      <AppBulkUploadDialog
        open={bulkOpen}
        onClose={() => setBulkOpen(false)}
        config={{
          ...categoryBulkUploadConfig,
          uploadMutation: useBulkUploadCategoriesMutation,
        }}
      />
    </Stack>
  );
};

// ─── Shared Form Fields ───────────────────────────────────────────────────────
interface CategoryFormFieldsProps {
  control: any;
  errors: any;
}

const CategoryFormFields = ({ control, errors }: CategoryFormFieldsProps) => (
  <>
    <Box>
      <AppLabel label="Category Name" required />
      <Controller
        name="categoryName"
        control={control}
        render={({ field }) => (
          <AppInput
            {...field}
            placeholder="Enter category name"
            fullWidth
            error={!!errors.categoryName}
            maxLength={90}
          />
        )}
      />
      <AppFormError message={errors.categoryName?.message} />
    </Box>

    <Box sx={{ mt: 2, mb: 2 }}>
      <AppLabel label="SLA Duration" required />
      <Stack sx={{ flexDirection: "row", gap: 2, alignItems: "flex-start" }}>
        <Box sx={{ flex: 1 }}>
          <Controller
            name="slaValue"
            control={control}
            render={({ field }) => (
              <AppInput
                {...field}
                type="number"
                placeholder="Enter SLA value"
                fullWidth
                error={!!errors.slaValue}
                onChange={(e) =>
                  field.onChange(e.target.value ? Number(e.target.value) : "")
                }
              />
            )}
          />
          <AppFormError message={errors.slaValue?.message} />
        </Box>
        <Box sx={{ width: 150 }}>
          <Controller
            name="slaUnit"
            control={control}
            render={({ field }) => (
              <AppSelect
                {...field}
                options={[
                  { label: "Hours", value: "hours" },
                  { label: "Days", value: "days" },
                  { label: "Months", value: "months" },
                ]}
                fullWidth
                error={!!errors.slaUnit}
              />
            )}
          />
          <AppFormError message={errors.slaUnit?.message} />
        </Box>
      </Stack>
    </Box>

    <Controller
      name="isApprovalRequired"
      control={control}
      render={({ field }) => (
        <AppCheckbox
          {...field}
          checked={field.value}
          label="Approval Required"
        />
      )}
    />
  </>
);

export default CategoryPage;
