import { useNavigate } from "react-router-dom";
import { Stack, Box } from "@mui/material";
import toast from "react-hot-toast";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "joi";

import { useCreateRequestMutation } from "../../../store/api/request.endpoints";
import { useGetCategoriesQuery } from "../../../store/api/category.endpoints";

import AppPageHeader from "../../../components/common/layout/AppPageHeader";
import AppCard from "../../../components/common/layout/AppCard";
import AppLabel from "../../../components/common/form/AppLabel";
import AppInput from "../../../components/common/form/AppInput";
import AppTextArea from "../../../components/common/form/AppTextArea";
import AppSelect from "../../../components/common/form/AppSelect";
import AppFormError from "../../../components/common/form/AppFormError";
import AppLoadingButton from "../../../components/common/buttons/AppLoadingButton";
import AppButton from "../../../components/common/buttons/AppButton";

import {
  PRIORITY,
  PRIORITY_OPTIONS,
} from "../../../constants/priority.constants";
import type { CreateRequestDto } from "../../../types/request.types";

// ─── Validation Schema ────────────────────────────────────────────────────────
const schema = Joi.object({
  categoryId: Joi.string().uuid().required().messages({
    "string.empty": "Category is required",
    "any.required": "Category is required",
  }),
  title: Joi.string().min(3).max(200).required().messages({
    "string.empty": "Title is required",
    "string.min": "Title must be at least 3 characters",
    "string.max": "Title cannot exceed 200 characters",
  }),
  description: Joi.string().min(10).required().messages({
    "string.empty": "Description is required",
    "string.min": "Description must be at least 10 characters",
  }),
  priority: Joi.number()
    .valid(PRIORITY.LOW, PRIORITY.MEDIUM, PRIORITY.HIGH)
    .required()
    .messages({
      "any.required": "Priority is required",
      "any.only": "Select a valid priority",
    }),
});

// ─── Component ────────────────────────────────────────────────────────────────
const CreateRequestPage = () => {
  const navigate = useNavigate();

  const { data: categoriesResponse } = useGetCategoriesQuery({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const [createRequest, { isLoading }] = useCreateRequestMutation();

  const categoryOptions =
    categoriesResponse?.data?.items?.map((c) => ({
      label: c.categoryName,
      value: c.categoryId,
    })) ?? [];

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateRequestDto>({
    resolver: joiResolver(schema),
    defaultValues: {
      categoryId: "",
      title: "",
      description: "",
      priority: PRIORITY.LOW,
    },
  });

  const onSubmit = async (data: CreateRequestDto) => {
    try {
      const res = await createRequest(data).unwrap();
      if (res.success) {
        toast.success("Request created successfully");
        navigate("/requests");
      } else {
        toast.error(res.message ?? "Failed to create request");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Failed to create request");
    }
  };

  return (
    <Stack spacing={3}>
      <AppPageHeader
        title="Create New Request"
        subtitle="Fill in the details to raise an asset or service request"
      />

      <AppCard>
        <Stack
          component="form"
          onSubmit={handleSubmit(onSubmit)}
          spacing={3}
          noValidate
        >
          {/* Category */}
          <Box>
            <AppLabel label="Category" required />
            <Controller
              name="categoryId"
              control={control}
              render={({ field }) => (
                <AppSelect
                  {...field}
                  options={categoryOptions}
                  fullWidth
                  error={!!errors.categoryId}
                  placeholder="Select a category"
                />
              )}
            />
            <AppFormError message={errors.categoryId?.message} />
          </Box>

          {/* Title */}
          <Box>
            <AppLabel label="Title" required />
            <Controller
              name="title"
              control={control}
              render={({ field }) => (
                <AppInput
                  {...field}
                  placeholder="Enter a short title for your request"
                  fullWidth
                  error={!!errors.title}
                />
              )}
            />
            <AppFormError message={errors.title?.message} />
          </Box>

          {/* Priority */}
          <Box>
            <AppLabel label="Priority" required />
            <Controller
              name="priority"
              control={control}
              render={({ field }) => (
                <AppSelect
                  {...field}
                  options={PRIORITY_OPTIONS}
                  fullWidth
                  error={!!errors.priority}
                  placeholder="Select priority"
                />
              )}
            />
            <AppFormError message={errors.priority?.message} />
          </Box>

          {/* Description */}
          <Box>
            <AppLabel label="Description" required />
            <Controller
              name="description"
              control={control}
              render={({ field }) => (
                <AppTextArea
                  {...field}
                  placeholder="Describe your request in detail"
                  fullWidth
                  rows={5}
                  error={!!errors.description}
                />
              )}
            />
            <AppFormError message={errors.description?.message} />
          </Box>

          {/* Actions */}
          <Stack
            sx={{
              direction: "row",
              spacing: 2,
              justifyContent: "flex-end",
            }}
          >
            <AppButton
              label="Cancel"
              variant="outlined"
              color="inherit"
              onClick={() => navigate("/requests")}
            />
            <AppLoadingButton
              label="Submit Request"
              loading={isLoading}
              type="submit"
            />
          </Stack>
        </Stack>
      </AppCard>
    </Stack>
  );
};

export default CreateRequestPage;
