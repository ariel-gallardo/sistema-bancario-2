using System.Collections.Generic;
using Mapster;
using TarjetaService.Application.Dtos;
using TarjetaService.Domain.Entities;

namespace TarjetaService.Application.Mapping;

public static class TarjetaMappingExtensions
{
    static TarjetaMappingExtensions()
    {
        TypeAdapterConfig<Tarjeta, TarjetaDto>.NewConfig();
    }

    public static TarjetaDto? ToDto(this Tarjeta? source) => source?.Adapt<TarjetaDto>();

    public static IReadOnlyCollection<TarjetaDto> ToDtoList(this IEnumerable<Tarjeta> source) => source.Adapt<IReadOnlyCollection<TarjetaDto>>();
}
