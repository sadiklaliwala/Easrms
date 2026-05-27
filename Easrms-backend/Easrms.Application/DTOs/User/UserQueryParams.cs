using System;
using System.Collections.Generic;
using System.Text;

namespace Easrms.Application.DTOs.User
{
    public class UserQueryParams
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public Guid? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "CreatedOn";
        public bool SortAscending { get; set; } = false;
        public string? SortDirection { get; set; } = null;
    }
}
