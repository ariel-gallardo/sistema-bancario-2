using System.Collections.Generic;
using Mapster;
using MovimientoService.Application.Dtos;
using MovimientoService.Domain.Entities;

namespace MovimientoService.Application.Mapping;

public static class MovimientoMappingExtensions
{
    static MovimientoMappingExtensions()
    {
        TypeAdapterConfig<Movimiento, MovimientoDto>.NewConfig();
    }

    public static IReadOnlyCollection<MovimientoDto> ToDtoList(this IEnumerable<Movimiento> source) => source.Adapt<IReadOnlyCollection<MovimientoDto>>();
}
