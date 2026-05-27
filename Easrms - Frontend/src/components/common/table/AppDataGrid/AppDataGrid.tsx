import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
} from "@mui/material";
import { type ReactNode, useState } from "react";
import { type GridSortModel } from "@mui/x-data-grid";
import AppEmptyState from "../../../common/feedback/AppEmptyState";

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
  onSortChange?: (sortBy: string, sortAscending: boolean) => void;
  sortBy?: string;
  sortAscending?: boolean;
}

type Order = "asc" | "desc";

function AppDataGrid<T>({
  columns,
  rows,
  keyField,
  onRowClick,
  onSortChange,
  sortBy,
  sortAscending,
}: AppDataGridProps<T>) {
  const [localOrderBy, setLocalOrderBy] = useState<string>("");
  const [localOrder, setLocalOrder] = useState<Order>("asc");

  const orderBy = sortBy !== undefined ? sortBy : localOrderBy;
  const order =
    sortAscending !== undefined
      ? sortAscending
        ? "asc"
        : "desc"
      : localOrder;

  const handleSort = (key: string) => {
    const isAsc = orderBy === key && order === "asc";
    const newOrder: Order = isAsc ? "desc" : "asc";
    if (sortBy === undefined) {
      setLocalOrderBy(key);
      setLocalOrder(newOrder);
    }
    if (onSortChange) {
      onSortChange(key, newOrder === "asc");
    }
  };

  const handleSortModelChange = (model: GridSortModel) => {
    if (!onSortChange) return;
    if (model.length === 0) {
      onSortChange("createdOn", false);
    } else {
      onSortChange(model[0].field, model[0].sort === "asc");
    }
  };

  const sortedRows = onSortChange
    ? rows
    : [...rows].sort((a, b) => {
        if (!orderBy) return 0;
        const aVal = (a as any)[orderBy];
        const bVal = (b as any)[orderBy];
        if (aVal < bVal) return order === "asc" ? -1 : 1;
        if (aVal > bVal) return order === "asc" ? 1 : -1;
        return 0;
      });

  return (
    <TableContainer component={Paper} elevation={0}>
      <Table
        size="small"
        {...({
          sortingMode: "server",
          onSortModelChange: handleSortModelChange,
        } as any)}
      >
        <TableHead>
          <TableRow>
            {columns.map((col) => (
              <TableCell
                key={String(col.key)}
                width={col.width}
                sortDirection={orderBy === col.key ? order : false}
              >
                {col.sortable !== false ? (
                  <TableSortLabel
                    active={orderBy === String(col.key)}
                    direction={orderBy === String(col.key) ? order : "asc"}
                    onClick={() => handleSort(String(col.key))}
                  >
                    {col.label}
                  </TableSortLabel>
                ) : (
                  col.label
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
                sx={{
                  cursor: onRowClick ? "pointer" : "default",
                  transition: "background-color 0.15s ease-in-out",
                }}
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
