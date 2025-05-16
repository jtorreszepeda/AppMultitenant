using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// Clase base para todos los ViewModels que proporciona implementación de INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Evento que se dispara cuando una propiedad cambia su valor
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Método para notificar que una propiedad ha cambiado su valor
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que ha cambiado</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Establece el valor de una propiedad y notifica si ha cambiado
        /// </summary>
        /// <typeparam name="T">Tipo de la propiedad</typeparam>
        /// <param name="storage">Referencia al campo de respaldo</param>
        /// <param name="value">Nuevo valor</param>
        /// <param name="propertyName">Nombre de la propiedad</param>
        /// <returns>True si el valor cambió, False si es el mismo valor</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 