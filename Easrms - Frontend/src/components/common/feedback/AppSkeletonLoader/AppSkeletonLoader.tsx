import { Skeleton, Stack } from "@mui/material";

interface AppSkeletonLoaderProps {
  rows?: number;
}

const AppSkeletonLoader = ({ rows = 5 }: AppSkeletonLoaderProps) => {
  return (
    <Stack spacing={1}>
      {Array.from({ length: rows }).map((_, i) => (
        <Skeleton key={i} variant="rectangular" height={48} borderRadius={1} />
      ))}
    </Stack>
  );
};

export default AppSkeletonLoader;