import { Box, Typography } from "@mui/material";
import { type StatusHistoryDto } from "../../../types/comment.types";
import { formatDate } from "../../../utils/formatDate";

interface RequestHistoryTimelineProps {
  history: StatusHistoryDto[];
}

const RequestHistoryTimeline = ({ history }: RequestHistoryTimelineProps) => {
  if (history.length === 0) {
    return (
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ textAlign: "center", py: 4 }}
      >
        No history available.
      </Typography>
    );
  }

  return (
    <Box sx={{ display: "flex", flexDirection: "column" }}>
      <Typography
        variant="subtitle2"
        sx={{ fontWeight: 700, mb: 3, color: "text.primary" }}
      >
        Activity Timeline
      </Typography>

      <Box sx={{ display: "flex", flexDirection: "column" }}>
        {history.map((item, index) => {
          const isLast = index === history.length - 1;
          return (
            <Box
              key={item.historyId}
              sx={{
                position: "relative",
                pl: 4,
                pb: isLast ? 0 : 3,
              }}
            >
              {/* Vertical line indicator */}
              {!isLast && (
                <Box
                  sx={{
                    position: "absolute",
                    left: 15,
                    top: 20,
                    bottom: 0,
                    width: 2,
                    bgcolor: "grey.200",
                  }}
                />
              )}

              {/* Dot indicator */}
              <Box
                sx={{
                  position: "absolute",
                  left: 11,
                  top: 6,
                  width: 10,
                  height: 10,
                  borderRadius: "50%",
                  bgcolor: "secondary.main",
                  border: "2px solid #fff",
                  boxShadow: "0 0 0 2px rgba(79, 70, 229, 0.15)",
                  zIndex: 1,
                }}
              />

              {/* Timeline Content */}
              <Box>
                <Box
                  sx={{
                    display: "flex",
                    flexWrap: "wrap",
                    alignItems: "center",
                    justifyContent: "space-between",
                    gap: 1,
                    mb: 0.5,
                  }}
                >
                  <Typography
                    variant="caption"
                    sx={{ color: "text.secondary", fontWeight: 600 }}
                  >
                    {formatDate(item.changedOn)}
                  </Typography>
                  <Typography
                    variant="caption"
                    sx={{ color: "text.secondary", fontWeight: 500 }}
                  >
                    By {item.changedByName}
                  </Typography>
                </Box>

                <Box
                  sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 1,
                    flexWrap: "wrap",
                    mb: 0.5,
                  }}
                >
                  {item.oldStatus ? (
                    <>
                      <Typography
                        variant="caption"
                        sx={{
                          bgcolor: "grey.100",
                          px: 1,
                          py: 0.25,
                          borderRadius: 1,
                          color: "text.secondary",
                          fontWeight: 500,
                        }}
                      >
                        {item.oldStatus}
                      </Typography>
                      <Typography
                        variant="caption"
                        sx={{ color: "text.secondary" }}
                      >
                        →
                      </Typography>
                      <Typography
                        variant="caption"
                        sx={{
                          bgcolor: "rgba(79, 70, 229, 0.08)",
                          px: 1,
                          py: 0.25,
                          borderRadius: 1,
                          color: "secondary.dark",
                          fontWeight: 600,
                        }}
                      >
                        {item.newStatus}
                      </Typography>
                    </>
                  ) : (
                    <Typography
                      variant="caption"
                      sx={{
                        bgcolor: "rgba(79, 70, 229, 0.08)",
                        px: 1,
                        py: 0.25,
                        borderRadius: 1,
                        color: "secondary.dark",
                        fontWeight: 600,
                      }}
                    >
                      {item.newStatus}
                    </Typography>
                  )}
                </Box>

                {item.remarks && (
                  <Box
                    sx={{
                      mt: 1,
                      p: 1.5,
                      bgcolor: "grey.50",
                      borderRadius: 1.5,
                      border: "1px solid",
                      borderColor: "grey.100",
                    }}
                  >
                    <Typography
                      variant="body2"
                      sx={{
                        color: "text.secondary",
                        fontSize: "0.825rem",
                        lineHeight: 1.4,
                      }}
                    >
                      {item.remarks}
                    </Typography>
                  </Box>
                )}
              </Box>
            </Box>
          );
        })}
      </Box>
    </Box>
  );
};

export default RequestHistoryTimeline;
