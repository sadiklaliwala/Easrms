import { Button, type ButtonProps } from "@mui/material";

interface AppButtonProps extends ButtonProps {
  label: string;
}

const AppButton = ({ label, ...rest }: AppButtonProps) => {
  return (
    <Button variant="contained" {...rest}>
      {label}
    </Button>
  );
};

export default AppButton;
