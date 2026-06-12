using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services;

/// <summary>
/// Punto unico para emitir eventos de tiempo real a los clientes conectados.
/// Evento "ordenes:cambio" con { evento, ordenId }; los clientes refrescan
/// su lista al recibirlo (payload minimo a proposito).
/// </summary>
public class RealtimeNotifier
{
    private readonly IHubContext<RestauranteHub> _hub;

    public RealtimeNotifier(IHubContext<RestauranteHub> hub)
    {
        _hub = hub;
    }

    /// <param name="evento">nueva | actualizada | lista | pagada | cancelada</param>
    public async Task OrdenCambioAsync(string evento, string ordenId)
    {
        try
        {
            await _hub.Clients.All.SendAsync("ordenes:cambio", new { evento, ordenId });
        }
        catch
        {
            // La notificacion nunca debe tumbar la operacion principal
        }
    }
}
