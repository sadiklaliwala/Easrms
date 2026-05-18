import { LoadingButton, type LoadingButtonProps } from "@mui/lab";

interface AppLoadingButtonProps extends LoadingButtonProps {
  label: string;
}

const AppLoadingButton = ({ label, ...rest }: AppLoadingButtonProps) => {
  return (
    <LoadingButton variant="contained" {...rest}>
      {label}
    </LoadingButton>
  );
};

export default AppLoadingButton;
