import { Box, Typography } from "@mui/material";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

interface CategoryChartItem {
  categoryName: string;
  count: number;
}

interface AppCategoryChartProps {
  data: CategoryChartItem[];
}

const AppCategoryChart = ({ data }: AppCategoryChartProps) => {
  return (
    <Box>
      <Typography variant="subtitle1" fontWeight={600} mb={2}>
        Requests by Category
      </Typography>
      <ResponsiveContainer width="100%" height={250}>
        <BarChart
          data={data}
          layout="vertical"
          margin={{ top: 5, right: 20, left: 80, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis type="number" allowDecimals={false} tick={{ fontSize: 12 }} />
          <YAxis
            type="category"
            dataKey="categoryName"
            tick={{ fontSize: 12 }}
          />
          <Tooltip />
          <Bar dataKey="count" fill="#9c27b0" radius={[0, 4, 4, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </Box>
  );
};

export default AppCategoryChart;