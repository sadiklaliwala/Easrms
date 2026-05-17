import {
  Checkbox,
  CheckboxProps,
  FormControlLabel,
} from "@mui/material";

type AppCheckboxProps = CheckboxProps & {
  label: string;
};

const AppCheckbox = ({ label, ...rest }: AppCheckboxProps) => {
  return (
    <FormControlLabel
      control={<Checkbox {...rest} />}
      label={label}
    />
  );
};

export default AppCheckbox;