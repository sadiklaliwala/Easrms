import { useState, useCallback, useRef, useEffect } from "react";
import { useSelector } from "react-redux";
import toast from "react-hot-toast";

import { useSendChatMessageMutation } from "../../../../store/api/chat.endpoints";
import type { ChatMessage } from "../../../../types/chat.types";
import { ChatHelpers } from "./chatHelpers";

export const useChat = () => {
  const { fullName, roleName } = useSelector((state: any) => state.auth);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [sendMessageMutation, { isLoading }] = useSendChatMessageMutation();

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const initChat = useCallback(() => {
    const welcomeText = ChatHelpers.generateWelcomeMessage(
      fullName || "",
      roleName || "",
    );

    setMessages([
      {
        id: crypto.randomUUID(),
        role: "bot",
        text: welcomeText,
        timestamp: new Date(),
      },
    ]);
  }, [roleName]);

  useEffect(() => {
    initChat();
  }, [initChat]);

  useEffect(() => {
    scrollToBottom();
  }, [scrollToBottom]);

  const handleSendMessage = async (text: string) => {
    if (!text.trim() || isLoading) return;

    // 1. Add User Message
    const userMsg: ChatMessage = {
      id: crypto.randomUUID(),
      role: "user",
      text,
      timestamp: new Date(),
    };
    setMessages((prev) => [...prev, userMsg]);

    // 2. Client-Side Commands (e.g. Help)
    if (ChatHelpers.isHelpCommand(text)) {
      const helpText = ChatHelpers.generateHelpMessage(roleName || "");
      const botMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: "bot",
        text: helpText,
        timestamp: new Date(),
        intent: "help",
      };

      setTimeout(() => setMessages((prev) => [...prev, botMsg]), 400);
      return;
    }

    // 3. Server-Side Execution
    try {
      const response = await sendMessageMutation({ message: text }).unwrap();

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
      console.error("Chat API Error:", error);
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

  return {
    messages,
    isLoading,
    messagesEndRef,
    handleSendMessage,
    handleClearChat,
  };
};
