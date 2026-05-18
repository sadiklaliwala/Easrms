import { Card, CardContent, type CardProps } from "@mui/material";
import type { ReactNode } from "react";

interface AppCardProps extends CardProps {
  children: ReactNode;
}

const AppCard = ({ children, ...rest }: AppCardProps) => {
  return (
    <Card elevation={1} {...rest}>
      <CardContent>{children}</CardContent>
    </Card>
  );
};

export default AppCard;
