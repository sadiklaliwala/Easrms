import { ROLES } from "../../../../constants/role.constants";

export const ChatHelpers = {
  generateWelcomeMessage: (fullName: string, roleName: string): string => {
    let examples = "show my requests, check status of REQ-0001";

    if (roleName === ROLES.MANAGER) {
      examples = "show my pending approvals, show dashboard summary";
    } else if (roleName === ROLES.ADMIN) {
      examples = "show dashboard summary, show open requests";
    } else if (roleName === ROLES.SUPPORT_USER) {
      examples = "show my tasks, show assigned requests";
    }

    return `Hi ${fullName || "there"}! I am your EASRMS assistant. You can ask me things like: ${examples}.`;
  },

  generateHelpMessage: (roleName: string): string => {
    let helpText = "Here is what you can ask me to do:\n• Show my requests\n• Show details for a request (e.g., 'What is REQ-0001?')\n• Show request history or comments";
    
    if (roleName === ROLES.MANAGER) {
      helpText += "\n• Show my pending approvals\n• Show dashboard summary";
    } else if (roleName === ROLES.ADMIN) {
      helpText += "\n• Show dashboard summary\n• Show open requests";
    } else if (roleName === ROLES.SUPPORT_USER) {
      helpText += "\n• Show my assigned tasks";
    }

    return helpText;
  },

  isHelpCommand: (text: string): boolean => {
    const lowerText = text.trim().toLowerCase();
    return lowerText === "help" || lowerText === "what can i do" || lowerText === "what can you do";
  }
};
