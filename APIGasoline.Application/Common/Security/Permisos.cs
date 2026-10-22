using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Common.Security;

/// <summary>Un privilegio del catálogo base (lo que la siembra crea si no existe).</summary>
public sealed record DefinicionPrivilegio(string Clave, string Nombre, string Descripcion, AmbitoPrivilegio Ambito);

/// <summary>
/// Catálogo de privilegios (columna privilegio.clave, formato modulo.accion).
///
/// AUTORIZACIÓN BASADA EN PERMISOS: la API decide SOLO con estas claves, nunca con el nombre
/// del rol. Cada empresa puede llamar a sus roles como quiera ("Confirmador", "Jefe de flota",
/// "Supervisor de ruta"...): lo que cuenta es qué privilegios tiene asignados el rol.
///
/// Las mismas claves están en la app (core/rbac/Privileges.kt). Si agregas una aquí, agrégala allá.
/// </summary>
public static class Permisos
{
    // ---------------- Plataforma (solo roles globales de la desarrolladora) ----------------

    /// <summary>Consultar y operar datos de cualquier empresa (sin esto, solo la del token).</summary>
    public const string PlataformaMultiempresa = "plataforma.multiempresa";

    /// <summary>Crear, editar y eliminar corporativos y divisiones.</summary>
    public const string CorporativoGestionar = "corporativo.gestionar";

    /// <summary>Crear y eliminar empresas.</summary>
    public const string EmpresaGestionar = "empresa.gestionar";

    /// <summary>Crear, editar y eliminar privilegios del catálogo.</summary>
    public const string PrivilegioGestionar = "privilegio.gestionar";

    /// <summary>Ver y reintentar los mensajes del outbox (push).</summary>
    public const string OutboxGestionar = "outbox.gestionar";

    // ---------------- Empresa ----------------

    public const string CorporativoVer = "corporativo.ver";
    public const string EmpresaVer = "empresa.ver";
    public const string EmpresaEditar = "empresa.editar";

    public const string CentroCostoVer = "centro_costo.ver";
    public const string CentroCostoGestionar = "centro_costo.gestionar";

    public const string UsuarioVer = "usuario.ver";
    public const string UsuarioGestionar = "usuario.gestionar";

    public const string EmpleadoVer = "empleado.ver";
    public const string EmpleadoGestionar = "empleado.gestionar";

    public const string RolVer = "rol.ver";

    /// <summary>Crear / editar / eliminar roles y decidir qué privilegios tiene cada uno.</summary>
    public const string RolGestionar = "rol.gestionar";

    /// <summary>Asignar y quitar roles a los usuarios.</summary>
    public const string RolAsignar = "rol.asignar";

    public const string PrivilegioVer = "privilegio.ver";

    /// <summary>Ver la lista de vehículos (GET /api/carros).</summary>
    public const string CarroVer = "carro.ver";

    /// <summary>Ver la información de un solo vehículo (GET /api/carros/{id}).</summary>
    public const string CarroVerDetalle = "carro.ver_detalle";
    public const string CarroCrear = "carro.crear";
    public const string CarroEditar = "carro.editar";
    public const string CarroEliminar = "carro.eliminar";

    /// <summary>Cambiar el estatus de un carro (disponible / mantenimiento / baja...).</summary>
    public const string CarroCambiarEstatus = "carro.cambiar_estatus";

    public const string SolicitudCrear = "solicitud.crear";

    /// <summary>Ver el status de sus propias solicitudes.</summary>
    public const string SolicitudVerPropias = "solicitud.ver_propias";

    /// <summary>Ver las solicitudes de toda la empresa.</summary>
    public const string SolicitudVerTodas = "solicitud.ver_todas";

    /// <summary>
    /// Aceptar solicitudes. Quien lo tenga recibe las notificaciones de nuevas solicitudes y de
    /// reportes a mantenimiento (esto es lo que antes hacía el rol "Confirmador").
    /// </summary>
    public const string SolicitudAprobar = "solicitud.aprobar";

    /// <summary>Rechazar solicitudes.</summary>
    public const string SolicitudRechazar = "solicitud.rechazar";

    public const string ViajeVerPropios = "viaje.ver_propios";
    public const string ViajeVerTodos = "viaje.ver_todos";

    /// <summary>Ver el detalle de un viaje: datos, pausas, mapa de la ruta y sus cargas.</summary>
    public const string ViajeVerDetalle = "viaje.ver_detalle";

    /// <summary>Iniciar, pausar, reanudar, sincronizar, mandar puntos GPS y finalizar viajes (pestaña Registrar).</summary>
    public const string ViajeRegistrar = "viaje.registrar";

    /// <summary>Corregir kilometraje / cargas de viajes ya cerrados (antes: Confirmador o Gestor de activos).</summary>
    public const string ViajeCorregirCerrado = "viaje.corregir_cerrado";

    public const string ViajeEliminar = "viaje.eliminar";

    /// <summary>Registrar una carga de gasolina.</summary>
    public const string CargaRegistrar = "carga.registrar";

    /// <summary>Editar una carga ya enviada (corrección con motivo; con el viaje cerrado además viaje.corregir_cerrado).</summary>
    public const string CargaEditar = "carga.editar";

    /// <summary>Eliminar una carga (con motivo; con el viaje cerrado además viaje.corregir_cerrado).</summary>
    public const string CargaEliminar = "carga.eliminar";

    /// <summary>Consultar el módulo de auditoría y trazabilidad.</summary>
    public const string AuditoriaVer = "auditoria.ver";

    /// <summary>Crear revisiones de auditoría y registrar su resultado.</summary>
    public const string AuditoriaRevisar = "auditoria.revisar";

    /// <summary>Enviar notificaciones manuales a otros empleados.</summary>
    public const string NotificacionEnviar = "notificacion.enviar";

    /// <summary>Todo el catálogo base. La siembra lo crea si falta (no modifica los que ya existen).</summary>
    public static readonly IReadOnlyList<DefinicionPrivilegio> Catalogo =
    [
        new(PlataformaMultiempresa, "Operar en todas las empresas", "Consultar y operar datos de cualquier empresa.", AmbitoPrivilegio.Plataforma),
        new(CorporativoGestionar, "Administrar corporativos y divisiones", "Crear, editar y eliminar corporativos y divisiones.", AmbitoPrivilegio.Plataforma),
        new(EmpresaGestionar, "Crear y eliminar empresas", "Dar de alta y eliminar empresas.", AmbitoPrivilegio.Plataforma),
        new(PrivilegioGestionar, "Administrar el catálogo de privilegios", "Crear, editar y eliminar privilegios.", AmbitoPrivilegio.Plataforma),
        new(OutboxGestionar, "Administrar el outbox de notificaciones", "Ver y reintentar mensajes push.", AmbitoPrivilegio.Plataforma),

        new(CorporativoVer, "Ver corporativos y divisiones", "Consultar corporativos y divisiones.", AmbitoPrivilegio.Empresa),
        new(EmpresaVer, "Ver empresas", "Consultar los datos de la empresa.", AmbitoPrivilegio.Empresa),
        new(EmpresaEditar, "Editar la empresa", "Editar los datos de la empresa.", AmbitoPrivilegio.Empresa),
        new(CentroCostoVer, "Ver centros de costo", "Consultar los centros de costo.", AmbitoPrivilegio.Empresa),
        new(CentroCostoGestionar, "Administrar centros de costo", "Crear, editar y eliminar centros de costo.", AmbitoPrivilegio.Empresa),
        new(UsuarioVer, "Ver usuarios", "Consultar los usuarios.", AmbitoPrivilegio.Empresa),
        new(UsuarioGestionar, "Administrar usuarios", "Crear, editar, eliminar usuarios y cambiar contraseñas.", AmbitoPrivilegio.Empresa),
        new(EmpleadoVer, "Ver empleados", "Consultar los empleados.", AmbitoPrivilegio.Empresa),
        new(EmpleadoGestionar, "Administrar empleados", "Crear, editar y eliminar empleados y sus fotos.", AmbitoPrivilegio.Empresa),
        new(RolVer, "Ver roles", "Consultar roles y sus privilegios.", AmbitoPrivilegio.Empresa),
        new(RolGestionar, "Administrar roles", "Crear, editar y eliminar roles y sus privilegios.", AmbitoPrivilegio.Empresa),
        new(RolAsignar, "Asignar roles", "Asignar y quitar roles a los usuarios.", AmbitoPrivilegio.Empresa),
        new(PrivilegioVer, "Ver privilegios", "Consultar el catálogo de privilegios.", AmbitoPrivilegio.Empresa),
        new(CarroVer, "Ver vehículos", "Ver vehículos", AmbitoPrivilegio.Empresa),
        new(CarroVerDetalle, "Ver detalles de un vehículo", "Ver la información de un solo vehículo (por ID).", AmbitoPrivilegio.Empresa),
        new(CarroCrear, "Agregar vehículos", "Agregar vehículos", AmbitoPrivilegio.Empresa),
        new(CarroEditar, "Editar vehículos", "Editar vehículos", AmbitoPrivilegio.Empresa),
        new(CarroEliminar, "Eliminar vehículos", "Eliminar vehículos", AmbitoPrivilegio.Empresa),
        new(CarroCambiarEstatus, "Cambiar estatus de vehículos", "Mandar a mantenimiento, liberar o dar de baja un vehículo.", AmbitoPrivilegio.Empresa),
        new(SolicitudCrear, "Solicitar un vehículo", "Solicitar un vehículo", AmbitoPrivilegio.Empresa),
        new(SolicitudVerPropias, "Ver mis solicitudes", "Ver el status de mis solicitudes.", AmbitoPrivilegio.Empresa),
        new(SolicitudVerTodas, "Ver las solicitudes de la empresa", "Ver todas las solicitudes de la empresa.", AmbitoPrivilegio.Empresa),
        new(SolicitudAprobar, "Aprobar solicitudes", "Aprobar solicitudes", AmbitoPrivilegio.Empresa),
        new(SolicitudRechazar, "Rechazar solicitudes", "Rechazar solicitudes.", AmbitoPrivilegio.Empresa),
        new(ViajeVerPropios, "Ver mis viajes", "Ver mis viajes", AmbitoPrivilegio.Empresa),
        new(ViajeVerTodos, "Ver los viajes de la empresa", "Ver los viajes de la empresa", AmbitoPrivilegio.Empresa),
        new(ViajeVerDetalle, "Ver detalles de un viaje", "Ver datos, pausas, mapa y cargas de un viaje.", AmbitoPrivilegio.Empresa),
        new(ViajeRegistrar, "Registrar viajes (pestaña Registrar)", "Registrar viajes (pestaña Registrar)", AmbitoPrivilegio.Empresa),
        new(ViajeCorregirCerrado, "Corregir viajes cerrados", "Corregir kilometraje y cargas de viajes ya cerrados.", AmbitoPrivilegio.Empresa),
        new(ViajeEliminar, "Eliminar viajes", "Eliminar viajes y puntos de ruta.", AmbitoPrivilegio.Empresa),
        new(CargaRegistrar, "Registrar recargas de gasolina", "Registrar recargas de gasolina", AmbitoPrivilegio.Empresa),
        new(CargaEditar, "Editar recargas de gasolina", "Corregir una carga ya enviada (con motivo).", AmbitoPrivilegio.Empresa),
        new(CargaEliminar, "Eliminar recargas de gasolina", "Eliminar una carga (con motivo).", AmbitoPrivilegio.Empresa),
        new(AuditoriaVer, "Ver auditoría", "Consultar el módulo de auditoría y trazabilidad.", AmbitoPrivilegio.Empresa),
        new(AuditoriaRevisar, "Revisar auditoría", "Crear revisiones de auditoría y registrar su resultado.", AmbitoPrivilegio.Empresa),
        new(NotificacionEnviar, "Enviar notificaciones", "Enviar notificaciones manuales.", AmbitoPrivilegio.Empresa),
    ];
}
