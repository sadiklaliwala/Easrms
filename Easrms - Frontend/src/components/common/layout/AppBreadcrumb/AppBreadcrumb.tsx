import { Breadcrumbs, Link, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";

interface BreadcrumbItem {
  label: string;
  path?: string;
}

interface AppBreadcrumbProps {
  items: BreadcrumbItem[];
}

const AppBreadcrumb = ({ items }: AppBreadcrumbProps) => {
  const navigate = useNavigate();

  return (
    <Breadcrumbs sx={{ mb: 2 }}>
      {items.map((item, index) =>
        item.path && index !== items.length - 1 ? (
          <Link
            key={index}
            underline="hover"
            color="inherit"
            sx={{ cursor: "pointer" }}
            onClick={() => navigate(item.path!)}
          >
            {item.label}
          </Link>
        ) : (
          <Typography key={index} color="text.primary">
            {item.label}
          </Typography>
        )
      )}
    </Breadcrumbs>
  );
};

export default AppBreadcrumb;