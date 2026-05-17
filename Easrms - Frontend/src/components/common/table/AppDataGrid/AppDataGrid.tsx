import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  Typography,
} from "@mui/material";
import { ReactNode, useState } from "react";
import { AppEmptyState } from "../../../common/feedback/AppEmptyState";

export interface Column<T> {
  key: keyof T | string;
  label: string;
  sortable?: boolean;
  render?: (row: T) => ReactNode;
  width?: string | number;
}

interface AppDataGridProps<T> {
  columns: Column<T>[];
  rows: T[];
  keyField: keyof T;
  onRowClick?: (row: T) => void;
}

type Order = "asc" | "desc";

function AppDataGrid<T>({
  columns,
  rows,
  keyField,
  onRowClick,
}: AppDataGridProps<T>) {
  const [orderBy, setOrderBy] = useState<string>("");
  const [order, setOrder] = useState<Order>("asc");

  const handleSort = (key: string) => {
    if (orderBy === key) {
      setOrder((prev) => (prev === "asc" ? "desc" : "asc"));
    } else {
      setOrderBy(key);
      setOrder("asc");
    }
  };

  const sortedRows = [...rows].sort((a, b) => {
    if (!orderBy) return 0;
    const aVal = (a as any)[orderBy];
    const bVal = (b as any)[orderBy];
    if (aVal < bVal) return order === "asc" ? -1 : 1;
    if (aVal > bVal) return order === "asc" ? 1 : -1;
    return 0;
  });

  return (
    <TableContainer component={Paper} elevation={1}>
      <Table size="small">
        <TableHead>
          <TableRow sx={{ bgcolor: "grey.50" }}>
            {columns.map((col) => (
              <TableCell
                key={String(col.key)}
                width={col.width}
                sortDirection={orderBy === col.key ? order : false}
              >
                {col.sortable ? (
                  <TableSortLabel
                    active={orderBy === String(col.key)}
                    direction={orderBy === String(col.key) ? order : "asc"}
                    onClick={() => handleSort(String(col.key))}
                  >
                    <Typography variant="body2" fontWeight={600}>
                      {col.label}
                    </Typography>
                  </TableSortLabel>
                ) : (
                  <Typography variant="body2" fontWeight={600}>
                    {col.label}
                  </Typography>
                )}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {sortedRows.length === 0 ? (
            <TableRow>
              <TableCell colSpan={columns.length}>
                <AppEmptyState />
              </TableCell>
            </TableRow>
          ) : (
            sortedRows.map((row) => (
              <TableRow
                key={String(row[keyField])}
                hover
                onClick={() => onRowClick?.(row)}
                sx={{ cursor: onRowClick ? "pointer" : "default" }}
              >
                {columns.map((col) => (
                  <TableCell key={String(col.key)}>
                    {col.render
                      ? col.render(row)
                      : String((row as any)[col.key] ?? "")}
                  </TableCell>
                ))}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

export default AppDataGrid;