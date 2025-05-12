namespace AppMultiTenant.Server.Configuration;

/// <summary>
/// Configuración relacionada con la estrategia multi-inquilino
/// </summary>
public class TenantConfiguration
{
    /// <summary>
    /// Método para identificar el inquilino: "Subdomain", "Path", "Header"
    /// </summary>
    public string TenantResolutionStrategy { get; set; } = "Subdomain";
    
    /// <summary>
    /// ID del inquilino predeterminado (cuando no se puede resolver)
    /// </summary>
    public string DefaultTenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Habilita características específicas de desarrollo para inquilinos
    /// </summary>
    public bool DevModeEnabled { get; set; } = false;
} 