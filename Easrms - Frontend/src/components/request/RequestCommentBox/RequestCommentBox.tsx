import { Box, Button, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { CommentListDto } from "../../../types/comment.types";
import { formatDate } from "../../../utils/formatDate";

interface RequestCommentBoxProps {
  comments: CommentListDto[];
  onAddComment: (text: string, type: number) => void;
  isSubmitting?: boolean;
}

const RequestCommentBox = ({
  comments,
  onAddComment,
  isSubmitting = false,
}: RequestCommentBoxProps) => {
  const [text, setText] = useState("");

  const handleSubmit = () => {
    if (!text.trim()) return;
    onAddComment(text.trim(), 1);
    setText("");
  };

  return (
    <Box>
      <Typography variant="subtitle1" fontWeight={600} mb={2}>
        Comments
      </Typography>
      <Box display="flex" flexDirection="column" gap={1.5} mb={3}>
        {comments.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No comments yet.
          </Typography>
        ) : (
          comments.map((c) => (
            <Box
              key={c.commentId}
              p={1.5}
              bgcolor="grey.50"
              borderRadius={1}
              border={1}
              borderColor="divider"
            >
              <Box display="flex" justifyContent="space-between" mb={0.5}>
                <Typography variant="body2" fontWeight={600}>
                  {c.commentByName}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {formatDate(c.createdOn)}
                </Typography>
              </Box>
              <Typography variant="body2">{c.commentText}</Typography>
            </Box>
          ))
        )}
      </Box>
      <Box display="flex" flexDirection="column" gap={1}>
        <TextField
          placeholder="Write a comment..."
          multiline
          rows={3}
          fullWidth
          size="small"
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <Box display="flex" justifyContent="flex-end">
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