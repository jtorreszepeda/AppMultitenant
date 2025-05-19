using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AppMultiTenant.Client.Services;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la creación de una nueva definición de sección
    /// </summary>
    public class CreateSectionDefinitionViewModel : INotifyPropertyChanged
    {
        private readonly ISectionApiClient _sectionApiClient;
        private bool _isLoading;
        private bool _isSaving;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private List<FieldDefinitionDto> _temporaryFields = new();
        private string _tempFieldName = string.Empty;
        private string _tempFieldDataType = "Text";
        private bool _tempFieldIsRequired;
        private int _tempFieldDisplayOrder;
        private string _tempFieldConfigJson = "{}";

        /// <summary>
        /// Constructor del ViewModel de creación de definición de sección
        /// </summary>
        /// <param name="sectionApiClient">Cliente API para operaciones con definiciones de secciones</param>
        public CreateSectionDefinitionViewModel(ISectionApiClient sectionApiClient)
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
        /// Indica si se está guardando datos
        /// </summary>
        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
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
        /// Mensaje de éxito tras una operación completada
        /// </summary>
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        /// <summary>
        /// Nombre de la definición de sección
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Descripción de la definición de sección
        /// </summary>
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Lista temporal de campos para la sección
        /// </summary>
        public List<FieldDefinitionDto> TemporaryFields
        {
            get => _temporaryFields;
            set => SetProperty(ref _temporaryFields, value);
        }

        /// <summary>
        /// Nombre del campo temporal que se está creando
        /// </summary>
        [Required(ErrorMessage = "El nombre del campo es obligatorio")]
        public string TempFieldName
        {
            get => _tempFieldName;
            set => SetProperty(ref _tempFieldName, value);
        }

        /// <summary>
        /// Tipo de datos del campo temporal
        /// </summary>
        [Required(ErrorMessage = "El tipo de datos es obligatorio")]
        public string TempFieldDataType
        {
            get => _tempFieldDataType;
            set => SetProperty(ref _tempFieldDataType, value);
        }

        /// <summary>
        /// Indica si el campo temporal es requerido
        /// </summary>
        public bool TempFieldIsRequired
        {
            get => _tempFieldIsRequired;
            set => SetProperty(ref _tempFieldIsRequired, value);
        }

        /// <summary>
        /// Orden de visualización del campo temporal
        /// </summary>
        public int TempFieldDisplayOrder
        {
            get => _tempFieldDisplayOrder;
            set => SetProperty(ref _tempFieldDisplayOrder, value);
        }

        /// <summary>
        /// Configuración adicional del campo temporal en formato JSON
        /// </summary>
        public string TempFieldConfigJson
        {
            get => _tempFieldConfigJson;
            set => SetProperty(ref _tempFieldConfigJson, value);
        }

        /// <summary>
        /// Lista de tipos de datos disponibles para los campos
        /// </summary>
        public List<string> AvailableDataTypes => new() 
        { 
            "Text", "TextArea", "Number", "Decimal", "Date", "DateTime", 
            "Boolean", "SingleSelect", "MultiSelect" 
        };

        /// <summary>
        /// Agrega un campo temporal a la lista de campos para la sección
        /// </summary>
        public void AddTemporaryField()
        {
            if (string.IsNullOrWhiteSpace(TempFieldName))
            {
                ErrorMessage = "El nombre del campo es obligatorio";
                return;
            }

            if (TemporaryFields.Exists(f => f.Name.Equals(TempFieldName, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Ya existe un campo con ese nombre";
                return;
            }

            var newField = new FieldDefinitionDto
            {
                Id = Guid.NewGuid().ToString(), // ID temporal
                Name = TempFieldName,
                DataType = TempFieldDataType,
                IsRequired = TempFieldIsRequired,
                DisplayOrder = TempFieldDisplayOrder,
                ConfigJson = TempFieldConfigJson
            };

            TemporaryFields.Add(newField);
            
            // Ordenar los campos por DisplayOrder
            TemporaryFields.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
            
            // Limpiar el formulario de campos temporales
            TempFieldName = string.Empty;
            TempFieldDataType = "Text";
            TempFieldIsRequired = false;
            TempFieldDisplayOrder = TemporaryFields.Count;
            TempFieldConfigJson = "{}";
            
            ErrorMessage = string.Empty;
            SuccessMessage = "Campo agregado correctamente";
        }

        /// <summary>
        /// Elimina un campo temporal de la lista
        /// </summary>
        /// <param name="fieldId">ID del campo temporal a eliminar</param>
        public void RemoveTemporaryField(string fieldId)
        {
            var field = TemporaryFields.Find(f => f.Id == fieldId);
            if (field != null)
            {
                TemporaryFields.Remove(field);
                SuccessMessage = "Campo eliminado correctamente";
            }
        }

        /// <summary>
        /// Crea una nueva definición de sección con los campos temporales
        /// </summary>
        public async Task CreateSectionDefinitionAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "El nombre de la sección es obligatorio";
                return;
            }

            try
            {
                IsSaving = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                // Primero crear la sección sin campos
                var createRequest = new CreateSectionDefinitionRequest
                {
                    Name = Name,
                    Description = Description
                };

                var createdSection = await _sectionApiClient.CreateSectionDefinitionAsync(createRequest);

                // Luego crear cada campo para la sección
                foreach (var field in TemporaryFields)
                {
                    var fieldRequest = new CreateFieldDefinitionRequest
                    {
                        Name = field.Name,
                        DataType = field.DataType,
                        IsRequired = field.IsRequired,
                        DisplayOrder = field.DisplayOrder,
                        ConfigJson = field.ConfigJson
                    };

                    await _sectionApiClient.CreateFieldDefinitionAsync(createdSection.Id, fieldRequest);
                }

                // Limpiar el formulario
                Name = string.Empty;
                Description = string.Empty;
                TemporaryFields.Clear();
                
                SuccessMessage = "Definición de sección creada correctamente";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear la definición de sección: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Reinicia el formulario de creación
        /// </summary>
        public void ResetForm()
        {
            Name = string.Empty;
            Description = string.Empty;
            TemporaryFields.Clear();
            TempFieldName = string.Empty;
            TempFieldDataType = "Text";
            TempFieldIsRequired = false;
            TempFieldDisplayOrder = 0;
            TempFieldConfigJson = "{}";
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
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