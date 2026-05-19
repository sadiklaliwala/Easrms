import { TextField, type TextFieldProps } from "@mui/material";

type AppTextAreaProps = TextFieldProps & {
  label?: string;
  rows?: number;
};

const AppTextArea = ({ label, rows = 4, ...rest }: AppTextAreaProps) => {
  return (
    <TextField
      label={label}
      fullWidth
      multiline
      rows={rows}
      variant="outlined"
      {...rest}
    />
  );
};

export default AppTextArea;
