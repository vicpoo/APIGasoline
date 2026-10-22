using APIGasoline.Infrastructure.Storage;

namespace APIGasoline.Configuration;

/// <summary>
/// Lee el host de la petición HTTP actual. Con Minio:PublicEndpoint = "auto", las URLs de las fotos
/// se firman con este mismo host, así funcionan igual en Postman (localhost), en el emulador
/// (10.0.2.2) y en un celular de la red (IP de la PC) sin cambiar la configuración.
/// </summary>
public class HostPeticionActual : IHostPeticionActual
{
    private readonly IHttpContextAccessor _accessor;

    public HostPeticionActual(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? Host => _accessor.HttpContext?.Request.Host.Host;
}
