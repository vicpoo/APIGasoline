global using static APIGasoline.Application.Tests.IdsPrueba;

namespace APIGasoline.Application.Tests;

internal static class IdsPrueba
{
    public static Guid Id(int n)
        => n == 0 ? Guid.Empty : new Guid($"00000000-0000-7000-8000-{n:D12}");

    public static Guid? Id(int? n) => n is null ? null : Id(n.Value);
}
