import { Box, Typography } from "@mui/material";
import { AppStatusBadge } from "../../common/table/AppStatusBadge";
import { AppPriorityBadge } from "../../common/table/AppPriorityBadge";
import { RequestNumberBadge } from "../RequestNumberBadge";

interface RequestDetailHeaderProps {
  requestNumber: string;
  title: string;
  status: string;
  priority: string;
  categoryName: string;
}

const RequestDetailHeader = ({
  requestNumber,
  title,
  status,
  priority,
  categoryName,
}: RequestDetailHeaderProps) => {
  return (
    <Box mb={3}>
      <Box display="flex" alignItems="center" gap={1} mb={1}>
        <RequestNumberBadge requestNumber={requestNumber} />
        <AppStatusBadge status={status} />
        <AppPriorityBadge priority={priority} />
      </Box>
      <Typography variant="h5" fontWeight={600} mb={0.5}>
        {title}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        Category: {categoryName}
      </Typography>
    </Box>
  );
};

export default RequestDetailHeader;