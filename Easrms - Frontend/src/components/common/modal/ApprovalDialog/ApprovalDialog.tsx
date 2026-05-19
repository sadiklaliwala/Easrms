import {
  Button,
  DialogActions,
  DialogContent,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import AppModal from "../AppModal";

interface ApprovalDialogProps {
  open: boolean;
  onClose: () => void;
  onApprove: (comment: string) => void;
  onReject: (comment: string) => void;
  isSubmitting?: boolean;
  loading?: boolean;
}

const ApprovalDialog = ({
  open,
  onClose,
  onApprove,
  onReject,
  isSubmitting = false,
}: ApprovalDialogProps) => {
  const [comment, setComment] = useState("");
  const [error, setError] = useState("");

  const handleClose = () => {
    setComment("");
    setError("");
    onClose();
  };

  const handleApprove = () => {
    setError("");
    onApprove(comment);
    handleClose();
  };

  const handleReject = () => {
    if (!comment.trim()) {
      setError("Comment is mandatory when rejecting a request.");
      return;
    }
    setError("");
    onReject(comment);
    handleClose();
  };

  return (
    <AppModal
      open={open}
      onClose={handleClose}
      title="Approval Decision"
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
          Add a comment before approving or rejecting. Comment is mandatory for
          rejection.
        </Typography>
        <TextField
          label="Comment"
          multiline
          rows={4}
          fullWidth
          value={comment}
          onChange={(e) => {
            setComment(e.target.value);
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
          onClick={handleReject}
          color="error"
          variant="outlined"
          disabled={isSubmitting}
        >
          Reject
        </Button>
        <Button
          onClick={handleApprove}
          color="primary"
          variant="contained"
          disabled={isSubmitting}
        >
          Approve
        </Button>
      </DialogActions>
    </AppModal>
  );
};

export default ApprovalDialog;
