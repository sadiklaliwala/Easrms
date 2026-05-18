import { FormLabel, type FormLabelProps } from "@mui/material";

interface AppLabelProps extends FormLabelProps {
  label: string;
  required?: boolean;
}

const AppLabel = ({ label, required, ...rest }: AppLabelProps) => {
  return (
    <FormLabel {...rest}>
      {label}
      {required && <span style={{ color: "red", marginLeft: 2 }}>*</span>}
    </FormLabel>
  );
};

export default AppLabel;