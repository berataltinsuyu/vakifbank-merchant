using Npgsql;

namespace VbMerchant.Data;

public static class PostgresConnectionString
{
    public static string Normalize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection yapılandırması eksik.");
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return connectionString;
        }

        var userInfoParts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        if (userInfoParts.Length != 2)
        {
            throw new InvalidOperationException("PostgreSQL URI kullanıcı adı/şifre bilgisi eksik.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = string.IsNullOrWhiteSpace(uri.AbsolutePath.Trim('/'))
                ? "postgres"
                : uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfoParts[0]),
            Password = Uri.UnescapeDataString(userInfoParts[1]),
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }
}
