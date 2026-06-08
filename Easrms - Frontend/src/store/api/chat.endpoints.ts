import { api } from "./api";
import type { ChatMessageRequest, ChatMessageResponse } from "../../types/chat.types";
import ApiEndPoints from "../ApiEndPoints";

export const chatApi = api.injectEndpoints({
  endpoints: (builder) => ({
    sendChatMessage: builder.mutation<ChatMessageResponse, ChatMessageRequest>({
      query: (data) => ({
        url: ApiEndPoints.CHAT.MESSAGE,
        method: "POST",
        body: data,
      }),
    }),
  }),
});

export const { useSendChatMessageMutation } = chatApi;
