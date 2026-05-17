import {
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  SelectProps,
} from "@mui/material";

interface SelectOption {
  label: string;
  value: string | number;
}

type AppSelectProps = Omit<SelectProps, "label"> & {
  label: string;
  options: SelectOption[];
  error?: boolean;
  helperText?: string;
};

const AppSelect = ({
  label,
  options,
  error,
  helperText,
  ...rest
}: AppSelectProps) => {
  const labelId = `${label}-label`;

  return (
    <FormControl fullWidth size="small" error={error}>
      <InputLabel id={labelId}>{label}</InputLabel>
      <Select labelId={labelId} label={label} {...rest}>
        {options.map((opt) => (
          <MenuItem key={opt.value} value={opt.value}>
            {opt.label}
          </MenuItem>
        ))}
      </Select>
      {helperText && <FormHelperText>{helperText}</FormHelperText>}
    </FormControl>
  );
};

export default AppSelect;