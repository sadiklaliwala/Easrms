import { CommentTypeEnum } from "../types/common.types";

export const mapCommentTypeNameToEnum = (
  name: string
): CommentTypeEnum | undefined => {
  switch (name.toLowerCase()) {
    case "approval":
      return CommentTypeEnum.Approval;
    case "feedback":
      return CommentTypeEnum.Feedback;
    case "resolution":
      return CommentTypeEnum.Resolution;
    default:
      return undefined;
  }
};

export const getCommentTypeName = (type: CommentTypeEnum): string => {
  switch (type) {
    case CommentTypeEnum.Approval:
      return "Approval";
    case CommentTypeEnum.Feedback:
      return "Feedback";
    case CommentTypeEnum.Resolution:
      return "Resolution";
    default:
      return "Unknown";
  }
};
