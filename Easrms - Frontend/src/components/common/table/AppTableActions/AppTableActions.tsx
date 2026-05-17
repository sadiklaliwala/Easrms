import {
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
} from "@mui/material";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import { MouseEvent, ReactNode, useState } from "react";

interface ActionItem {
  label: string;
  icon?: ReactNode;
  onClick: () => void;
  color?: string;
  disabled?: boolean;
}

interface AppTableActionsProps {
  actions: ActionItem[];
}

const AppTableActions = ({ actions }: AppTableActionsProps) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleOpen = (e: MouseEvent<HTMLElement>) => {
    e.stopPropagation();
    setAnchorEl(e.currentTarget);
  };

  const handleClose = () => setAnchorEl(null);

  return (
    <>
      <IconButton size="small" onClick={handleOpen}>
        <MoreVertIcon />
      </IconButton>
      <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
        {actions.map((action, index) => (
          <MenuItem
            key={index}
            disabled={action.disabled}
            onClick={() => {
              action.onClick();
              handleClose();
            }}
          >
            {action.icon && (
              <ListItemIcon sx={{ color: action.color }}>
                {action.icon}
              </ListItemIcon>
            )}
            <ListItemText
              primary={action.label}
              primaryTypographyProps={{
                fontSize: 14,
                color: action.color,
              }}
            />
          </MenuItem>
        ))}
      </Menu>
    </>
  );
};

export default AppTableActions;