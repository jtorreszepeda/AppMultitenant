using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppMultiTenant.Client.Services;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de listado de definiciones de secciones
    /// </summary>
    public class SectionDefinitionListViewModel : INotifyPropertyChanged
    {
        private readonly ISectionApiClient _sectionApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private List<SectionDefinitionDto> _sectionDefinitions = new();
        private string _searchTerm = string.Empty;
        private SectionDefinitionDto _selectedSectionDefinition;

        /// <summary>
        /// Constructor del ViewModel de listado de definiciones de secciones
        /// </summary>
        /// <param name="sectionApiClient">Cliente API para operaciones con definiciones de secciones</param>
        public SectionDefinitionListViewModel(ISectionApiClient sectionApiClient)
        {
            _sectionApiClient = sectionApiClient ?? throw new ArgumentNullException(nameof(sectionApiClient));
        }

        /// <summary>
        /// Indica si se está cargando datos
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Mensaje de error si ocurre algún problema
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Lista de definiciones de secciones actuales
        /// </summary>
        public List<SectionDefinitionDto> SectionDefinitions
        {
            get => _sectionDefinitions;
            set => SetProperty(ref _sectionDefinitions, value);
        }

        /// <summary>
        /// Término de búsqueda para filtrar definiciones de secciones
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        /// <summary>
        /// Definición de sección seleccionada actualmente
        /// </summary>
        public SectionDefinitionDto SelectedSectionDefinition
        {
            get => _selectedSectionDefinition;
            set => SetProperty(ref _selectedSectionDefinition, value);
        }

        /// <summary>
        /// Carga la lista de definiciones de secciones desde la API
        /// </summary>
        public async Task LoadSectionDefinitionsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var sections = await _sectionApiClient.GetAllSectionDefinitionsAsync();
                
                SectionDefinitions = new List<SectionDefinitionDto>(sections);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar definiciones de secciones: {ex.Message}";
                SectionDefinitions = new List<SectionDefinitionDto>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Filtra la lista de definiciones de secciones según el término de búsqueda
        /// </summary>
        public async Task SearchSectionDefinitionsAsync()
        {
            await LoadSectionDefinitionsAsync();
            
            if (string.IsNullOrWhiteSpace(SearchTerm))
                return;

            var filtered = new List<SectionDefinitionDto>();
            var searchTermLower = SearchTerm.ToLower();
            
            foreach (var section in SectionDefinitions)
            {
                if (section.Name.ToLower().Contains(searchTermLower) || 
                    section.Description.ToLower().Contains(searchTermLower))
                {
                    filtered.Add(section);
                }
            }
            
            SectionDefinitions = filtered;
        }

        /// <summary>
        /// Elimina una definición de sección
        /// </summary>
        /// <param name="sectionId">ID de la definición de sección a eliminar</param>
        public async Task DeleteSectionDefinitionAsync(string sectionId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _sectionApiClient.DeleteSectionDefinitionAsync(sectionId);
                await LoadSectionDefinitionsAsync(); // Recargar la lista tras la eliminación
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar definición de sección: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 