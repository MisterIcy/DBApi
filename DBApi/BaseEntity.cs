using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBApi.Entities
{
    /// <summary>
    /// Κλάση η οποία κληρονομείται από τα entities, έτσι ώστε να υπάρχει ένα 
    /// κοινό API, όσον αφορά το development των παρεκλομένων υπηρεσιών
    /// </summary>
    /// <remarks></remarks>
    public abstract class BaseEntity : INotifyPropertyChanged
    {
        #region Events
        /// <summary>
        /// Event το οποίο πυροδοτείται όταν αλλάζει ένα property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public bool Loaded { get; set; } = false;
        public bool Incomplete { get; set; } = false;
    }
}
