import { IconButton, IconButtonProps, Tooltip } from "@mui/material";
import { ReactNode } from "react";

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