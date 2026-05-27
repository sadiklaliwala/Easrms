import { TextField, type TextFieldProps } from "@mui/material";

type AppInputProps = TextFieldProps & {
  maxLength?: number;
};

const AppInput = ({ maxLength, helperText, ...rest }: AppInputProps) => {
  const value = rest.value !== undefined && rest.value !== null ? String(rest.value) : "";
  const isLimitReached = maxLength !== undefined && value.length >= maxLength;

  return (
    <TextField
      fullWidth
      size="small"
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

export default AppInput;
