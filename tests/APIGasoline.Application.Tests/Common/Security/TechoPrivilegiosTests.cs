using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Tests.Features.Auth;

namespace APIGasoline.Application.Tests.Common.Security;

/// <summary>Techo de privilegios: nadie da ni quita más de lo que tiene.</summary>
public class TechoPrivilegiosTests
{
    private static readonly Guid Usuario = Guid.Parse("01920000-0000-7000-8000-000000000001");
    private static readonly Guid RolChico = Guid.Parse("01920000-0000-7000-8000-0000000000a1");
    private static readonly Guid RolGrande = Guid.Parse("01920000-0000-7000-8000-0000000000a2");

    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public TechoPrivilegiosTests()
    {
        var (unitOfWork, estado) = UnidadFalsa.Crear();
        _unitOfWork = unitOfWork;
        estado.ClavesPorRol[RolChico] = [Permisos.CarroVer];
        estado.ClavesPorRol[RolGrande] = [Permisos.CarroVer, Permisos.ViajeEliminar];
    }

    private static UsuarioActualFalso Gestor(params string[] privilegios)
        => new(Usuario, null, [Permisos.RolGestionar, Permisos.RolAsignar, .. privilegios]);

    [Fact]
    public async Task DarUnPrivilegioQueTienes_Pasa()
        => await TechoPrivilegios.ExigirPrivilegioAsync(Gestor(Permisos.CarroVer), Permisos.CarroVer, CancellationToken.None);

    [Fact]
    public async Task DarUnPrivilegioQueNoTienes_Lanza403()
        => await Assert.ThrowsAsync<ForbiddenException>(() =>
            TechoPrivilegios.ExigirPrivilegioAsync(Gestor(Permisos.CarroVer), Permisos.ViajeEliminar, CancellationToken.None));

    [Fact]
    public async Task RolConPrivilegiosQueTienes_Pasa()
        => await TechoPrivilegios.ExigirRolAsync(_unitOfWork, Gestor(Permisos.CarroVer), RolChico, CancellationToken.None);

    [Fact]
    public async Task RolConUnPrivilegioQueNoTienes_Lanza403YDiceCual()
    {
        var error = await Assert.ThrowsAsync<ForbiddenException>(() =>
            TechoPrivilegios.ExigirRolAsync(_unitOfWork, Gestor(Permisos.CarroVer), RolGrande, CancellationToken.None));

        Assert.Contains(Permisos.ViajeEliminar, error.Message);
        Assert.DoesNotContain(Permisos.CarroVer, error.Message);
    }

    [Fact]
    public async Task RolSinPrivilegios_CualquierGestorPuede()
        => await TechoPrivilegios.ExigirRolAsync(_unitOfWork, Gestor(), Guid.CreateVersion7(), CancellationToken.None);
}
