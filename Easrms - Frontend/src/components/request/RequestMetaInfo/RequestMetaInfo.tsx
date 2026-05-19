import { Box, Divider, Typography } from "@mui/material";

interface RequestMetaInfoProps {
  employeeName: string;
  assigneeName?: string;
  createdOn: string;
  updatedOn?: string;
  resolvedOn?: string;
  closedOn?: string;
}

const MetaRow = ({ label, value }: { label: string; value?: string }) => {
  if (!value) return null;
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        py: 1,
      }}
    >
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 500 }}>
        {value}
      </Typography>
    </Box>
  );
};

const RequestMetaInfo = ({
  employeeName,
  assigneeName,
  createdOn,
  updatedOn,
  resolvedOn,
  closedOn,
}: RequestMetaInfoProps) => {
  return (
    <Box>
      <MetaRow label="Raised By" value={employeeName} />
      <Divider />
      <MetaRow label="Assigned To" value={assigneeName} />
      <Divider />
      <MetaRow label="Created On" value={createdOn} />
      <Divider />
      <MetaRow label="Updated On" value={updatedOn} />
      <Divider />
      <MetaRow label="Resolved On" value={resolvedOn} />
      <Divider />
      <MetaRow label="Closed On" value={closedOn} />
    </Box>
  );
};

export default RequestMetaInfo;
