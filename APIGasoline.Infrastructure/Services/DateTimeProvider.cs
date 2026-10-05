using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    // Hora local del servidor, igual que CURRENT_TIMESTAMP de MySQL
    public DateTime Now => DateTime.Now;
}
