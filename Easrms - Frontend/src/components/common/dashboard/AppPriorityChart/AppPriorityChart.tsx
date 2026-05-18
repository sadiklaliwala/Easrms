import { Box, Typography } from "@mui/material";
import {
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
} from "recharts";

interface PriorityChartItem {
  priority: string;
  count: number;
}

interface AppPriorityChartProps {
  data: PriorityChartItem[];
}

const COLORS: Record<string, string> = {
  Low: "#4caf50",
  Medium: "#ff9800",
  High: "#f44336",
};

const AppPriorityChart = ({ data }: AppPriorityChartProps) => {
  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 2, ml: 2 }}>
        Requests by Priority
      </Typography>
      <ResponsiveContainer width="100%" height={250}>
        <PieChart>
          <Pie
            data={data}
            dataKey="count"
            nameKey="priority"
            cx="50%"
            cy="50%"
            outerRadius={80}
            label
          >
            {data.map((entry, index) => (
              <Cell key={index} fill={COLORS[entry.priority] ?? "#8884d8"} />
            ))}
          </Pie>
          <Tooltip />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </Box>
  );
};

export default AppPriorityChart;
