import { useEffect } from "react";
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  HttpTransportType,
} from "@microsoft/signalr";
import toast from "react-hot-toast";
import { useAppDispatch, useAppSelector } from "./useAppSelector";
import { addNotification } from "../store/slices/notificationSlice";
import { api } from "../store/api/api";

export const useNotificationHub = () => {
  const dispatch = useAppDispatch();
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);

  useEffect(() => {
    if (!isAuthenticated) return;

    let backendUrl = import.meta.env.VITE_API_BASE_URL || window.location.origin;
    // Strip trailing '/api' or '/api/' if present
    backendUrl = backendUrl.replace(/\/api\/?$/, "");
    const fullHubUrl = `${backendUrl}/hubs/notification`;

    const connection = new HubConnectionBuilder()
      .withUrl(fullHubUrl, {
        accessTokenFactory: () => localStorage.getItem("accessToken") || "",
        transport: HttpTransportType.LongPolling,
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    const handleNotification = (message: string, type: string) => {
      dispatch(addNotification({ message, type }));

      // Auto-refresh RTK Query caches
      dispatch(
        api.util.invalidateTags([
          "Request",
          "RequestList",
          "RequestDetail",
          "Dashboard",
          "History",
          "Comment",
        ]),
      );

      // Show toast
      toast(message, {
        position: "top-right",
        duration: 5000,
      });
    };

    // Listeners with camelCase and PascalCase fallback support
    connection.on("NewRequestPendingApproval", (data: any) => {
      const requestNumber = data?.requestNumber ?? data?.RequestNumber ?? "";
      const title = data?.title ?? data?.Title ?? "";
      const employeeName = data?.employeeName ?? data?.EmployeeName ?? "";
      const titlePart = title ? ` ("${title}")` : "";
      const msg = `New Request #${requestNumber}${titlePart} submitted by ${employeeName} is pending approval.`;
      handleNotification(msg, "NewRequestPendingApproval");
    });

    connection.on("NewRequestOpen", (data: any) => {
      const requestNumber = data?.requestNumber ?? data?.RequestNumber ?? "";
      const title = data?.title ?? data?.Title ?? "";
      const titlePart = title ? ` ("${title}")` : "";
      const msg = `New Open Request #${requestNumber}${titlePart} created.`;
      handleNotification(msg, "NewRequestOpen");
    });

    connection.on("RequestApproved", (data: any) => {
      const requestNumber = data?.requestNumber ?? data?.RequestNumber ?? "";
      const title = data?.title ?? data?.Title ?? "";
      const titlePart = title ? ` ("${title}")` : "";
      const msg = `Request #${requestNumber}${titlePart} has been approved.`;
      handleNotification(msg, "RequestApproved");
    });

    connection.on("RequestRejected", (data: any) => {
      const requestNumber = data?.requestNumber ?? data?.RequestNumber ?? "";
      const title = data?.title ?? data?.Title ?? "";
      const rejectionReason =
        data?.rejectionReason ?? data?.RejectionReason ?? "";
      const titlePart = title ? ` ("${title}")` : "";
      const reasonSuffix = rejectionReason ? ` Reason: ${rejectionReason}` : "";
      const msg = `Request #${requestNumber}${titlePart} was rejected.${reasonSuffix}`;
      handleNotification(msg, "RequestRejected");
    });

    connection.on("RequestAssigned", (data: any) => {
      const requestNumber = data?.requestNumber ?? data?.RequestNumber ?? "";
      const title = data?.title ?? data?.Title ?? "";
      const titlePart = title ? ` ("${title}")` : "";
      const msg = `Request #${requestNumber}${titlePart} was assigned to you.`;
      handleNotification(msg, "RequestAssigned");
    });

    connection.on("RequestStatusUpdated", (data: any) => {
      const requestNumber = data?.requestNumber ?? data?.RequestNumber ?? "";
      const status =
        data?.newStatus ??
        data?.NewStatus ??
        data?.status ??
        data?.Status ??
        "";
      const msg = `Request #${requestNumber} status updated to ${status}.`;
      handleNotification(msg, "RequestStatusUpdated");
    });

    connection.on("RequestClosed", (data: any) => {
      const requestNumber = data?.requestNumber ?? data?.RequestNumber ?? "";
      const msg = `Request #${requestNumber} is now closed.`;
      handleNotification(msg, "RequestClosed");
    });

    // Start connection
    connection
      .start()
      .then(() => {
        console.log("⚡ [SignalR] Connected to Notification Hub");
      })
      .catch((err) => {
        console.error("⚡ [SignalR] Connection Error: ", err);
      });

    return () => {
      if (
        connection.state === HubConnectionState.Connected ||
        connection.state === HubConnectionState.Connecting
      ) {
        connection
          .stop()
          .then(() => console.log("🔌 [SignalR] Disconnected cleanly"))
          .catch((err) => console.error("🔌 [SignalR] Stop Error: ", err));
      }
    };
  }, [isAuthenticated, dispatch]);
};
