import {
  Box,
  Button,
  DialogActions,
  DialogContent,
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import AppModal from "../AppModal";

interface UpdateStatusDialogProps {
  open: boolean;
  onClose: () => void;
  onUpdate: (newStatus: number, remarks: string) => void;
  currentStatus: string;
  isSubmitting?: boolean;
}

import { STATUS } from "../../../../constants/status.constants";

const STATUS_TRANSITIONS: Record<string, { label: string; value: number }[]> = {
  [STATUS.ASSIGNED]: [{ label: STATUS.IN_PROGRESS, value: 6 }],
  [STATUS.IN_PROGRESS]: [{ label: STATUS.RESOLVED, value: 7 }],
};

const UpdateStatusDialog = ({
  open,
  onClose,
  onUpdate,
  currentStatus,
  isSubmitting = false,
}: UpdateStatusDialogProps) => {
  const [selectedStatus, setSelectedStatus] = useState<number | "">("");
  const [remarks, setRemarks] = useState("");
  const [statusError, setStatusError] = useState("");
  const [remarksError, setRemarksError] = useState("");

  const availableTransitions = STATUS_TRANSITIONS[currentStatus] ?? [];

  const handleClose = () => {
    setSelectedStatus("");
    setRemarks("");
    setStatusError("");
    setRemarksError("");
    onClose();
  };

  const handleUpdate = () => {
    let valid = true;

    if (selectedStatus === "") {
      setStatusError("Please select a new status.");
      valid = false;
    } else {
      setStatusError("");
    }

    const selectedLabel = availableTransitions.find(
      (t) => t.value === selectedStatus,
    )?.label;

    if (selectedLabel === "Resolved" && !remarks.trim()) {
      setRemarksError("Remarks are mandatory when resolving a request.");
      valid = false;
    } else {
      setRemarksError("");
    }

    if (!valid) return;

    onUpdate(selectedStatus as number, remarks);
    handleClose();
  };

  return (
    <AppModal
      open={open}
      onClose={handleClose}
      title="Update Request Status"
      maxWidth="sm"
    >
      <DialogContent sx={{ px: 0, pt: 1 }}>
        <Typography
          variant="body2"
          sx={{
            color: "text.secondary",
            mb: 2,
          }}
        >
          Current Status: <strong>{currentStatus}</strong>
        </Typography>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          <FormControl fullWidth size="small" error={!!statusError}>
            <InputLabel>New Status</InputLabel>
            <Select
              value={selectedStatus}
              label="New Status"
              onChange={(e) => {
                setSelectedStatus(e.target.value as number);
                if (statusError) setStatusError("");
              }}
            >
              {availableTransitions.map((t) => (
                <MenuItem key={t.value} value={t.value}>
                  {t.label}
                </MenuItem>
              ))}
            </Select>
            {statusError && <FormHelperText>{statusError}</FormHelperText>}
          </FormControl>
          <TextField
            variant="outlined"
            label="Remarks"
            multiline
            rows={3}
            fullWidth
            value={remarks}
            onChange={(e) => {
              setRemarks(e.target.value);
              if (remarksError) setRemarksError("");
            }}
            error={!!remarksError}
            helperText={
              remarks.length >= 490 ? (
                <span style={{ color: "#d32f2f" }}>Maximum limit of 490 characters reached</span>
              ) : (
                remarksError || "Remarks are mandatory when resolving."
              )
            }
            slotProps={{
              htmlInput: {
                maxLength: 490,
              },
            }}
          />
        </Box>
      </DialogContent>
      <DialogActions sx={{ px: 0, pt: 2 }}>
        <Button onClick={handleClose} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button
          onClick={handleUpdate}
          variant="contained"
          disabled={isSubmitting}
        >
          Update Status
        </Button>
      </DialogActions>
    </AppModal>
  );
};

export default UpdateStatusDialog;
