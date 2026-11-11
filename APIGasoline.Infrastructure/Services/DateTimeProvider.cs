using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    // Hora local del servidor, igual que SYSDATETIME() de SQL Server (los defaults de fecha de la BD)
    public DateTime Now => DateTime.Now;
}
