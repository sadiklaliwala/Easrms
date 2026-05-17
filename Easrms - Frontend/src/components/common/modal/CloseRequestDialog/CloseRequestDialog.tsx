import { Button, DialogActions, DialogContent, Typography } from "@mui/material";
import { AppModal } from "../AppModal";

interface CloseRequestDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  requestNumber: string;
  isSubmitting?: boolean;
}

const CloseRequestDialog = ({
  open,
  onClose,
  onConfirm,
  requestNumber,
  isSubmitting = false,
}: CloseRequestDialogProps) => {
  return (
    <AppModal open={open} onClose={onClose} title="Close Request" maxWidth="xs">
      <DialogContent sx={{ px: 0, pt: 1 }}>
        <Typography variant="body2">
          Are you sure you want to close request{" "}
          <strong>{requestNumber}</strong>? This action cannot be undone.
        </Typography>
      </DialogContent>
      <DialogActions sx={{ px: 0, pt: 2 }}>
        <Button onClick={onClose} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button
          onClick={onConfirm}
          color="error"
          variant="contained"
          disabled={isSubmitting}
        >
          Close Request
        </Button>
      </DialogActions>
    </AppModal>
  );
};

export default CloseRequestDialog;