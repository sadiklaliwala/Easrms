using AutoMapper;
using Easrms.Application.DTOs.Auth;
using Easrms.Application.DTOs.Category;
using Easrms.Application.DTOs.Comment;
using Easrms.Application.DTOs.Request;
using Easrms.Application.DTOs.User;
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
            .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : string.Empty));

        // Comment mappings
        CreateMap<RequestComment, CommentListDto>()
            .ForMember(dest => dest.CommentByName, opt => opt.MapFrom(src => src.CommentByUser != null ? src.CommentByUser.FullName : string.Empty));

        // Status history mappings
        CreateMap<RequestStatusHistory, StatusHistoryDto>()
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.FullName : string.Empty));
    }
}