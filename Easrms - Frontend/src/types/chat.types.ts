export interface ChatMessageRequest {
  message: string;
}

export interface ChatMessageResponse {
  success: boolean;
  statusCode: number;
  message: string;
  data: {
    reply: string;
    intent: string;
  };
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'bot';
  text: string;
  timestamp: Date;
  intent?: string;
}
