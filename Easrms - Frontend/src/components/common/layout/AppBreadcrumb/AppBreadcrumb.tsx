import { Breadcrumbs, Link, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import HomeOutlinedIcon from "@mui/icons-material/HomeOutlined";

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
    <Breadcrumbs
      separator={<NavigateNextIcon fontSize="small" sx={{ color: "grey.400" }} />}
      sx={{ mb: 1.5 }}
    >
      <Link
        underline="hover"
        color="inherit"
        sx={{
          display: "flex",
          alignItems: "center",
          gap: 0.5,
          cursor: "pointer",
          color: "text.secondary",
          fontSize: "0.85rem",
          fontWeight: 500,
          transition: "color 0.2s ease",
          "&:hover": { color: "primary.main" },
        }}
        onClick={() => navigate("/")}
      >
        <HomeOutlinedIcon sx={{ fontSize: 16 }} />
        Home
      </Link>

      {items.map((item, index) =>
        item.path && index !== items.length - 1 ? (
          <Link
            key={index}
            underline="hover"
            color="inherit"
            sx={{
              cursor: "pointer",
              color: "text.secondary",
              fontSize: "0.85rem",
              fontWeight: 500,
              transition: "color 0.2s ease",
              "&:hover": { color: "primary.main" },
            }}
            onClick={() => navigate(item.path!)}
          >
            {item.label}
          </Link>
        ) : (
          <Typography
            key={index}
            variant="body2"
            sx={{
              color: "text.primary",
              fontWeight: 600,
              fontSize: "0.85rem",
            }}
          >
            {item.label}
          </Typography>
        )
      )}
    </Breadcrumbs>
  );
};

export default AppBreadcrumb;