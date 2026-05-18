import { TextField, type TextFieldProps } from "@mui/material";

type AppInputProps = TextFieldProps;

const AppInput = ({ ...rest }: AppInputProps) => {
  return <TextField fullWidth size="small" variant="outlined" {...rest} />;
};

export default AppInput;
