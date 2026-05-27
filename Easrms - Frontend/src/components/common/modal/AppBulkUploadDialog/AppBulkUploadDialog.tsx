import { useState, useRef, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Box,
  Typography,
  Button,
  Stack,
  IconButton,
  Chip,
  Alert,
  AlertTitle,
  CircularProgress,
  Card,
  CardContent,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import InsertDriveFileIcon from "@mui/icons-material/InsertDriveFile";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ErrorIcon from "@mui/icons-material/Error";
import WarningIcon from "@mui/icons-material/Warning";
import FileDownloadIcon from "@mui/icons-material/FileDownload";
import toast from "react-hot-toast";
import Papa from "papaparse";
import * as XLSX from "xlsx";
import type {
  BulkUploadError,
  BulkUploadResult,
} from "../../../../types/bulkUpload.types";
import AppDataGrid from "../../table/AppDataGrid";

interface AppBulkUploadDialogProps {
  open: boolean;
  onClose: () => void;
  config: BulkUploadConfig;
}

export default function AppBulkUploadDialog({
  open,
  onClose,
  config,
}: AppBulkUploadDialogProps) {
  const [state, setState] = useState<"idle" | "preview" | "result">("idle");
  const [file, setFile] = useState<File | null>(null);
  const [fileSizeStr, setFileSizeStr] = useState("");
  const [parsedRows, setParsedRows] = useState<any[]>([]);
  const [headers, setHeaders] = useState<string[]>([]);
  const [errorBanner, setErrorBanner] = useState<string | null>(null);
  const [dragOver, setDragOver] = useState(false);

  // Duplicates state
  const [duplicateRows, setDuplicateRows] = useState<
    { row: number; value: string; firstRow: number }[]
  >([]);

  // Mutation upload state
  const [uploadMutation, { isLoading: isUploading }] = config.uploadMutation();
  const [uploadResult, setUploadResult] = useState<BulkUploadResult | null>(
    null,
  );

  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!open) {
      resetState();
    }
  }, [open]);

  const resetState = () => {
    setState("idle");
    setFile(null);
    setFileSizeStr("");
    setParsedRows([]);
    setHeaders([]);
    setErrorBanner(null);
    setDuplicateRows([]);
    setUploadResult(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleClose = () => {
    resetState();
    onClose();
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return bytes + " Bytes";
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + " KB";
    return (bytes / 1048576).toFixed(1) + " MB";
  };

  const validateAndProcessFile = (selectedFile: File) => {
    setErrorBanner(null);
    setDuplicateRows([]);

    // File type check
    const ext = selectedFile.name.split(".").pop()?.toLowerCase();
    if (ext !== "csv" && ext !== "xlsx") {
      setErrorBanner("Only CSV and Excel files are accepted");
      return;
    }

    // File size check (5 MB)
    if (selectedFile.size > 5 * 1024 * 1024) {
      setErrorBanner("File size exceeds 5MB limit");
      return;
    }

    setFile(selectedFile);
    setFileSizeStr(formatFileSize(selectedFile.size));

    if (ext === "csv") {
      Papa.parse(selectedFile, {
        header: true,
        skipEmptyLines: "greedy",
        complete: (results) => {
          const rawHeaders = results.meta.fields || [];
          processParsedData(selectedFile, rawHeaders, results.data);
        },
        error: () => {
          setErrorBanner("Failed to parse CSV file");
        },
      });
    } else {
      const reader = new FileReader();
      reader.onload = (e) => {
        try {
          const data = new Uint8Array(e.target?.result as ArrayBuffer);
          const workbook = XLSX.read(data, { type: "array" });
          const sheetName = workbook.SheetNames[0];
          const worksheet = workbook.Sheets[sheetName];
          const jsonRows = XLSX.utils.sheet_to_json<any[]>(worksheet, {
            header: 1,
          });

          if (jsonRows.length === 0) {
            setErrorBanner("File contains no data rows");
            return;
          }

          const rawHeaders = (jsonRows[0] || []).map((h) => String(h).trim());
          const dataRows = jsonRows.slice(1).map((row) => {
            const obj: any = {};
            rawHeaders.forEach((h, index) => {
              obj[h] =
                row[index] !== undefined && row[index] !== null
                  ? String(row[index]).trim()
                  : "";
            });
            return obj;
          });

          processParsedData(selectedFile, rawHeaders, dataRows);
        } catch (err) {
          setErrorBanner("Failed to parse Excel file");
        }
      };
      reader.readAsArrayBuffer(selectedFile);
    }
  };

  const processParsedData = (
    fileObj: File,
    parsedHeaders: string[],
    dataRows: any[],
  ) => {
    const trimmedHeaders = parsedHeaders.map((h) => h.trim());

    // 1. Column structure check (case-insensitive)
    const missing = config.requiredColumns.filter(
      (reqCol: any) =>
        !trimmedHeaders.some((h) => h.toLowerCase() === reqCol.toLowerCase()),
    );

    if (missing.length > 0) {
      setErrorBanner(
        `Invalid file structure. Missing columns: ${missing.join(", ")}`,
      );
      return;
    }

    const extra = trimmedHeaders.filter(
      (h) =>
        !config.requiredColumns.some(
          (reqCol) => reqCol.toLowerCase() === h.toLowerCase(),
        ),
    );

    if (extra.length > 0) {
      setErrorBanner(`Unknown columns found: ${extra.join(", ")}`);
      return;
    }

    // 2. Zero data rows check
    if (dataRows.length === 0) {
      setErrorBanner("File contains no data rows");
      return;
    }

    // 3. Client-side duplicate check
    const seen = new Map<string, number>(); // value -> first row number
    const duplicates: { row: number; value: string; firstRow: number }[] = [];

    dataRows.forEach((row, index) => {
      const rowNumber = index + 2; // header is row 1
      const value = (row[config.duplicateCheckColumn] ?? "")
        .toLowerCase()
        .trim();

      if (value === "") return; // Empty values handled by backend required validation

      if (seen.has(value)) {
        duplicates.push({
          row: rowNumber,
          value: row[config.duplicateCheckColumn],
          firstRow: seen.get(value)!,
        });
      } else {
        seen.set(value, rowNumber);
      }
    });

    if (duplicates.length > 0) {
      setDuplicateRows(duplicates);
      return;
    }

    // All clear, move to preview state
    setHeaders(trimmedHeaders);
    setParsedRows(dataRows);
    setState("preview");
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(true);
  };

  const handleDragLeave = () => {
    setDragOver(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      validateAndProcessFile(e.dataTransfer.files[0]);
    }
  };

  const handleBrowseClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      validateAndProcessFile(e.target.files[0]);
    }
  };

  const handleTemplateDownload = () => {
    // Generate CSV template client-side with PapaParse
    const csvContent = Papa.unparse({
      fields: config.requiredColumns,
      data: [],
    });

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", config.templateFileName);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleUpload = async () => {
    if (!file) return;
    const formData = new FormData();
    formData.append("file", file);

    try {
      const res = await uploadMutation(formData).unwrap();
      if (res.success && res.data) {
        setUploadResult(res.data);
        setState("result");
      } else {
        toast.error(res.message ?? "Bulk upload failed");
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? "An error occurred during bulk upload");
    }
  };

  const handleErrorReportDownload = () => {
    if (
      !uploadResult ||
      !uploadResult.errors ||
      uploadResult.errors.length === 0
    )
      return;

    const errorReport = uploadResult.errors.map((err) => ({
      ROW: err.row,
      IDENTIFIER: err.identifier,
      REASON: err.reason,
    }));

    const csvContent = Papa.unparse(errorReport);
    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", `${config.title}_error_report.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  // Convert config preview columns to key/label columns expected by AppDataGrid
  const previewGridColumns = config.previewColumns.map((col) => ({
    key: col.field,
    label: col.headerName,
    width: col.width,
    sortable: false,
  }));

  // Preview first 10 rows
  const previewRows = parsedRows.slice(0, 10).map((row, index) => ({
    ...row,
    rowId: `preview-${index}`,
  }));

  // Error styling helper
  const getErrorStyle = (reason: string) => {
    const lReason = reason.toLowerCase();
    if (lReason.includes("already exists in db")) {
      return {
        borderLeft: "4px solid #fbc02d",
        color: "#b78103",
        bgColor: "#fffde7",
      };
    }
    if (lReason.includes("duplicate")) {
      return {
        borderLeft: "4px solid #ef6c00",
        color: "#ef6c00",
        bgColor: "#fff3e0",
      };
    }
    return {
      borderLeft: "4px solid #d32f2f",
      color: "#d32f2f",
      bgColor: "#ffebee",
    };
  };

  const errorGridColumns = [
    {
      key: "row",
      label: "ROW",
      width: 80,
      sortable: false,
      render: (row: BulkUploadError) => {
        const style = getErrorStyle(row.reason);
        return (
          <Box
            sx={{
              borderLeft: style.borderLeft,
              pl: 1.5,
              py: 0.5,
              color: style.color,
              fontWeight: 500,
            }}
          >
            {row.row}
          </Box>
        );
      },
    },
    {
      key: "identifier",
      label: "IDENTIFIER",
      width: 220,
      sortable: false,
      render: (row: BulkUploadError) => {
        const style = getErrorStyle(row.reason);
        return <Box sx={{ color: style.color }}>{row.identifier}</Box>;
      },
    },
    {
      key: "reason",
      label: "REASON",
      width: 320,
      sortable: false,
      render: (row: BulkUploadError) => {
        const style = getErrorStyle(row.reason);
        return <Box sx={{ color: style.color }}>{row.reason}</Box>;
      },
    },
  ];

  return (
    <Dialog
      open={open}
      onClose={isUploading ? undefined : handleClose}
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          pb: 1,
        }}
      >
        <Typography variant="h6" fontWeight="bold">
          Bulk upload {config.title}
        </Typography>
        <IconButton onClick={handleClose} disabled={isUploading}>
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers sx={{ pb: 3 }}>
        {/* ─── STATE 1: IDLE ─────────────────────────────────────────────────── */}
        {state === "idle" && (
          <Stack spacing={3}>
            {/* Drag & Drop Container */}
            <Box
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              sx={{
                border: "2px dashed",
                borderColor: dragOver ? "success.main" : "grey.400",
                borderRadius: 2,
                p: 5,
                textAlign: "center",
                cursor: "pointer",
                backgroundColor: dragOver
                  ? "rgba(16, 185, 129, 0.04)"
                  : "grey.50",
                transition: "all 0.2s ease-in-out",
                "&:hover": {
                  borderColor: "success.main",
                  backgroundColor: "rgba(16, 185, 129, 0.02)",
                },
              }}
              onClick={handleBrowseClick}
            >
              <input
                type="file"
                ref={fileInputRef}
                style={{ display: "none" }}
                accept=".csv, .xlsx, .xls"
                onChange={handleFileChange}
              />
              <CloudUploadIcon
                sx={{
                  fontSize: 48,
                  color: dragOver ? "success.main" : "grey.500",
                  mb: 2,
                }}
              />
              <Typography
                variant="body1"
                fontWeight="medium"
                color="text.primary"
              >
                Drop a CSV or Excel file here
              </Typography>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ mt: 0.5 }}
              >
                Maximum file size: 5 MB
              </Typography>
            </Box>

            <Stack
              direction="row"
              justifyContent="space-between"
              alignItems="center"
            >
              <Button
                variant="text"
                color="success"
                onClick={(e) => {
                  e.stopPropagation();
                  handleTemplateDownload();
                }}
                startIcon={<FileDownloadIcon />}
                sx={{ textTransform: "none", fontWeight: "bold" }}
              >
                Download CSV template
              </Button>
              <Button
                variant="outlined"
                color="primary"
                onClick={handleBrowseClick}
                sx={{ textTransform: "none" }}
              >
                Browse file
              </Button>
            </Stack>

            {/* Error Banner */}
            {errorBanner && (
              <Alert severity="error" onClose={() => setErrorBanner(null)}>
                {errorBanner}
              </Alert>
            )}

            {/* Client-side Duplicate Warning Table */}
            {duplicateRows.length > 0 && (
              <Stack spacing={2}>
                <Alert
                  severity="warning"
                  icon={<WarningIcon />}
                  action={
                    <Button color="inherit" size="small" onClick={resetState}>
                      Clear
                    </Button>
                  }
                >
                  <AlertTitle sx={{ fontWeight: "bold" }}>
                    Duplicate rows found in file
                  </AlertTitle>
                  <Stack direction="row" spacing={1} sx={{ mt: 1, mb: 1 }}>
                    <Chip
                      size="small"
                      color="warning"
                      label={`${duplicateRows.length} duplicates found`}
                      sx={{ fontWeight: "bold" }}
                    />
                    <Typography
                      variant="caption"
                      sx={{ display: "flex", alignItems: "center" }}
                    >
                      Please fix duplicates in your file and re-upload
                    </Typography>
                  </Stack>
                </Alert>

                <Box
                  sx={{
                    maxHeight: 180,
                    overflowY: "auto",
                    border: "1px solid",
                    borderColor: "warning.light",
                    borderRadius: 1,
                    "& thead tr": { backgroundColor: "#fffde7" },
                    "& thead th": { color: "#b78103", fontWeight: "bold" },
                  }}
                >
                  <AppDataGrid
                    keyField="row"
                    rows={duplicateRows}
                    columns={[
                      { key: "row", label: "ROW", width: 70 },
                      { key: "value", label: "VALUE", width: 220 },
                      {
                        key: "reason",
                        label: "REASON",
                        width: 250,
                        render: (row) => (
                          <Typography variant="body2" color="warning.dark">
                            Duplicate — first seen on row {row.firstRow}
                          </Typography>
                        ),
                      },
                    ]}
                  />
                </Box>
              </Stack>
            )}
          </Stack>
        )}

        {/* ─── STATE 2: PREVIEW ──────────────────────────────────────────────── */}
        {state === "preview" && file && (
          <Stack spacing={3}>
            {/* File Info Card */}
            <Stack
              direction="row"
              justifyContent="space-between"
              alignItems="center"
              sx={{
                p: 2,
                bgcolor: "grey.50",
                border: "1px solid",
                borderColor: "grey.200",
                borderRadius: 1.5,
              }}
            >
              <Stack direction="row" spacing={2} alignItems="center">
                <Box
                  sx={{
                    width: 40,
                    height: 40,
                    borderRadius: 1,
                    bgcolor: "success.light",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    color: "success.main",
                  }}
                >
                  <InsertDriveFileIcon />
                </Box>
                <Box>
                  <Typography
                    variant="body2"
                    fontWeight="bold"
                    noWrap
                    sx={{ maxWidth: 220 }}
                  >
                    {file.name}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {fileSizeStr} &middot; {parsedRows.length} data rows
                  </Typography>
                </Box>
              </Stack>
              <Chip
                color="success"
                label={`${parsedRows.length} ${config.title} detected`}
                sx={{ fontWeight: "bold" }}
              />
            </Stack>

            {/* Preview Grid */}
            <Stack spacing={1}>
              <Typography
                variant="subtitle2"
                fontWeight="bold"
                color="text.secondary"
              >
                File Preview
              </Typography>
              <Box
                sx={{
                  border: "1px solid",
                  borderColor: "grey.200",
                  borderRadius: 1,
                }}
              >
                <AppDataGrid
                  keyField="rowId"
                  rows={previewRows}
                  columns={previewGridColumns}
                />
              </Box>
              <Typography variant="caption" color="text.secondary">
                Showing first 10 of {parsedRows.length} rows
              </Typography>
            </Stack>
          </Stack>
        )}

        {/* ─── STATE 3: RESULT ───────────────────────────────────────────────── */}
        {state === "result" && uploadResult && (
          <Stack spacing={3}>
            {/* Summary Cards */}
            <Stack direction="row" spacing={2}>
              <Card
                sx={{
                  flex: 1,
                  bgcolor: "grey.50",
                  border: "1px solid",
                  borderColor: "grey.200",
                }}
                elevation={0}
              >
                <CardContent sx={{ textAlign: "center", p: "16px !important" }}>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    fontWeight="bold"
                    textTransform="uppercase"
                  >
                    Total Rows
                  </Typography>
                  <Typography variant="h5" fontWeight="bold" sx={{ mt: 0.5 }}>
                    {uploadResult.totalRows}
                  </Typography>
                </CardContent>
              </Card>

              <Card
                sx={{
                  flex: 1,
                  bgcolor: "#e8f5e9",
                  border: "1px solid",
                  borderColor: "#c8e6c9",
                }}
                elevation={0}
              >
                <CardContent sx={{ textAlign: "center", p: "16px !important" }}>
                  <Typography
                    variant="caption"
                    color="success.dark"
                    fontWeight="bold"
                    textTransform="uppercase"
                  >
                    Inserted
                  </Typography>
                  <Typography
                    variant="h5"
                    fontWeight="bold"
                    color="success.main"
                    sx={{ mt: 0.5 }}
                  >
                    {uploadResult.insertedCount}
                  </Typography>
                </CardContent>
              </Card>

              {uploadResult.failedCount > 0 && (
                <Card
                  sx={{
                    flex: 1,
                    bgcolor: "#ffebee",
                    border: "1px solid",
                    borderColor: "#ffcdd2",
                  }}
                  elevation={0}
                >
                  <CardContent
                    sx={{ textAlign: "center", p: "16px !important" }}
                  >
                    <Typography
                      variant="caption"
                      color="error.dark"
                      fontWeight="bold"
                      textTransform="uppercase"
                    >
                      Failed
                    </Typography>
                    <Typography
                      sx={{
                        mt: 0.5,
                        variant: "h5",
                        fontWeight: "bold",
                        color: "error.main",
                      }}
                    >
                      {uploadResult.failedCount}
                    </Typography>
                  </CardContent>
                </Card>
              )}
            </Stack>

            {/* Summary Alert Banner */}
            {uploadResult.failedCount === 0 ? (
              <Alert icon={<CheckCircleIcon />} severity="success">
                All {uploadResult.totalRows} rows inserted successfully
              </Alert>
            ) : uploadResult.insertedCount > 0 ? (
              <Alert icon={<WarningIcon />} severity="warning">
                {uploadResult.insertedCount} inserted.{" "}
                {uploadResult.failedCount} rows failed &mdash; review errors
                below
              </Alert>
            ) : (
              <Alert icon={<ErrorIcon />} severity="error">
                0 rows inserted. {uploadResult.failedCount} rows failed &mdash;
                review the errors below
              </Alert>
            )}

            {/* Error Report List */}
            {uploadResult.failedCount > 0 && (
              <Stack spacing={1.5}>
                <Stack
                  direction="row"
                  justifyContent="space-between"
                  alignItems="center"
                >
                  <Chip
                    size="small"
                    color="error"
                    label={`${uploadResult.failedCount} errors`}
                    sx={{ fontWeight: "bold" }}
                  />
                  <Button
                    variant="outlined"
                    color="error"
                    size="small"
                    startIcon={<FileDownloadIcon />}
                    onClick={handleErrorReportDownload}
                    sx={{ textTransform: "none" }}
                  >
                    Download error report
                  </Button>
                </Stack>

                <Box
                  sx={{
                    maxHeight: 250,
                    overflowY: "auto",
                    border: "1px solid",
                    borderColor: "error.light",
                    borderRadius: 1,
                    "& thead tr": { backgroundColor: "#ffebee" },
                    "& thead th": { color: "#c62828", fontWeight: "bold" },
                  }}
                >
                  <AppDataGrid
                    keyField="row"
                    rows={uploadResult.errors}
                    columns={errorGridColumns}
                  />
                </Box>
              </Stack>
            )}
          </Stack>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        {/* State 1 Actions */}
        {state === "idle" && (
          <Button
            onClick={handleClose}
            variant="outlined"
            sx={{ textTransform: "none" }}
          >
            Close
          </Button>
        )}

        {/* State 2 Actions */}
        {state === "preview" && (
          <Stack
            direction="row"
            spacing={2}
            justifyContent="flex-end"
            width="100%"
          >
            <Button
              variant="outlined"
              color="inherit"
              onClick={resetState}
              disabled={isUploading}
              sx={{ textTransform: "none" }}
            >
              Cancel
            </Button>
            <Button
              variant="contained"
              color="success"
              onClick={handleUpload}
              disabled={isUploading}
              sx={{ textTransform: "none", minWidth: 100, fontWeight: "bold" }}
            >
              {isUploading ? (
                <CircularProgress size={20} color="inherit" />
              ) : (
                "Upload"
              )}
            </Button>
          </Stack>
        )}

        {/* State 3 Actions */}
        {state === "result" && (
          <Stack
            direction="row"
            spacing={2}
            justifyContent="flex-end"
            width="100%"
          >
            <Button
              variant="outlined"
              onClick={handleClose}
              sx={{ textTransform: "none" }}
            >
              Close
            </Button>
            <Button
              variant="contained"
              color="success"
              onClick={resetState}
              sx={{ textTransform: "none", fontWeight: "bold" }}
            >
              Upload another
            </Button>
          </Stack>
        )}
      </DialogActions>
    </Dialog>
  );
}
