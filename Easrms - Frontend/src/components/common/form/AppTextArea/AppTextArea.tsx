import { TextField, type TextFieldProps } from "@mui/material";

type AppTextAreaProps = TextFieldProps & {
  label?: string;
  rows?: number;
  maxLength?: number;
};

const AppTextArea = ({
  label,
  rows = 4,
  maxLength,
  helperText,
  ...rest
}: AppTextAreaProps) => {
  const value = rest.value !== undefined && rest.value !== null ? String(rest.value) : "";
  const isLimitReached = maxLength !== undefined && value.length >= maxLength;

  return (
    <TextField
      label={label}
      fullWidth
      multiline
      rows={rows}
      variant="outlined"
      slotProps={{
        htmlInput: {
          maxLength,
        },
      }}
      helperText={
        isLimitReached ? (
          <span style={{ color: "#d32f2f" }}>Maximum limit of {maxLength} characters reached</span>
        ) : (
          helperText
        )
      }
      {...rest}
    />
  );
};

export default AppTextArea;
