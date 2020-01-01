using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telerik_UI
{
    public class ClubItemViewModel: INotifyPropertyChanged
    {
        public ClubItemViewModel(string name, DateTime established, int stadiumCapacity)
        {
            this.Name = name;
            this.Established = established;
            this.StadiumCapacity = stadiumCapacity;
        }

        string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChange("Name");
                }
            }
        }

        DateTime? _Established;
        public DateTime? Established
        {
            get { return this._Established; }
            set
            {
                if (value != _Established)
                {
                    _Established = value;
                    RaisePropertyChange("Established");
                }
            }
        }

        int _StadiumCapacity;
        public int StadiumCapacity
        {
            get { return this._StadiumCapacity; }
            set
            {
                if (value != _StadiumCapacity)
                {
                    _StadiumCapacity = value;
                    RaisePropertyChange("StadiumCapacity");
                }
            }
        }

        ObservableCollection<CustomData> _ListCustomData = new ObservableCollection<CustomData>();
        public ObservableCollection<CustomData> ListCustomData
        {
            get { return this._ListCustomData; }
            set
            {
                if (value != _ListCustomData)
                {
                    _ListCustomData = value;
                    RaisePropertyChange("ListCustomData");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ObservableCollection<ClubItemViewModel> GetClubs()
        {
            ObservableCollection<ClubItemViewModel> clubs = new ObservableCollection<ClubItemViewModel>();
            clubs.Add(new ClubItemViewModel("Liverpool", new DateTime(1892, 1, 1), 45362));
            clubs.Add(new ClubItemViewModel("Manchester Utd.", new DateTime(1878, 1, 1), 76212));
            clubs.Add(new ClubItemViewModel("Chelsea", new DateTime(1905, 1, 1), 42055));
            clubs.Add(new ClubItemViewModel("Arsenal", new DateTime(1886, 1, 1), 60355));
            return clubs;
        }
    }

    public class CustomData
    {
        public string Name { get; set; }

        public DateTime DateCreate { get; set; }
    }
}
