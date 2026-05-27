import { useEffect } from "react";
import {
  DialogActions,
  DialogContent,
  TextField,
  Typography,
} from "@mui/material";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";

import AppModal from "../AppModal";
import AppLoadingButton from "../../buttons/AppLoadingButton";
import AppButton from "../../buttons/AppButton";
import Joi from "../../../../utils/appJoi";

interface ReopenRequestDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (reason: string) => void;
  isLoading: boolean;
}

const schema = Joi.object({
  reopenReason: Joi.string().min(10).max(500).required().messages({
    "string.empty": "Reopen reason is required",
    "string.min": "Minimum 10 characters required",
    "string.max": "Maximum 500 characters allowed",
  }),
});

const ReopenRequestDialog = ({
  open,
  onClose,
  onConfirm,
  isLoading,
}: ReopenRequestDialogProps) => {
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm({
    resolver: joiResolver(schema),
    defaultValues: {
      reopenReason: "",
    },
  });

  useEffect(() => {
    if (open) {
      reset({ reopenReason: "" });
    }
  }, [open, reset]);

  const handleClose = () => {
    reset();
    onClose();
  };

  const onSubmit = (data: { reopenReason: string }) => {
    onConfirm(data.reopenReason);
  };

  return (
    <AppModal
      open={open}
      onClose={handleClose}
      title="Reopen Request"
      maxWidth="sm"
    >
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogContent sx={{ px: 0, pt: 1 }}>
          <Typography
            variant="body2"
            sx={{
              fontSize: 13,
              color: "text.secondary",
              mb: 2,
            }}
          >
            This request was rejected. Please provide a reason for reopening.
          </Typography>
          <Controller
            name="reopenReason"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                variant="outlined"
                label="Reopen Reason *"
                multiline
                rows={3}
                fullWidth
                error={!!errors.reopenReason}
                helperText={errors.reopenReason?.message}
                disabled={isLoading}
              />
            )}
          />
        </DialogContent>
        <DialogActions sx={{ px: 0, pt: 2 }}>
          <AppButton
            label="Cancel"
            variant="outlined"
            color="inherit"
            onClick={handleClose}
            disabled={isLoading}
          />
          <AppLoadingButton
            label="Reopen Request"
            type="submit"
            loading={isLoading}
            variant="contained"
            color="primary"
          />
        </DialogActions>
      </form>
    </AppModal>
  );
};

export default ReopenRequestDialog;
