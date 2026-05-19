// import { LoadingButton, type LoadingButtonProps } from "@mui/lab";

// interface AppLoadingButtonProps extends LoadingButtonProps {
//   label: string;
// }

// const AppLoadingButton = ({ label, ...rest }: AppLoadingButtonProps) => {
//   return (
//     <LoadingButton variant="contained" {...rest}>
//       {label}
//     </LoadingButton>
//   );
// };

// export default AppLoadingButton;

import { Button, type ButtonProps } from "@mui/material";

interface AppLoadingButtonProps extends ButtonProps {
  label: string;
  loading?: boolean;
}

const AppLoadingButton = ({
  label,
  loading,
  disabled,
  ...rest
}: AppLoadingButtonProps) => {
  return (
    <Button
      variant="contained"
      loading={loading}
      disabled={loading || disabled}
      {...rest}
    >
      {label}
    </Button>
  );
};

export default AppLoadingButton;
