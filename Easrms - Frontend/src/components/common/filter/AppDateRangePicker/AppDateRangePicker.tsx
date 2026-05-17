import { Box, TextField } from "@mui/material";
import { useState } from "react";

interface DateRange {
  from: string;
  to: string;
}

interface AppDateRangePickerProps {
  onChange: (range: DateRange) => void;
}

const AppDateRangePicker = ({ onChange }: AppDateRangePickerProps) => {
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");

  const handleFromChange = (value: string) => {
    setFrom(value);
    onChange({ from: value, to });
  };

  const handleToChange = (value: string) => {
    setTo(value);
    onChange({ from, to: value });
  };

  return (
    <Box display="flex" gap={2}>
      <TextField
        label="From"
        type="date"
        size="small"
        value={from}
        onChange={(e) => handleFromChange(e.target.value)}
        InputLabelProps={{ shrink: true }}
      />
      <TextField
        label="To"
        type="date"
        size="small"
        value={to}
        onChange={(e) => handleToChange(e.target.value)}
        InputLabelProps={{ shrink: true }}
      />
    </Box>
  );
};

export default AppDateRangePicker;