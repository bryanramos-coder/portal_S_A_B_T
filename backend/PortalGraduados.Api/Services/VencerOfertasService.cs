using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Helpers;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Services;

public class VencerOfertasService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VencerOfertasService> _logger;

    public VencerOfertasService(IServiceScopeFactory scopeFactory, ILogger<VencerOfertasService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VencerOfertasExpiradasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vencer ofertas expiradas.");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task VencerOfertasExpiradasAsync(CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();

        EstadoOfertum? estadoPublicada = await db.EstadoOferta
            .FirstOrDefaultAsync(e => e.Nombre == Constantes.OfertaPublicada, ct);
        EstadoOfertum? estadoVencida = await db.EstadoOferta
            .FirstOrDefaultAsync(e => e.Nombre == Constantes.OfertaVencida, ct);

        if (estadoPublicada == null || estadoVencida == null) return;

        DateOnly hoy = Validaciones.HoyLocal();
        await db.OfertaLaborals
            .Where(o => o.IdEstadoOferta == estadoPublicada.IdEstadoOferta && o.FechaVencimiento < hoy)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.IdEstadoOferta, estadoVencida.IdEstadoOferta), ct);
    }
}
