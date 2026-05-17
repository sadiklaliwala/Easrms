import { useState } from 'react';
import { Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import toast from 'react-hot-toast';
import { useForm, Controller } from 'react-hook-form';
import { joiResolver } from '@hookform/resolvers/joi';
import Joi from 'joi';
import { Box } from '@mui/material';

import {
  useGetCategoriesQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useToggleCategoryStatusMutation,
} from '../../../store/api/category.endpoints';

import AppPageHeader from '../../../components/common/layout/AppPageHeader';
import AppButton from '../../../components/common/buttons/AppButton';
import AppSearchInput from '../../../components/common/filter/AppSearchInput';
import AppFilterBar from '../../../components/common/filter/AppFilterBar';
import AppDataGrid from '../../../components/common/table/AppDataGrid';
import AppPagination from '../../../components/common/table/AppPagination';
import AppModal from '../../../components/common/modal/AppModal';
import AppConfirmDialog from '../../../components/common/modal/AppConfirmDialog';
import AppStatusBadge from '../../../components/common/table/AppStatusBadge';
import AppTableActions from '../../../components/common/table/AppTableActions';
import AppLoader from '../../../components/common/feedback/AppLoader';
import AppErrorState from '../../../components/common/feedback/AppErrorState';
import AppLabel from '../../../components/common/form/AppLabel';
import AppInput from '../../../components/common/form/AppInput';
import AppFormError from '../../../components/common/form/AppFormError';
import AppCheckbox from '../../../components/common/form/AppCheckbox';

import type { CategoryListDto, CreateCategoryDto, UpdateCategoryDto, CategoryQueryParams } from '../../../types/category.types';
import type { GridColumn } from '../../../types/common.types';

// ─── Validation Schema ────────────────────────────────────────────────────────
const schema = Joi.object({
  categoryName: Joi.string().min(2).required().messages({
    'string.empty': 'Category name is required',
    'string.min': 'Must be at least 2 characters',
  }),
  isApprovalRequired: Joi.boolean().required(),
});

// ─── Component ────────────────────────────────────────────────────────────────
const CategoryPage = () => {
  const [params, setParams] = useState<CategoryQueryParams>({ pageNumber: 1, pageSize: 10 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editCategory, setEditCategory] = useState<CategoryListDto | null>(null);
  const [toggleCategory, setToggleCategory] = useState<CategoryListDto | null>(null);

  const { data: response, isLoading, isError } = useGetCategoriesQuery(params);
  const [createCategory, { isLoading: creating }] = useCreateCategoryMutation();
  const [updateCategory, { isLoading: updating }] = useUpdateCategoryMutation();
  const [toggleCategoryStatus, { isLoading: toggling }] = useToggleCategoryStatusMutation();

  // ─── Create Form ─────────────────────────────────────────────────────────────
  const {
    control: createControl,
    handleSubmit: handleCreateSubmit,
    reset: resetCreate,
    formState: { errors: createErrors },
  } = useForm<CreateCategoryDto>({
    resolver: joiResolver(schema),
    defaultValues: { categoryName: '', isApprovalRequired: false },
  });

  // ─── Edit Form ───────────────────────────────────────────────────────────────
  const {
    control: editControl,
    handleSubmit: handleEditSubmit,
    reset: resetEdit,
    formState: { errors: editErrors },
  } = useForm<UpdateCategoryDto>({
    resolver: joiResolver(schema),
  });

  // ─── Handlers ────────────────────────────────────────────────────────────────
  const handleCreate = async (data: CreateCategoryDto) => {
    try {
      const res = await createCategory(data).unwrap();
      if (res.success) {
        toast.success('Category created successfully');
        setCreateOpen(false);
        resetCreate();
      } else {
        toast.error(res.message ?? 'Failed to create category');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Failed to create category');
    }
  };

  const handleEdit = async (data: UpdateCategoryDto) => {
    if (!editCategory) return;
    try {
      const res = await updateCategory({ id: editCategory.categoryId, body: data }).unwrap();
      if (res.success) {
        toast.success('Category updated successfully');
        setEditCategory(null);
        resetEdit();
      } else {
        toast.error(res.message ?? 'Failed to update category');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Failed to update category');
    }
  };

  const handleToggle = async () => {
    if (!toggleCategory) return;
    try {
      const res = await toggleCategoryStatus(toggleCategory.categoryId).unwrap();
      if (res.success) {
        toast.success(`Category ${toggleCategory.isActive ? 'deactivated' : 'activated'} successfully`);
        setToggleCategory(null);
      } else {
        toast.error(res.message ?? 'Failed to update status');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Failed to update status');
    }
  };

  const openEdit = (cat: CategoryListDto) => {
    setEditCategory(cat);
    resetEdit({ categoryName: cat.categoryName, isApprovalRequired: cat.isApprovalRequired });
  };

  // ─── Table Columns ────────────────────────────────────────────────────────────
  const columns: GridColumn<CategoryListDto>[] = [
    { key: 'categoryName', label: 'Category Name' },
    {
      key: 'isApprovalRequired',
      label: 'Approval Required',
      render: (row) => (
        <AppStatusBadge status={row.isApprovalRequired ? 'Yes' : 'No'} />
      ),
    },
    {
      key: 'isActive',
      label: 'Status',
      render: (row) => (
        <AppStatusBadge status={row.isActive ? 'Active' : 'Inactive'} />
      ),
    },
    {
      key: 'actions',
      label: 'Actions',
      render: (row) => (
        <AppTableActions
          actions={[
            { label: 'Edit', onClick: () => openEdit(row) },
            {
              label: row.isActive ? 'Deactivate' : 'Activate',
              onClick: () => setToggleCategory(row),
              color: row.isActive ? 'error' : 'success',
            },
          ]}
        />
      ),
    },
  ];

  if (isLoading) return <AppLoader />;
  if (isError || !response?.success) return <AppErrorState message="Failed to load categories" />;

  const categories = response.data.items;
  const pagination = response.data.pagination;

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Category Management"
        subtitle="Manage request categories"
        actions={
          <AppButton
            label="Add Category"
            startIcon={<AddIcon />}
            onClick={() => setCreateOpen(true)}
          />
        }
      />

      {/* Filters */}
      <AppFilterBar>
        <AppSearchInput
          onSearch={(val) => setParams((p) => ({ ...p, search: val, pageNumber: 1 }))}
          placeholder="Search categories"
        />
      </AppFilterBar>

      {/* Table */}
      <AppDataGrid columns={columns} rows={categories} keyField="categoryId" />

      {/* Pagination */}
      <AppPagination
        page={pagination.pageNumber}
        pageSize={pagination.pageSize}
        total={pagination.totalCount}
        onPageChange={(page) => setParams((p) => ({ ...p, pageNumber: page }))}
        onPageSizeChange={(size) => setParams((p) => ({ ...p, pageSize: size, pageNumber: 1 }))}
      />

      {/* Create Modal */}
      <AppModal
        open={createOpen}
        onClose={() => { setCreateOpen(false); resetCreate(); }}
        title="Add New Category"
      >
        <Stack
          component="form"
          onSubmit={handleCreateSubmit(handleCreate)}
          spacing={2}
          noValidate
        >
          <CategoryFormFields control={createControl} errors={createErrors} />
          <AppButton label={creating ? 'Creating...' : 'Create Category'} type="submit" fullWidth disabled={creating} />
        </Stack>
      </AppModal>

      {/* Edit Modal */}
      <AppModal
        open={!!editCategory}
        onClose={() => { setEditCategory(null); resetEdit(); }}
        title="Edit Category"
      >
        <Stack
          component="form"
          onSubmit={handleEditSubmit(handleEdit)}
          spacing={2}
          noValidate
        >
          <CategoryFormFields control={editControl} errors={editErrors} />
          <AppButton label={updating ? 'Saving...' : 'Save Changes'} type="submit" fullWidth disabled={updating} />
        </Stack>
      </AppModal>

      {/* Toggle Confirm */}
      <AppConfirmDialog
        open={!!toggleCategory}
        title={toggleCategory?.isActive ? 'Deactivate Category' : 'Activate Category'}
        message={`Are you sure you want to ${toggleCategory?.isActive ? 'deactivate' : 'activate'} "${toggleCategory?.categoryName}"?`}
        confirmLabel={toggleCategory?.isActive ? 'Deactivate' : 'Activate'}
        confirmColor={toggleCategory?.isActive ? 'error' : 'success'}
        onConfirm={handleToggle}
        onCancel={() => setToggleCategory(null)}
        loading={toggling}
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
      <Controller name="categoryName" control={control} render={({ field }) => (
        <AppInput {...field} placeholder="Enter category name" fullWidth error={!!errors.categoryName} />
      )} />
      <AppFormError message={errors.categoryName?.message} />
    </Box>

    <Controller name="isApprovalRequired" control={control} render={({ field }) => (
      <AppCheckbox
        {...field}
        checked={field.value}
        label="Approval Required"
      />
    )} />
  </>
);

export default CategoryPage;