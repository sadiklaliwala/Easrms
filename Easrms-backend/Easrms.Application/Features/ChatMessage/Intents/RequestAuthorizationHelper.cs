using Easrms.Common.Constants;
using Easrms.Domain.Entities;
using System;

namespace Easrms.Application.Features.ChatMessage.Intents;

public static class RequestAuthorizationHelper
{
    public static bool CanViewRequest(ServiceRequest request, Guid currentUserId, string currentUserRole)
    {
        if (request == null) return false;

        // Admins can see everything
        if (currentUserRole == RoleConstants.Admin)
            return true;

        // Employees can only see their own requests
        if (currentUserRole == RoleConstants.Employee)
            return request.EmployeeId == currentUserId;

        // Managers can see their own requests OR requests raised by employees reporting to them
        if (currentUserRole == RoleConstants.Manager)
        {
            if (request.EmployeeId == currentUserId) 
                return true;
            
            if (request.Employee != null && request.Employee.ManagerId == currentUserId) 
                return true;
                
            return false;
        }

        // Support users can only see requests assigned to them
        if (currentUserRole == RoleConstants.SupportUser)
        {
            return request.AssignedTo == currentUserId;
        }

        return false;
    }
}
