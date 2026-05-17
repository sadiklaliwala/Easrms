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
  Typography,
} from "@mui/material";
import { useState } from "react";
import { AppModal } from "../AppModal";
import { SupportUserLookupDto } from "../../../../types/common.types";

interface AssignSupportDialogProps {
  open: boolean;
  onClose: () => void;
  onAssign: (supportUserId: string) => void;
  supportUsers: SupportUserLookupDto[];
  isSubmitting?: boolean;
}

const AssignSupportDialog = ({
  open,
  onClose,
  onAssign,
  supportUsers,
  isSubmitting = false,
}: AssignSupportDialogProps) => {
  const [selectedUserId, setSelectedUserId] = useState("");
  const [error, setError] = useState("");

  const handleClose = () => {
    setSelectedUserId("");
    setError("");
    onClose();
  };

  const handleAssign = () => {
    if (!selectedUserId) {
      setError("Please select a support user.");
      return;
    }
    setError("");
    onAssign(selectedUserId);
    handleClose();
  };

  return (
    <AppModal open={open} onClose={handleClose} title="Assign Support User" maxWidth="sm">
      <DialogContent sx={{ px: 0, pt: 1 }}>
        <Typography variant="body2" color="text.secondary" mb={2}>
          Select a support user to assign this request to.
        </Typography>
        <FormControl fullWidth size="small" error={!!error}>
          <InputLabel>Support User</InputLabel>
          <Select
            value={selectedUserId}
            label="Support User"
            onChange={(e) => {
              setSelectedUserId(e.target.value);
              if (error) setError("");
            }}
          >
            {supportUsers.map((user) => (
              <MenuItem key={user.userId} value={user.userId}>
                {user.fullName}
              </MenuItem>
            ))}
          </Select>
          {error && <FormHelperText>{error}</FormHelperText>}
        </FormControl>
      </DialogContent>
      <DialogActions sx={{ px: 0, pt: 2 }}>
        <Button onClick={handleClose} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button
          onClick={handleAssign}
          variant="contained"
          disabled={isSubmitting}
        >
          Assign
        </Button>
      </DialogActions>
    </AppModal>
  );
};

export default AssignSupportDialog;