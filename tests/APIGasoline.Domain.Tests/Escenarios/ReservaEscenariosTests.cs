using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Escenarios;

/// <summary>
/// Escenarios de punta a punta de la reserva de un carro, componiendo las entidades reales
/// en el mismo orden que AprobarSolicitudReservaCommand y CancelarSolicitudReservaCommand:
/// solicitud → aprobación (ocupa el carro + bitácora) → cancelación (libera el carro + bitácora).
/// </summary>
public class ReservaEscenariosTests
{
    private static readonly Guid Solicitante = Id(1);
    private static readonly Guid OtroSolicitante = Id(3);
    private static readonly Guid Confirmador = Id(2);
    private static readonly DateTime Ahora = new(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc);

    // --- Aprobar ocupa el carro y deja registro en la bitácora ---

    [Fact]
    public void Aprobar_CarroDisponible_QuedaOcupadoYSeRegistraElCambio()
    {
        var carro = ConstruirCarro();
        var solicitud = SolicitudReserva.Crear(Id(1), Id(1), Solicitante, "Visita a cliente");

        solicitud.Aprobar(Confirmador, Ahora, "Viaje a Tapachula");
        var cambio = CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Asignado por la solicitud de reserva.", fechaCambio: Ahora);

        Assert.True(solicitud.EstaAprobada);
        Assert.Equal(EstadoCarro.Ocupado, carro.Estado);
        Assert.Equal(EstadoCarro.Disponible, cambio.EstatusAnterior);
        Assert.Equal(EstadoCarro.Ocupado, cambio.EstatusNuevo);
    }

    // --- Precedencia: la primera aprobada gana, las demás pendientes del carro se rechazan ---

    [Fact]
    public void Aprobar_ConOtraSolicitudPendienteDelMismoCarro_LaOtraSeRechazaConMotivo()
    {
        var carro = ConstruirCarro();
        var aprobada = SolicitudReserva.Crear(Id(1), Id(1), Solicitante, "Visita a cliente");
        var otra = SolicitudReserva.Crear(Id(1), Id(1), OtroSolicitante, "Visita a cliente");

        aprobada.Aprobar(Confirmador, Ahora);
        CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Asignado", fechaCambio: Ahora);
        otra.Rechazar(Confirmador, Ahora, "Rechazada automáticamente: el carro fue asignado a otra solicitud.");

        Assert.True(aprobada.EstaAprobada);
        Assert.Equal(EstadoSolicitudReserva.Rechazada, otra.Estado);
        Assert.Throws<DomainException>(() => otra.Aprobar(Confirmador, Ahora));
    }

    [Fact]
    public void Aprobar_CarroYaOcupadoPorOtraReserva_NoSePuedeVolverAOcupar()
    {
        var carro = ConstruirCarro();
        CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Primera reserva");
        var segunda = SolicitudReserva.Crear(Id(1), Id(1), OtroSolicitante, "Visita a cliente");

        segunda.Aprobar(Confirmador, Ahora);

        // El handler hace todo antes de SaveChanges: si ocupar falla, no se guarda la aprobación
        Assert.Throws<DomainException>(() => CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Segunda reserva"));
    }

    [Fact]
    public void Aprobar_CarroEnMantenimiento_NoSeOcupaYConservaSuEstado()
    {
        var carro = ConstruirCarro(EstadoCarro.EnMantenimiento);
        var solicitud = SolicitudReserva.Crear(Id(1), Id(1), Solicitante, "Visita a cliente");

        solicitud.Aprobar(Confirmador, Ahora);

        Assert.Throws<DomainException>(() => CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Asignado"));
        Assert.Equal(EstadoCarro.EnMantenimiento, carro.Estado);
    }

    // --- Cancelar libera el carro solo si la reserva estaba aprobada ---

    [Fact]
    public void Cancelar_ReservaAprobada_LiberaElCarro()
    {
        var carro = ConstruirCarro();
        var solicitud = SolicitudReserva.Crear(Id(1), Id(1), Solicitante, "Visita a cliente");
        solicitud.Aprobar(Confirmador, Ahora);
        CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Asignado", fechaCambio: Ahora);

        var estabaAprobada = solicitud.EstaAprobada;
        solicitud.Cancelar(Ahora.AddHours(1), "Ya no se necesita");
        var cambio = CambioEstatusCarro.Registrar(carro, Solicitante, EstadoCarro.Disponible, "Liberado por cancelación", fechaCambio: Ahora.AddHours(1));

        Assert.True(estabaAprobada);
        Assert.Equal(EstadoSolicitudReserva.Cancelada, solicitud.Estado);
        Assert.Equal(EstadoCarro.Disponible, carro.Estado);
        Assert.Equal(EstadoCarro.Ocupado, cambio.EstatusAnterior);
    }

    [Fact]
    public void Cancelar_ReservaPendiente_ElCarroNuncaSeOcupo()
    {
        var carro = ConstruirCarro();
        var solicitud = SolicitudReserva.Crear(Id(1), Id(1), Solicitante, "Visita a cliente");

        var estabaAprobada = solicitud.EstaAprobada;
        solicitud.Cancelar(Ahora);

        Assert.False(estabaAprobada);
        Assert.Equal(EstadoCarro.Disponible, carro.Estado);
    }

    // --- Después de liberar, el carro se puede reservar otra vez ---

    [Fact]
    public void CancelarYVolverAReservar_ElCarroSePuedeAsignarANuevaSolicitud()
    {
        var carro = ConstruirCarro();
        var primera = SolicitudReserva.Crear(Id(1), Id(1), Solicitante, "Visita a cliente");
        primera.Aprobar(Confirmador, Ahora);
        CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Asignado");
        primera.Cancelar(Ahora.AddHours(1));
        CambioEstatusCarro.Registrar(carro, Solicitante, EstadoCarro.Disponible, "Liberado");

        var segunda = SolicitudReserva.Crear(Id(1), Id(1), OtroSolicitante, "Visita a cliente");
        segunda.Aprobar(Confirmador, Ahora.AddHours(2));
        CambioEstatusCarro.Registrar(carro, Confirmador, EstadoCarro.Ocupado, "Asignado");

        Assert.True(segunda.EstaAprobada);
        Assert.Equal(EstadoCarro.Ocupado, carro.Estado);
    }

    private static Carro ConstruirCarro(EstadoCarro estado = EstadoCarro.Disponible)
        => Carro.Crear(Id(1), "Nissan", "NP300", 2023, "Blanco", "DFG-123-A", null, 80m, 15000m, 2026, estado);
}
