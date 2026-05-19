import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Box, Typography } from "@mui/material";
import toast from "react-hot-toast";
import Stack from "@mui/material/Stack";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "joi";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import AttachFileIcon from "@mui/icons-material/AttachFile";

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
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

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

          {/* Attachment (Placeholder only) */}
          <Box>
            <AppLabel label="Attachment (Optional)" />
            <Box
              sx={{
                border: "2px dashed",
                borderColor: "divider",
                borderRadius: 2,
                p: 3,
                textAlign: "center",
                cursor: "pointer",
                bgcolor: "grey.50",
                transition: "all 0.2s ease-in-out",
                "&:hover": {
                  borderColor: "primary.main",
                  bgcolor: "rgba(79, 70, 229, 0.02)",
                },
              }}
              component="label"
            >
              <input
                type="file"
                hidden
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file) {
                    setSelectedFile(file);
                  }
                }}
              />
              <Stack spacing={1} sx={{ alignItems: "center" }}>
                <CloudUploadIcon
                  sx={{ fontSize: 36, color: "text.secondary" }}
                />
                <Typography
                  variant="body2"
                  color="text.primary"
                  sx={{ fontWeight: 600 }}
                >
                  {selectedFile ? selectedFile.name : "Click to select a file"}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Placeholder only (Max size 5MB)
                </Typography>
              </Stack>
            </Box>
            {selectedFile && (
              <Stack
                direction="row"
                spacing={1}
                sx={{ alignItems: "center", mt: 1, color: "text.secondary" }}
              >
                <AttachFileIcon sx={{ fontSize: 18 }} />
                <Typography variant="caption" sx={{ flexGrow: 1 }}>
                  {(selectedFile.size / 1024 / 1024).toFixed(2)} MB
                </Typography>
                <AppButton
                  label="Remove"
                  variant="text"
                  color="error"
                  size="small"
                  onClick={() => setSelectedFile(null)}
                  sx={{ minWidth: "auto", p: 0 }}
                />
              </Stack>
            )}
          </Box>

          {/* Actions */}
          <Stack
            direction="row"
            spacing={2}
            sx={{ justifyContent: "flex-end" }}
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
