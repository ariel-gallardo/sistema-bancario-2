using ClienteCuentaService.Domain.Models;
using Mapster;

namespace ClienteCuentaService.Application.Mapping;

internal static class ClienteCuentaMappingConfig
{
    private static bool _configured;

    public static void EnsureConfigured()
    {
        if (_configured)
        {
            return;
        }

        TypeAdapterConfig<ClienteResumen, Dtos.ResumenClienteDto>.NewConfig();
        TypeAdapterConfig<MovimientoResumen, Dtos.MovimientoResumenDto>.NewConfig();

        _configured = true;
    }
}
