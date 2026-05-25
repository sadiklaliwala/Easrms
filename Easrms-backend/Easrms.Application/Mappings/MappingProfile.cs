using AutoMapper;
using Easrms.Application.DTOs.Auth;
using Easrms.Application.DTOs.Category;
using Easrms.Application.DTOs.Comment;
using Easrms.Application.DTOs.Request;
using Easrms.Application.DTOs.User;
using Easrms.Application.Helpers;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;

namespace Easrms.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserListDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));

        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
            .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.FullName : string.Empty));

        CreateMap<User, CurrentUserDto>()
             .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
             //.ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId!=null ? src.ManagerId : string.Empty));

        // Category mappings
        CreateMap<RequestCategory, CategoryListDto>();
        CreateMap<RequestCategory, CategoryDetailDto>();

        // ServiceRequest mappings
        CreateMap<ServiceRequest, RequestListDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : string.Empty));

        CreateMap<ServiceRequest, RequestDetailDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : string.Empty))
            .ForMember(d => d.EscalatedByName, o => o.MapFrom(s => s.Escalator != null ? s.Escalator.FullName : null))
            .ForMember(d => d.SLAStatus, o => o.MapFrom(s => SlaHelper.Calculate(s.Status, s.DueDate)))
            .ForMember(dest => dest.AttachmentUrl, opt => opt.MapFrom(src => src.AttachmentUrl));

        // Comment mappings
        CreateMap<RequestComment, CommentListDto>()
            .ForMember(dest => dest.CommentByName, opt => opt.MapFrom(src => src.CommentByUser != null ? src.CommentByUser.FullName : string.Empty));

        // Status history mappings
        CreateMap<RequestStatusHistory, StatusHistoryDto>()
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.FullName : string.Empty));
    }
}