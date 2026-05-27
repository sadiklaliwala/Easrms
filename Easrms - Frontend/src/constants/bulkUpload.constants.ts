import { type BulkUploadConfig } from "../types/bulkUpload.types";

export const userBulkUploadConfig: BulkUploadConfig = {
  title: "users",
  requiredColumns: ["FullName", "Email", "Password", "RoleName", "ManagerEmail"],
  previewColumns: [
    { field: "FullName", headerName: "Full Name", width: 180 },
    { field: "Email", headerName: "Email", width: 220 },
    { field: "RoleName", headerName: "Role", width: 120 },
    { field: "ManagerEmail", headerName: "Manager Email", width: 220 },
  ],
  templateFileName: "users_template.csv",
  uploadMutation: null, // Injected at usage site
  duplicateCheckColumn: "Email",
};

export const categoryBulkUploadConfig: BulkUploadConfig = {
  title: "categories",
  requiredColumns: ["CategoryName", "IsApprovalRequired", "SLAHours"],
  previewColumns: [
    { field: "CategoryName", headerName: "Category Name", width: 200 },
    { field: "IsApprovalRequired", headerName: "Approval Required", width: 160 },
    { field: "SLAHours", headerName: "SLA Hours", width: 120 },
  ],
  templateFileName: "categories_template.csv",
  uploadMutation: null, // Injected at usage site
  duplicateCheckColumn: "CategoryName",
};

export const requestBulkUploadConfig: BulkUploadConfig = {
  title: "requests",
  requiredColumns: ["CategoryName", "Title", "Description", "Priority"],
  previewColumns: [
    { field: "CategoryName", headerName: "Category", width: 160 },
    { field: "Title", headerName: 'Title', width: 200 },
    { field: "Priority", headerName: "Priority", width: 120 },
  ],
  templateFileName: "requests_template.csv",
  uploadMutation: null, // Injected at usage site
  duplicateCheckColumn: "Title",
};
