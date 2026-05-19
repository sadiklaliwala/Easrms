import { IconButton, type IconButtonProps, Tooltip } from "@mui/material";
import { type ReactNode } from "react";

interface AppIconButtonProps extends IconButtonProps {
  icon: ReactNode;
  tooltip?: string;
}

const AppIconButton = ({ icon, tooltip, ...rest }: AppIconButtonProps) => {
  return tooltip ? (
    <Tooltip title={tooltip}>
      <span>
        <IconButton {...rest}>{icon}</IconButton>
      </span>
    </Tooltip>
  ) : (
    <IconButton {...rest}>{icon}</IconButton>
  );
};

export default AppIconButton;
