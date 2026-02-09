using System.Collections.Generic;
using ClienteCuentaService.Application.Dtos;
using ClienteCuentaService.Domain.Models;
using Mapster;

namespace ClienteCuentaService.Application.Mapping;

public static class ClienteCuentaMappingExtensions
{
    static ClienteCuentaMappingExtensions()
    {
        ClienteCuentaMappingConfig.EnsureConfigured();
    }

    public static ResumenClienteDto ToDto(this ClienteResumen source) => source.Adapt<ResumenClienteDto>();

    public static IReadOnlyCollection<MovimientoResumenDto> ToDto(this IEnumerable<MovimientoResumen> source) => source.Adapt<IReadOnlyCollection<MovimientoResumenDto>>();
}
