import { Box, Typography, Button } from "@mui/material";
import { useState } from "react";
import AttachFileIcon from "@mui/icons-material/AttachFile";
import AppStatusBadge from "../../common/table/AppStatusBadge";
import AppPriorityBadge from "../../common/table/AppPriorityBadge";
import RequestNumberBadge from "../RequestNumberBadge";

interface RequestDetailHeaderProps {
  requestNumber: string;
  title: string;
  status: string;
  priority: string;
  categoryName: string;
  attachmentUrl?: string | null;
}

const RequestDetailHeader = ({
  requestNumber,
  title,
  status,
  priority,
  categoryName,
  attachmentUrl,
}: RequestDetailHeaderProps) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const limit = 120;

  const displayTitle =
    title.length > limit && !isExpanded ? `${title.slice(0, limit)}...` : title;

  return (
    <Box sx={{ mb: 3, display: "flex", justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: 2 }}>
      <Box sx={{ flex: "1 1 auto", minWidth: 0, maxWidth: "100%" }}>
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          gap: 1,
          mb: 1,
        }}
      >
        <RequestNumberBadge requestNumber={requestNumber} />
        <AppStatusBadge status={status} />
        <AppPriorityBadge priority={priority} />
      </Box>
      <Typography variant="h5" sx={{ fontWeight: 600, mb: 0.5, wordBreak: "break-word" }}>
        {displayTitle}
        {title.length > limit && (
          <Box
            component="span"
            onClick={() => setIsExpanded(!isExpanded)}
            sx={{
              color: "primary.main",
              cursor: "pointer",
              fontWeight: 600,
              ml: 1,
              fontSize: "0.85rem",
              display: "inline-block",
              userSelect: "none",
              "&:hover": {
                textDecoration: "underline",
              },
            }}
          >
            {isExpanded ? "Show less" : "Read more"}
          </Box>
        )}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        Category: {categoryName}
      </Typography>
      </Box>
      
      {attachmentUrl && (
        <Button
          variant="outlined"
          startIcon={<AttachFileIcon />}
          href={attachmentUrl}
          target="_blank"
          rel="noopener noreferrer"
          size="small"
          sx={{ textTransform: 'none' }}
        >
          View Attachment
        </Button>
      )}
    </Box>
  );
};

export default RequestDetailHeader;
