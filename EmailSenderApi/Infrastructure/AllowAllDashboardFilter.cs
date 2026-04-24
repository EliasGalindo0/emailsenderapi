using Hangfire.Dashboard;

namespace EmailSenderApi.Infrastructure;

/// <summary>
/// Libera acesso ao dashboard do Hangfire sem autenticação.
/// Use apenas em desenvolvimento. Em produção, implemente autenticação real.
/// </summary>
public class AllowAllDashboardFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
