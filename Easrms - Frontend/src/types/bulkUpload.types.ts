export interface BulkUploadError {
  row: number;
  identifier: string;
  reason: string;
}

export interface BulkUploadResult {
  totalRows: number;
  insertedCount: number;
  failedCount: number;
  errors: BulkUploadError[];
}

export interface BulkUploadConfig {
  title: string;
  // exact column names matching backend expectation
  requiredColumns: string[];
  // columns to show in preview table (subset of requiredColumns)
  previewColumns: { field: string; headerName: string; width: number }[];
  // template file name for download
  templateFileName: string;
  // RTK mutation hook to call on upload
  uploadMutation: any;
  duplicateCheckColumn: string;
}
