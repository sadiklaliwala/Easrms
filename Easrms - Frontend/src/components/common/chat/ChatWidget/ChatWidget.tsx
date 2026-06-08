import { useState } from "react";
import { Box, Fab, Zoom, Badge, Fade } from "@mui/material";
import ChatIcon from "@mui/icons-material/Chat";
import ChatWindow from "../ChatWindow/ChatWindow";

const ChatWidget = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [hasUnread, setHasUnread] = useState(true);

  const toggleChat = () => {
    setIsOpen(!isOpen);
    if (!isOpen) {
      setHasUnread(false);
    }
  };

  return (
    <Box sx={{ position: "fixed", bottom: 24, right: 24, zIndex: 1200 }}>
      {/* Floating Action Button */}
      <Zoom in={!isOpen}>
        <Fab
          color="primary"
          aria-label="chat"
          onClick={toggleChat}
          sx={{
            position: "absolute",
            bottom: 0,
            right: 0,
            boxShadow: 4,
            "&:hover": { transform: "scale(1.05)" },
            transition: "transform 0.2s",
          }}
        >
          <Badge color="error" variant="dot" invisible={!hasUnread}>
            <ChatIcon />
          </Badge>
        </Fab>
      </Zoom>

      {/* Chat Window Container */}
      <Fade in={isOpen} unmountOnExit>
        <Box sx={{ position: "absolute", bottom: 0, right: 0, transformOrigin: "bottom right" }}>
          <ChatWindow onClose={toggleChat} />
        </Box>
      </Fade>
    </Box>
  );
};

export default ChatWidget;
