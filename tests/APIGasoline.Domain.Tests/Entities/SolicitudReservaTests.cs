using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Flujo de la solicitud de reserva: nace pendiente; solo una pendiente se aprueba o rechaza;
/// nadie resuelve su propia solicitud; rechazar exige motivo; se cancela si está pendiente o aprobada.
/// </summary>
public class SolicitudReservaTests
{
    private const int Solicitante = 1;
    private const int Confirmador = 2;
    private static readonly DateTime Fecha = new(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc);

    // --- Crear ---

    [Fact]
    public void Crear_DatosValidos_QuedaPendiente()
    {
        var solicitud = ConstruirSolicitud();

        Assert.Equal(EstadoSolicitudReserva.Pendiente, solicitud.Estado);
        Assert.True(solicitud.EstaPendiente);
        Assert.Null(solicitud.EmpleadoConfirmadorId);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int empresaId, int carroId, int solicitanteId)
    {
        Assert.Throws<DomainException>(() => SolicitudReserva.Crear(empresaId, carroId, solicitanteId));
    }

    // --- Aprobar ---

    [Fact]
    public void Aprobar_Pendiente_QuedaAprobadaConConfirmadorYFecha()
    {
        var solicitud = ConstruirSolicitud();

        solicitud.Aprobar(Confirmador, Fecha, "  Viaje a Tapachula  ");

        Assert.Equal(EstadoSolicitudReserva.Aprobada, solicitud.Estado);
        Assert.Equal(Confirmador, solicitud.EmpleadoConfirmadorId);
        Assert.Equal(Fecha, solicitud.FechaResolucion);
        Assert.Equal("Viaje a Tapachula", solicitud.MotivoResolucion);
    }

    [Fact]
    public void Aprobar_SinMotivo_SeAcepta()
    {
        var solicitud = ConstruirSolicitud();

        solicitud.Aprobar(Confirmador, Fecha);

        Assert.True(solicitud.EstaAprobada);
        Assert.Null(solicitud.MotivoResolucion);
    }

    [Fact]
    public void Aprobar_PropiaSolicitud_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();

        Assert.Throws<DomainException>(() => solicitud.Aprobar(Solicitante, Fecha));
    }

    [Fact]
    public void Aprobar_ConfirmadorCero_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();

        Assert.Throws<DomainException>(() => solicitud.Aprobar(0, Fecha));
    }

    [Fact]
    public void Aprobar_YaAprobada_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();
        solicitud.Aprobar(Confirmador, Fecha);

        Assert.Throws<DomainException>(() => solicitud.Aprobar(Confirmador, Fecha));
    }

    [Fact]
    public void Aprobar_Rechazada_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();
        solicitud.Rechazar(Confirmador, Fecha, "No hay viajes programados");

        Assert.Throws<DomainException>(() => solicitud.Aprobar(Confirmador, Fecha));
    }

    // --- Rechazar ---

    [Fact]
    public void Rechazar_ConMotivo_QuedaRechazada()
    {
        var solicitud = ConstruirSolicitud();

        solicitud.Rechazar(Confirmador, Fecha, "No hay viajes programados");

        Assert.Equal(EstadoSolicitudReserva.Rechazada, solicitud.Estado);
        Assert.Equal("No hay viajes programados", solicitud.MotivoResolucion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rechazar_SinMotivo_LanzaDomainException(string motivo)
    {
        var solicitud = ConstruirSolicitud();

        Assert.Throws<DomainException>(() => solicitud.Rechazar(Confirmador, Fecha, motivo));
    }

    [Fact]
    public void Rechazar_PropiaSolicitud_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();

        Assert.Throws<DomainException>(() => solicitud.Rechazar(Solicitante, Fecha, "Motivo"));
    }

    [Fact]
    public void Rechazar_MotivoMayorA255_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();

        Assert.Throws<DomainException>(() => solicitud.Rechazar(Confirmador, Fecha, new string('x', 256)));
    }

    // --- Cancelar ---

    [Fact]
    public void Cancelar_Pendiente_QuedaCancelada()
    {
        var solicitud = ConstruirSolicitud();

        solicitud.Cancelar(Fecha, "Ya no se necesita");

        Assert.Equal(EstadoSolicitudReserva.Cancelada, solicitud.Estado);
        Assert.Equal(Fecha, solicitud.FechaResolucion);
    }

    [Fact]
    public void Cancelar_Aprobada_QuedaCancelada()
    {
        var solicitud = ConstruirSolicitud();
        solicitud.Aprobar(Confirmador, Fecha);

        solicitud.Cancelar(Fecha.AddHours(1));

        Assert.Equal(EstadoSolicitudReserva.Cancelada, solicitud.Estado);
    }

    [Fact]
    public void Cancelar_Rechazada_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();
        solicitud.Rechazar(Confirmador, Fecha, "Motivo");

        Assert.Throws<DomainException>(() => solicitud.Cancelar(Fecha));
    }

    [Fact]
    public void Cancelar_YaCancelada_LanzaDomainException()
    {
        var solicitud = ConstruirSolicitud();
        solicitud.Cancelar(Fecha);

        Assert.Throws<DomainException>(() => solicitud.Cancelar(Fecha));
    }

    private static SolicitudReserva ConstruirSolicitud(int empresaId = 1, int carroId = 1, int solicitanteId = Solicitante)
        => SolicitudReserva.Crear(empresaId, carroId, solicitanteId);
}
