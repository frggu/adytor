using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Adytor
{
    public class AdytorCurve : INotifyPropertyChanged
    {
        private ObservableCollection<FreqAmpPair> _curveCollection;
        private string _name;

        public AdytorCurve(ObservableCollection<FreqAmpPair> collection, string name)
        {
            CurveCollection = collection;
            Name = name;
        }

        public string Name {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public ObservableCollection<FreqAmpPair> CurveCollection {
            get
            {
                return _curveCollection;
            }
            set
            {
                _curveCollection = value;
                RaisePropertyChanged("CurveCollection");
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region methods
        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}