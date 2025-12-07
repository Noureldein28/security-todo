using AutoMapper;
using SecureTodo.Application.DTOs;
using SecureTodo.Domain.Entities;

namespace SecureTodo.Application.Mappings;

/// <summary>
/// AutoMapper profile for entity-to-DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        
        // Todo mappings
        CreateMap<Todo, TodoDto>()
            .ForMember(dest => dest.Content, opt => opt.Ignore()) // Content is decrypted separately
            .ForMember(dest => dest.Tampered, opt => opt.Ignore());
    }
}
