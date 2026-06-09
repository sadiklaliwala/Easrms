import { useState, useRef } from "react";
import { Box, Typography, IconButton, Paper, Tooltip } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import DeleteSweepIcon from "@mui/icons-material/DeleteSweep";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";
import KeyboardArrowUpIcon from "@mui/icons-material/KeyboardArrowUp";
import FullscreenIcon from "@mui/icons-material/Fullscreen";
import FullscreenExitIcon from "@mui/icons-material/FullscreenExit";
import Draggable from "react-draggable";

import ChatBubble from "../ChatBubble/ChatBubble";
import ChatInput from "../ChatInput/ChatInput";
import { useChat } from "./useChat";

interface ChatWindowProps {
  onClose: () => void;
}

const ChatWindow = ({ onClose }: ChatWindowProps) => {
  const {
    messages,
    isLoading,
    messagesEndRef,
    handleSendMessage,
    handleClearChat,
  } = useChat();

  const [isMinimized, setIsMinimized] = useState(false);
  const [isMaximized, setIsMaximized] = useState(false);
  const paperRef = useRef<HTMLDivElement>(null);

  const toggleMinimize = () => {
    setIsMinimized((prev) => !prev);
    if (isMaximized && !isMinimized) {
      setIsMaximized(false);
    }
  };

  const toggleMaximize = () => {
    setIsMaximized((prev) => !prev);
    if (isMinimized && !isMaximized) {
      setIsMinimized(false);
    }
  };

  return (
    <Draggable nodeRef={paperRef} handle=".chat-drag-handle">
      <Paper
        ref={paperRef}
        elevation={6}
        sx={{
          position: "fixed",
          bottom: isMaximized ? 0 : 80,
          right: isMaximized ? 0 : 24,
          width: isMaximized ? "100vw" : 380,
          height: isMinimized ? 64 : isMaximized ? "100vh" : 550,
          display: "flex",
          flexDirection: "column",
          overflow: "hidden",
          borderRadius: isMaximized ? 0 : 3,
          zIndex: 1000,
          bgcolor: "background.paper",
          transition:
            "height 0.3s ease, width 0.3s ease, border-radius 0.3s ease",
        }}
      >
        {/* Header - Drag Handle */}
        <Box
          className="chat-drag-handle"
          sx={{
            p: 2,
            bgcolor: "primary.main",
            color: "primary.contrastText",
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            cursor: "grab",
            "&:active": {
              cursor: "grabbing",
            },
          }}
        >
          <Typography
            sx={{
              variant: "subtitle1",
              fontWeight: "bold",
              userSelect: "none",
            }}
          >
            EASRMS Assistant
          </Typography>

          <Box sx={{ display: "flex", gap: 0.5 }}>
            <Tooltip title="Clear Chat">
              <IconButton
                size="small"
                onClick={handleClearChat}
                sx={{ color: "inherit" }}
              >
                <DeleteSweepIcon fontSize="small" />
              </IconButton>
            </Tooltip>

            <Tooltip title={isMinimized ? "Expand" : "Minimize"}>
              <IconButton
                size="small"
                onClick={toggleMinimize}
                sx={{ color: "inherit" }}
              >
                {isMinimized ? (
                  <KeyboardArrowUpIcon fontSize="small" />
                ) : (
                  <KeyboardArrowDownIcon fontSize="small" />
                )}
              </IconButton>
            </Tooltip>

            <Tooltip title={isMaximized ? "Restore" : "Maximize"}>
              <IconButton
                size="small"
                onClick={toggleMaximize}
                sx={{ color: "inherit" }}
              >
                {isMaximized ? (
                  <FullscreenExitIcon fontSize="small" />
                ) : (
                  <FullscreenIcon fontSize="small" />
                )}
              </IconButton>
            </Tooltip>

            <Tooltip title="Close">
              <IconButton
                size="small"
                onClick={onClose}
                sx={{ color: "inherit" }}
              >
                <CloseIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        </Box>

        {/* Messages Area - hidden when minimized */}
        {!isMinimized && (
          <>
            <Box
              sx={{
                flex: 1,
                p: 2,
                overflowY: "auto",
                bgcolor: "grey.50",
                display: "flex",
                flexDirection: "column",
              }}
            >
              {messages.map((msg) => (
                <ChatBubble
                  key={msg.id}
                  role={msg.role}
                  text={msg.text}
                  timestamp={msg.timestamp}
                  intent={msg.intent}
                />
              ))}
              {isLoading && (
                <Box
                  sx={{ display: "flex", alignItems: "center", gap: 1, mt: 1 }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Typing...
                  </Typography>
                </Box>
              )}
              <div ref={messagesEndRef} />
            </Box>

            {/* Input Area */}
            <ChatInput
              onSendMessage={handleSendMessage}
              isLoading={isLoading}
            />
          </>
        )}
      </Paper>
    </Draggable>
  );
};

export default ChatWindow;
