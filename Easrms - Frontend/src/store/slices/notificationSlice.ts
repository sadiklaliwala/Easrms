import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export interface AppNotification {
  id: string;
  message: string;
  type: string;
  createdAt: string;
  isRead: boolean;
}

interface NotificationState {
  notifications: AppNotification[];
  unreadCount: number;
}

const initialState: NotificationState = {
  notifications: [],
  unreadCount: 0,
};

const notificationSlice = createSlice({
  name: "notification",
  initialState,
  reducers: {
    addNotification: (
      state,
      action: PayloadAction<Omit<AppNotification, "id" | "createdAt" | "isRead">>
    ) => {
      const newNotification: AppNotification = {
        ...action.payload,
        id: crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2, 9),
        createdAt: new Date().toISOString(),
        isRead: false,
      };
      state.notifications.unshift(newNotification);
      state.unreadCount += 1;
    },
    markAsRead: (state, action: PayloadAction<string>) => {
      const notif = state.notifications.find((n) => n.id === action.payload);
      if (notif && !notif.isRead) {
        notif.isRead = true;
        state.unreadCount = Math.max(0, state.unreadCount - 1);
      }
    },
    markAllAsRead: (state) => {
      state.notifications.forEach((n) => {
        n.isRead = true;
      });
      state.unreadCount = 0;
    },
    clearNotifications: (state) => {
      state.notifications = [];
      state.unreadCount = 0;
    },
  },
});

export const { addNotification, markAsRead, markAllAsRead, clearNotifications } =
  notificationSlice.actions;
export default notificationSlice.reducer;
