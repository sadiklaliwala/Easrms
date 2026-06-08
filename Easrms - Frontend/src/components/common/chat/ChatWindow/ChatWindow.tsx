import { useState, useEffect, useRef, useCallback } from "react";
import { Box, Typography, IconButton, Paper } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import DeleteSweepIcon from "@mui/icons-material/DeleteSweep";
import { useSelector } from "react-redux";
import toast from "react-hot-toast";

import ChatBubble from "../ChatBubble/ChatBubble";
import ChatInput from "../ChatInput/ChatInput";

import { useSendChatMessageMutation } from "../../../../store/api/chat.endpoints";
import { ROLES } from "../../../../constants/role.constants";
import type { ChatMessage } from "../../../../types/chat.types";

interface ChatWindowProps {
  onClose: () => void;
}

const generateWelcomeMessage = (fullName: string, roleName: string) => {
  let examples = "show my requests, check status of REQ-0001";

  if (roleName === ROLES.MANAGER) {
    examples = "show my pending approvals, show dashboard summary";
  } else if (roleName === ROLES.ADMIN) {
    examples = "show dashboard summary, show open requests";
  } else if (roleName === ROLES.SUPPORT_USER) {
    examples = "show my tasks, show assigned requests";
  }

  return `Hi ${
    fullName || "there"
  }! I am your EASRMS assistant. You can ask me things like: ${examples}.`;
};

const ChatWindow = ({ onClose }: ChatWindowProps) => {
  const user = useSelector((state: any) => state.auth.user);

  const [messages, setMessages] = useState<ChatMessage[]>([]);

  const [sendMessage, { isLoading }] = useSendChatMessageMutation();

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({
      behavior: "smooth",
    });
  }, []);

  const initChat = useCallback(() => {
    const welcomeText = generateWelcomeMessage(
      user?.fullName || "",
      user?.roleName || "",
    );

    setMessages([
      {
        id: crypto.randomUUID(),
        role: "bot",
        text: welcomeText,
        timestamp: new Date(),
      },
    ]);
  }, [user]);

  useEffect(() => {
    initChat();
  }, [initChat]);

  useEffect(() => {
    scrollToBottom();
  }, [messages, scrollToBottom]);

  const handleSendMessage = async (text: string) => {
    if (!text.trim() || isLoading) return;

    const userMsg: ChatMessage = {
      id: crypto.randomUUID(),
      role: "user",
      text,
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, userMsg]);

    const lowerText = text.trim().toLowerCase();
    if (
      lowerText === "help" ||
      lowerText === "what can i do" ||
      lowerText === "what can you do"
    ) {
      let helpText =
        "Here is what you can ask me to do:\n• Show my requests\n• Show details for a request (e.g., 'What is REQ-0001?')\n• Show request history or comments\n\nPhase 2 Actions:\n• 'create request'\n• 'approve REQ-0001' / 'reject REQ-0001'\n• 'add comment to REQ-0001'";

      if (user?.roleName === ROLES.MANAGER) {
        helpText += "\n• Show my pending approvals\n• Show dashboard summary";
      } else if (user?.roleName === ROLES.ADMIN) {
        helpText += "\n• Show dashboard summary\n• Show open requests";
      } else if (user?.roleName === ROLES.SUPPORT_USER) {
        helpText += "\n• Show my assigned tasks";
      }

      const botMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: "bot",
        text: helpText,
        timestamp: new Date(),
        intent: "help",
      };

      // Add a tiny delay to feel natural
      setTimeout(() => {
        setMessages((prev) => [...prev, botMsg]);
      }, 400);
      return;
    }

    try {
      const response = await sendMessage({
        message: text,
      }).unwrap();

      const botMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: "bot",
        text:
          response.data?.reply ||
          response.message ||
          "Sorry, I couldn't process that.",
        timestamp: new Date(),
        intent: response.data?.intent,
      };

      setMessages((prev) => [...prev, botMsg]);
    } catch (error) {
      console.error(error);

      toast.error("Failed to send message to the assistant.");

      const errorMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: "bot",
        text: "Sorry, I am having trouble connecting to the server right now.",
        timestamp: new Date(),
      };

      setMessages((prev) => [...prev, errorMsg]);
    }
  };

  const handleClearChat = () => {
    initChat();
  };

  return (
    <Paper
      elevation={6}
      sx={{
        width: 380,
        height: 520,
        display: "flex",
        flexDirection: "column",
        borderRadius: 3,
        overflow: "hidden",
      }}
    >
      <Box
        sx={{
          p: 2,
          bgcolor: "primary.main",
          color: "white",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Typography
          sx={{
            variant: "subtitle1",
            fontWeight: "bold",
          }}
        >
          EASRMS Assistant
        </Typography>

        <Box>
          <IconButton
            size="small"
            onClick={handleClearChat}
            sx={{ color: "white", mr: 1 }}
            title="Clear Chat"
          >
            <DeleteSweepIcon fontSize="small" />
          </IconButton>

          <IconButton size="small" onClick={onClose} sx={{ color: "white" }}>
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>
      </Box>

      <Box
        sx={{
          flexGrow: 1,
          p: 2,
          overflowY: "auto",
          bgcolor: "grey.50",
        }}
      >
        {messages.map((msg) => (
          <ChatBubble key={msg.id} {...msg} />
        ))}

        {isLoading && (
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              mb: 2,
            }}
          >
            <Typography variant="body2" color="text.secondary">
              Assistant is typing...
            </Typography>
          </Box>
        )}

        <div ref={messagesEndRef} />
      </Box>

      <ChatInput onSendMessage={handleSendMessage} isLoading={isLoading} />
    </Paper>
  );
};

export default ChatWindow;
