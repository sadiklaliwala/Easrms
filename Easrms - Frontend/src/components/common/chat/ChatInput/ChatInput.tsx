import { useState, type KeyboardEvent } from "react";
import {
  Box,
  TextField,
  IconButton,
  CircularProgress,
  Typography,
} from "@mui/material";
import SendIcon from "@mui/icons-material/Send";

interface ChatInputProps {
  onSendMessage: (message: string) => void;
  isLoading: boolean;
}

const ChatInput = ({ onSendMessage, isLoading }: ChatInputProps) => {
  const [message, setMessage] = useState("");

  const handleSend = () => {
    if (message.trim() && !isLoading) {
      onSendMessage(message.trim());
      setMessage("");
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLDivElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <Box
      sx={{
        display: "flex",
        p: 1,
        borderTop: 1,
        borderColor: "divider",
        alignItems: "flex-end",
      }}
    >
      <Box sx={{ flex: 1, position: "relative" }}>
        <TextField
          fullWidth
          multiline
          maxRows={4}
          placeholder="Type your message..."
          variant="outlined"
          size="small"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={isLoading}
          slotProps={{
            htmlInput: {
              maxLength: 250,
            },
          }}
          sx={{
            "& .MuiOutlinedInput-root": {
              borderRadius: 3,
              backgroundColor: "background.paper",
            },
          }}
        />
        <Typography
          variant="caption"
          sx={{
            position: "absolute",
            bottom: -18,
            right: 8,
            fontSize: "0.65rem",
            color: message.length >= 250 ? "error.main" : "text.secondary",
          }}
        >
          {message.length} / 250
        </Typography>
      </Box>
      <IconButton
        color="primary"
        onClick={handleSend}
        disabled={!message.trim() || isLoading}
        sx={{
          ml: 1,
          mb: 0.5,
          bgcolor: "primary.main",
          color: "white",
          "&:hover": { bgcolor: "primary.dark" },
          "&.Mui-disabled": {
            bgcolor: "action.disabledBackground",
            color: "action.disabled",
          },
        }}
      >
        {isLoading ? (
          <CircularProgress size={24} color="inherit" />
        ) : (
          <SendIcon fontSize="small" />
        )}
      </IconButton>
    </Box>
  );
};

export default ChatInput;
