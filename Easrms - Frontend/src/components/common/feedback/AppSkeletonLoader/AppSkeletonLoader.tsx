import { Skeleton, Stack } from "@mui/material";

interface AppSkeletonLoaderProps {
  rows?: number;
}

const AppSkeletonLoader = ({ rows = 5 }: AppSkeletonLoaderProps) => {
  return (
    <Stack spacing={1}>
      {Array.from({ length: rows }).map((_, i) => (
        <Skeleton
          key={i}
          variant="rectangular"
          sx={{
            borderRadius: 1,
            height: 48,
          }}
        />
      ))}
    </Stack>
  );
};

export default AppSkeletonLoader;
