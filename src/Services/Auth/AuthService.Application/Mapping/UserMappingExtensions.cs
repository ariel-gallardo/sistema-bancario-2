using System;
using AuthService.Application.Dtos;
using AuthService.Domain.Entities;
using Mapster;

namespace AuthService.Application.Mapping;

public static class UserMappingExtensions
{
    static UserMappingExtensions()
    {
        TypeAdapterConfig<Usuario, UserRolesDto>
            .NewConfig()
            .Map(dest => dest.Roles, src => string.IsNullOrWhiteSpace(src.Roles)
                ? Array.Empty<string>()
                : src.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public static UserRolesDto ToRolesDto(this Usuario source) => source.Adapt<UserRolesDto>();
}
