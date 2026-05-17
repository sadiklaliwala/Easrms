import { Card, CardContent, CardProps } from "@mui/material";
import { ReactNode } from "react";

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