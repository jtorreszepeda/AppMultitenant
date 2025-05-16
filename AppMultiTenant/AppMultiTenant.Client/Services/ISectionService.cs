using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Servicio para obtener las secciones disponibles para el usuario actual
    /// </summary>
    public interface ISectionService
    {
        /// <summary>
        /// Obtiene las secciones disponibles para el usuario actual
        /// </summary>
        /// <returns>Lista de secciones disponibles</returns>
        Task<List<SectionInfo>> GetAvailableSectionsAsync();
    }

    /// <summary>
    /// Información básica de una sección
    /// </summary>
    public class SectionInfo
    {
        /// <summary>
        /// Identificador único de la sección
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre para mostrar de la sección
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ícono para la sección (opcional, se puede usar un valor por defecto)
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Indica si el usuario puede crear nuevos registros en esta sección
        /// </summary>
        public bool CanCreate { get; set; }

        /// <summary>
        /// Indica si el usuario puede editar registros en esta sección
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Indica si el usuario puede eliminar registros en esta sección
        /// </summary>
        public bool CanDelete { get; set; }
    }
} 