using System.ComponentModel;

namespace SSMSTools_21.Models
{
    public class CheckboxItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isVisible;

        public string Name { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CheckboxItem Clone()
        {
            return new CheckboxItem
            {
                IsSelected = _isSelected,
                IsVisible = _isVisible,
                Name = Name
            };
        }
    }
}