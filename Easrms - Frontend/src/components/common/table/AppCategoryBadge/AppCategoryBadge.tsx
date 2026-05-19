import { Chip } from "@mui/material";

interface AppCategoryBadgeProps {
  category: string;
}

const AppCategoryBadge = ({ category }: AppCategoryBadgeProps) => {
  return (
    <Chip
      label={category}
      size="small"
      variant="outlined"
      color="primary"
    />
  );
};

export default AppCategoryBadge;