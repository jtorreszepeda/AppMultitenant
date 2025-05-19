using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Linq;
using AppMultiTenant.Client.Services;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la edición de una definición de sección existente
    /// </summary>
    public class EditSectionDefinitionViewModel : INotifyPropertyChanged
    {
        private readonly ISectionApiClient _sectionApiClient;
        private bool _isLoading;
        private bool _isSaving;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private string _sectionId = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private List<FieldDefinitionDto> _fields = new();
        private List<FieldDefinitionDto> _deletedFields = new();
        private List<FieldDefinitionDto> _newFields = new();
        private FieldDefinitionDto _selectedField;
        private string _tempFieldName = string.Empty;
        private string _tempFieldDataType = "Text";
        private bool _tempFieldIsRequired;
        private int _tempFieldDisplayOrder;
        private string _tempFieldConfigJson = "{}";
        private bool _isAddingNewField;
        private bool _isEditingField;

        /// <summary>
        /// Constructor del ViewModel de edición de definición de sección
        /// </summary>
        /// <param name="sectionApiClient">Cliente API para operaciones con definiciones de secciones</param>
        public EditSectionDefinitionViewModel(ISectionApiClient sectionApiClient)
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
        /// ID de la definición de sección a editar
        /// </summary>
        public string SectionId
        {
            get => _sectionId;
            set => SetProperty(ref _sectionId, value);
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
        /// Lista de campos de la sección
        /// </summary>
        public List<FieldDefinitionDto> Fields
        {
            get => _fields;
            set => SetProperty(ref _fields, value);
        }

        /// <summary>
        /// Lista de campos eliminados (para seguimiento)
        /// </summary>
        public List<FieldDefinitionDto> DeletedFields
        {
            get => _deletedFields;
            set => SetProperty(ref _deletedFields, value);
        }

        /// <summary>
        /// Lista de campos nuevos (para seguimiento)
        /// </summary>
        public List<FieldDefinitionDto> NewFields
        {
            get => _newFields;
            set => SetProperty(ref _newFields, value);
        }

        /// <summary>
        /// Campo seleccionado actualmente para edición
        /// </summary>
        public FieldDefinitionDto SelectedField
        {
            get => _selectedField;
            set
            {
                if (SetProperty(ref _selectedField, value) && value != null)
                {
                    // Copiar propiedades al formulario temporal
                    TempFieldName = value.Name;
                    TempFieldDataType = value.DataType;
                    TempFieldIsRequired = value.IsRequired;
                    TempFieldDisplayOrder = value.DisplayOrder;
                    TempFieldConfigJson = value.ConfigJson;
                }
            }
        }

        /// <summary>
        /// Nombre del campo temporal que se está creando/editando
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
        /// Indica si se está agregando un nuevo campo
        /// </summary>
        public bool IsAddingNewField
        {
            get => _isAddingNewField;
            set => SetProperty(ref _isAddingNewField, value);
        }

        /// <summary>
        /// Indica si se está editando un campo existente
        /// </summary>
        public bool IsEditingField
        {
            get => _isEditingField;
            set => SetProperty(ref _isEditingField, value);
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
        /// Carga los datos de la definición de sección y sus campos
        /// </summary>
        /// <param name="sectionId">ID de la sección a cargar</param>
        public async Task LoadSectionDefinitionAsync(string sectionId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SectionId = sectionId;

                // Cargar la definición de sección
                var section = await _sectionApiClient.GetSectionDefinitionByIdAsync(sectionId);
                
                Name = section.Name;
                Description = section.Description;
                
                // Cargar los campos de la sección
                var fields = await _sectionApiClient.GetFieldDefinitionsBySectionIdAsync(sectionId);
                Fields = new List<FieldDefinitionDto>(fields);
                
                // Ordenar los campos por DisplayOrder
                Fields.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
                
                // Limpiar las listas de seguimiento
                DeletedFields.Clear();
                NewFields.Clear();
                
                // Limpiar el formulario temporal
                ResetFieldForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar la definición de sección: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Guarda los cambios en la definición de sección y sus campos
        /// </summary>
        public async Task SaveChangesAsync()
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

                // Actualizar la definición de sección
                var updateRequest = new UpdateSectionDefinitionRequest
                {
                    Name = Name,
                    Description = Description
                };

                await _sectionApiClient.UpdateSectionDefinitionAsync(SectionId, updateRequest);

                // Eliminar campos marcados para eliminación
                foreach (var field in DeletedFields)
                {
                    if (!string.IsNullOrEmpty(field.Id) && !field.Id.StartsWith("new_"))
                    {
                        await _sectionApiClient.DeleteFieldDefinitionAsync(field.Id);
                    }
                }

                // Actualizar campos existentes
                foreach (var field in Fields.Where(f => !NewFields.Contains(f)))
                {
                    var updateFieldRequest = new UpdateFieldDefinitionRequest
                    {
                        Name = field.Name,
                        DataType = field.DataType,
                        IsRequired = field.IsRequired,
                        DisplayOrder = field.DisplayOrder,
                        ConfigJson = field.ConfigJson
                    };

                    await _sectionApiClient.UpdateFieldDefinitionAsync(field.Id, updateFieldRequest);
                }

                // Crear nuevos campos
                foreach (var field in NewFields)
                {
                    var createFieldRequest = new CreateFieldDefinitionRequest
                    {
                        Name = field.Name,
                        DataType = field.DataType,
                        IsRequired = field.IsRequired,
                        DisplayOrder = field.DisplayOrder,
                        ConfigJson = field.ConfigJson
                    };

                    await _sectionApiClient.CreateFieldDefinitionAsync(SectionId, createFieldRequest);
                }

                // Recargar la sección para obtener los datos actualizados
                await LoadSectionDefinitionAsync(SectionId);
                
                SuccessMessage = "Definición de sección actualizada correctamente";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar los cambios: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Inicia la edición de un campo existente
        /// </summary>
        /// <param name="field">Campo a editar</param>
        public void StartEditField(FieldDefinitionDto field)
        {
            SelectedField = field;
            IsEditingField = true;
            IsAddingNewField = false;
        }

        /// <summary>
        /// Inicia la creación de un nuevo campo
        /// </summary>
        public void StartAddNewField()
        {
            ResetFieldForm();
            TempFieldDisplayOrder = Fields.Count; // Posicionar al final por defecto
            IsAddingNewField = true;
            IsEditingField = false;
        }

        /// <summary>
        /// Guarda los cambios en el campo que se está editando
        /// </summary>
        public void SaveFieldChanges()
        {
            if (string.IsNullOrWhiteSpace(TempFieldName))
            {
                ErrorMessage = "El nombre del campo es obligatorio";
                return;
            }

            // Verificar nombre duplicado (excepto el campo actual)
            if ((IsAddingNewField || (IsEditingField && SelectedField.Name != TempFieldName)) && 
                Fields.Any(f => f.Name.Equals(TempFieldName, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Ya existe un campo con ese nombre";
                return;
            }

            if (IsAddingNewField)
            {
                // Crear nuevo campo
                var newField = new FieldDefinitionDto
                {
                    Id = $"new_{Guid.NewGuid()}",  // ID temporal que indica que es nuevo
                    Name = TempFieldName,
                    DataType = TempFieldDataType,
                    IsRequired = TempFieldIsRequired,
                    DisplayOrder = TempFieldDisplayOrder,
                    ConfigJson = TempFieldConfigJson
                };

                Fields.Add(newField);
                NewFields.Add(newField);
                
                // Ordenar los campos por DisplayOrder
                Fields.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
                
                SuccessMessage = "Campo agregado correctamente";
            }
            else if (IsEditingField && SelectedField != null)
            {
                // Actualizar campo existente
                SelectedField.Name = TempFieldName;
                SelectedField.DataType = TempFieldDataType;
                SelectedField.IsRequired = TempFieldIsRequired;
                SelectedField.DisplayOrder = TempFieldDisplayOrder;
                SelectedField.ConfigJson = TempFieldConfigJson;
                
                // Ordenar los campos por DisplayOrder
                Fields.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
                
                SuccessMessage = "Campo actualizado correctamente";
            }

            // Salir del modo edición/creación
            IsAddingNewField = false;
            IsEditingField = false;
            ResetFieldForm();
        }

        /// <summary>
        /// Cancela la edición actual de un campo
        /// </summary>
        public void CancelFieldEdit()
        {
            IsAddingNewField = false;
            IsEditingField = false;
            ResetFieldForm();
        }

        /// <summary>
        /// Marca un campo para eliminación
        /// </summary>
        /// <param name="field">Campo a eliminar</param>
        public void DeleteField(FieldDefinitionDto field)
        {
            if (NewFields.Contains(field))
            {
                // Si es un campo nuevo que aún no se ha guardado, simplemente removerlo
                NewFields.Remove(field);
            }
            else
            {
                // Si es un campo existente, agregarlo a la lista de eliminados
                DeletedFields.Add(field);
            }
            
            Fields.Remove(field);
            SuccessMessage = "Campo marcado para eliminación";
        }

        /// <summary>
        /// Restaura un campo que estaba marcado para eliminación
        /// </summary>
        /// <param name="field">Campo a restaurar</param>
        public void RestoreDeletedField(FieldDefinitionDto field)
        {
            if (DeletedFields.Contains(field))
            {
                DeletedFields.Remove(field);
                Fields.Add(field);
                
                // Ordenar los campos por DisplayOrder
                Fields.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
                
                SuccessMessage = "Campo restaurado";
            }
        }

        /// <summary>
        /// Reinicia el formulario de edición de campo
        /// </summary>
        private void ResetFieldForm()
        {
            TempFieldName = string.Empty;
            TempFieldDataType = "Text";
            TempFieldIsRequired = false;
            TempFieldDisplayOrder = 0;
            TempFieldConfigJson = "{}";
            SelectedField = null;
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