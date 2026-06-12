using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

/// <summary>
/// Hub de difusion en tiempo real. El servidor empuja eventos cuando cambian
/// las ordenes; los clientes (POS, cocina) solo escuchan y refrescan.
/// Anonimo, igual que CocinaController: el monitor de cocina no tiene login.
/// </summary>
public class RestauranteHub : Hub
{
}
