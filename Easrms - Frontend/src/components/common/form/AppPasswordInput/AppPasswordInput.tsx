import { useState } from "react";
import {
  IconButton,
  InputAdornment,
  TextField,
  type TextFieldProps,
} from "@mui/material";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";

type AppPasswordInputProps = TextFieldProps & {
  label?: string;
  maxLength?: number;
};

const AppPasswordInput = ({
  label = "Password",
  maxLength,
  helperText,
  ...rest
}: AppPasswordInputProps) => {
  const [show, setShow] = useState(false);
  const value = rest.value !== undefined && rest.value !== null ? String(rest.value) : "";
  const isLimitReached = maxLength !== undefined && value.length >= maxLength;

  return (
    <TextField
      // label={label}
      type={show ? "text" : "password"}
      fullWidth
      slotProps={{
        input: {
          endAdornment: (
            <InputAdornment position="end">
              <IconButton onClick={() => setShow((prev) => !prev)} edge="end">
                {show ? <VisibilityOff /> : <Visibility />}
              </IconButton>
            </InputAdornment>
          ),
        },
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

export default AppPasswordInput;
