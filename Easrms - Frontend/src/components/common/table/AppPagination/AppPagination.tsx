import { Box, MenuItem, Pagination, Select, Typography } from "@mui/material";

interface AppPaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

const PAGE_SIZE_OPTIONS = [10, 25, 50];

const AppPagination = ({
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  onPageChange,
  onPageSizeChange,
}: AppPaginationProps) => {
  const from = (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <Box
      display="flex"
      justifyContent="space-between"
      alignItems="center"
      mt={2}
      flexWrap="wrap"
      gap={1}
    >
      <Typography variant="body2" color="text.secondary">
        Showing {totalCount === 0 ? 0 : from}–{to} of {totalCount} records
      </Typography>
      <Box display="flex" alignItems="center" gap={2}>
        <Box display="flex" alignItems="center" gap={1}>
          <Typography variant="body2" color="text.secondary">
            Rows per page:
          </Typography>
          <Select
            size="small"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            sx={{ fontSize: 14 }}
          >
            {PAGE_SIZE_OPTIONS.map((size) => (
              <MenuItem key={size} value={size}>
                {size}
              </MenuItem>
            ))}
          </Select>
        </Box>
        <Pagination
          count={totalPages}
          page={pageNumber}
          onChange={(_, page) => onPageChange(page)}
          color="primary"
          shape="rounded"
          size="small"
        />
      </Box>
    </Box>
  );
};

export default AppPagination;