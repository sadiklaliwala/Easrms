import { Box } from "@mui/material";
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
  Low: "#10b981", // Emerald 500
  Medium: "#f59e0b", // Amber 500
  High: "#ef4444", // Red 500
};

const AppPriorityChart = ({ data }: AppPriorityChartProps) => {
  return (
    <Box sx={{ width: "100%", height: 250 }}>
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={data}
            dataKey="count"
            nameKey="priority"
            cx="50%"
            cy="50%"
            innerRadius={60}
            outerRadius={80}
            paddingAngle={4}
          >
            {data.map((entry, index) => (
              <Cell key={index} fill={COLORS[entry.priority] ?? "#cbd5e1"} />
            ))}
          </Pie>
          <Tooltip
            contentStyle={{
              borderRadius: 8,
              border: "1px solid #e2e8f0",
              boxShadow: "0 4px 6px -1px rgba(0,0,0,0.05)",
            }}
          />
          <Legend iconType="circle" wrapperStyle={{ fontSize: 12 }} />
        </PieChart>
      </ResponsiveContainer>
    </Box>
  );
};

export default AppPriorityChart;
