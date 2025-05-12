namespace AppMultiTenant.Application.Configuration;

/// <summary>
/// Configuración para la autenticación basada en JWT
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// El emisor del token JWT (normalmente el nombre de la aplicación)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// La audiencia del token JWT (normalmente el cliente de la aplicación)
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Clave secreta para firmar tokens JWT (debe almacenarse de forma segura)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Tiempo de caducidad del token JWT en minutos
    /// </summary>
    public int ExpiryInMinutes { get; set; } = 60;
    
    /// <summary>
    /// Tiempo de caducidad del token de actualización en días
    /// </summary>
    public int RefreshTokenExpiryInDays { get; set; } = 7;
} 