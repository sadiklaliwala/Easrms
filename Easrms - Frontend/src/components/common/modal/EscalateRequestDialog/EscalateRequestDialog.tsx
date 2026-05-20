import {
  Button,
  DialogActions,
  DialogContent,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import AppModal from "../AppModal";

interface EscalateRequestDialogProps {
  open: boolean;
  onClose: () => void;
  requestNumber: string;
  dueDate: string | null;
  onEscalate: (reason: string) => void;
  isSubmitting?: boolean;
}

const EscalateRequestDialog = ({
  open,
  onClose,
  requestNumber,
  dueDate,
  onEscalate,
  isSubmitting = false,
}: EscalateRequestDialogProps) => {
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");

  const handleClose = () => {
    setReason("");
    setError("");
    onClose();
  };

  const handleEscalate = () => {
    if (!reason.trim()) {
      setError("Escalation reason is mandatory.");
      return;
    }
    setError("");
    onEscalate(reason);
    handleClose();
  };

  return (
    <AppModal
      open={open}
      onClose={handleClose}
      title="Escalate Request"
      maxWidth="sm"
    >
      <DialogContent sx={{ px: 0, pt: 1 }}>
        <Typography
          variant="body2"
          sx={{
            fontSize: 13,
            color: "text.secondary",
            mb: 2,
          }}
        >
          Request: {requestNumber} | Due Date: {dueDate || "N/A"}
        </Typography>
        <Typography
          variant="body2"
          sx={{
            fontSize: 13,
            color: "text.secondary",
            mb: 2,
          }}
        >
          Please provide a reason for escalating this request. This action cannot be undone.
        </Typography>
        <TextField
          label="Escalation Reason"
          multiline
          rows={4}
          fullWidth
          value={reason}
          onChange={(e) => {
            setReason(e.target.value);
            if (error) setError("");
          }}
          error={!!error}
          helperText={error}
        />
      </DialogContent>
      <DialogActions sx={{ px: 0, pt: 2 }}>
        <Button onClick={handleClose} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button
          onClick={handleEscalate}
          color="error"
          variant="contained"
          disabled={isSubmitting || !reason.trim()}
        >
          Escalate
        </Button>
      </DialogActions>
    </AppModal>
  );
};

export default EscalateRequestDialog;
