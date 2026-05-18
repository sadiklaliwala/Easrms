import { Box, Chip, Paper, Typography } from "@mui/material";
import { type StatusHistoryDto } from "../../../types/comment.types";
import { formatDate } from "../../../utils/formatDate";

interface RequestHistoryTimelineProps {
  history: StatusHistoryDto[];
}

const RequestHistoryTimeline = ({ history }: RequestHistoryTimelineProps) => {
  if (history.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        No history available.
      </Typography>
    );
  }

  return (
    <Box sx={{ display: "flex", flexDirection: "column", gap: 1.5 }}>
      {history.map((item) => (
        <Paper key={item.historyId} variant="outlined" sx={{ p: 2 }}>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              mb: 0.5,
            }}
          >
            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
              {item.oldStatus && (
                <>
                  <Chip
                    label={item.oldStatus}
                    size="small"
                    variant="outlined"
                  />
                  <Typography variant="body2">→</Typography>
                </>
              )}
              <Chip label={item.newStatus} size="small" color="primary" />
            </Box>
            <Typography variant="caption" color="text.secondary">
              {formatDate(item.changedOn)}
            </Typography>
          </Box>
          <Typography variant="body2" color="text.secondary">
            By: <strong>{item.changedByName}</strong>
          </Typography>
          {item.remarks && (
            <Typography sx={{ variant: "body2", mt: 0.5 }}>
              {item.remarks}
            </Typography>
          )}
        </Paper>
      ))}
    </Box>
  );
};

export default RequestHistoryTimeline;
