import { TextField, TextFieldProps } from "@mui/material";

type AppInputProps = TextFieldProps & {
  label: string;
};

const AppInput = ({ label, ...rest }: AppInputProps) => {
  return (
    <TextField
      label={label}
      fullWidth
      size="small"
      variant="outlined"
      {...rest}
    />
  );
};

export default AppInput;