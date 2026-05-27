import { Box, Button, TextField, Typography } from "@mui/material";
import { useState } from "react";

import { formatDate } from "../../../utils/formatDate";
import {
  type AddCommentDto,
  type CommentListDto,
  CommentTypeEnum,
} from "../../../types/common.types";
import AppSelect from "../../common/form/AppSelect";

interface RequestCommentBoxProps {
  comments: CommentListDto[];
  onAddComment: (data: AddCommentDto) => Promise<void>;
  isSubmitting?: boolean;
}

const RequestCommentBox = ({
  comments,
  onAddComment,
  isSubmitting = false,
}: RequestCommentBoxProps) => {
  const [text, setText] = useState("");
  const [commentType, setCommentType] = useState<CommentTypeEnum>(
    CommentTypeEnum.Feedback,
  );

  const handleSubmit = async () => {
    if (!text.trim()) return;

    await onAddComment({
      commentText: text.trim(),
      commentType,
    });

    setText("");
    setCommentType(CommentTypeEnum.Feedback);
  };
  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: "bold", mb: 2 }}>
        Comments
      </Typography>
      <Box sx={{ display: "flex", flexDirection: "column", gap: 1.5, mb: 3 }}>
        {comments.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No comments yet.
          </Typography>
        ) : (
          comments.map((c) => (
            <Box
              key={c.commentId}
              sx={{
                p: 1.5,
                bgcolor: "grey.50",
                borderRadius: 1,
                border: 1,
                borderColor: "divider",
              }}
            >
              <Box
                sx={{
                  display: "flex",
                  justifyContent: "space-between",
                  mb: 0.5,
                }}
              >
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {c.commentByName}
                </Typography>
                <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
                  <Typography
                    variant="caption"
                    sx={{
                      bgcolor: "primary.main",
                      color: "primary.contrastText",
                      px: 1,
                      py: 0.25,
                      borderRadius: 1,
                      fontSize: "0.7rem",
                      fontWeight: "bold",
                    }}
                  >
                    {c.commentType}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {formatDate(c.createdOn)}
                  </Typography>
                </Box>
              </Box>
              <Typography variant="body2">{c.commentText}</Typography>
            </Box>
          ))
        )}
      </Box>
      <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
        <TextField
          variant="outlined"
          placeholder="Write a comment..."
          multiline
          rows={3}
          fullWidth
          size="small"
          value={text}
          onChange={(e) => setText(e.target.value)}
          slotProps={{
            htmlInput: {
              maxLength: 990,
            },
          }}
          helperText={
            text.length >= 990 ? (
              <span style={{ color: "#d32f2f" }}>Maximum limit of 990 characters reached</span>
            ) : (
              ""
            )
          }
        />
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Box sx={{ width: 200 }}>
            <AppSelect
              value={commentType}
              onChange={(e) =>
                setCommentType(e.target.value as CommentTypeEnum)
              }
              options={[
                { label: "Feedback", value: CommentTypeEnum.Feedback },
                { label: "Approval", value: CommentTypeEnum.Approval },
                { label: "Resolution", value: CommentTypeEnum.Resolution },
              ]}
              size="small"
            />
          </Box>
          <Button
            variant="contained"
            size="small"
            onClick={handleSubmit}
            disabled={!text.trim() || isSubmitting}
          >
            {isSubmitting ? "Posting..." : "Post Comment"}
          </Button>
        </Box>
      </Box>
    </Box>
  );
};

export default RequestCommentBox;
