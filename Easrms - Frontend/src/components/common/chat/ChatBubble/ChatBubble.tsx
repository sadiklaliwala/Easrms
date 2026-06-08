import { Box, Typography, Avatar } from "@mui/material";
import SmartToyIcon from "@mui/icons-material/SmartToy";
import { format } from "date-fns";

interface ChatBubbleProps {
  role: "user" | "bot";
  text: string;
  timestamp: Date;
  intent?: string;
}

const ChatBubble = ({ role, text, timestamp, intent }: ChatBubbleProps) => {
  const isBot = role === "bot";
  const isUnknown = intent === "unknown";

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: isBot ? "row" : "row-reverse",
        alignItems: "flex-end",
        mb: 2,
        gap: 1,
      }}
    >
      {isBot && (
        <Avatar
          sx={{
            width: 32,
            height: 32,
            bgcolor: "primary.main",
          }}
        >
          <SmartToyIcon fontSize="small" />
        </Avatar>
      )}

      <Box
        sx={{
          maxWidth: "75%",
          display: "flex",
          flexDirection: "column",
          alignItems: isBot ? "flex-start" : "flex-end",
        }}
      >
        <Box
          sx={{
            p: 1.5,
            borderRadius: 2,
            bgcolor: isBot ? (isUnknown ? "warning.light" : "grey.200") : "primary.main",
            color: isBot ? (isUnknown ? "warning.contrastText" : "text.primary") : "primary.contrastText",
            borderBottomLeftRadius: isBot ? 0 : 8,
            borderBottomRightRadius: isBot ? 8 : 0,
            wordBreak: "break-word",
          }}
        >
          <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
            {text}
          </Typography>
        </Box>
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ mt: 0.5, px: 0.5, fontSize: "0.7rem" }}
        >
          {format(new Date(timestamp), "HH:mm")}
        </Typography>
      </Box>
    </Box>
  );
};

export default ChatBubble;
