using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Telerik_UI
{
    class ClubViewModel: INotifyPropertyChanged
    {
        ClubItemViewModel _SelectedClubItem;
        ObservableCollection<ClubItemViewModel> _ListClubItems;

        public ClubItemViewModel SelectedClubItem
        {
            get { return this._SelectedClubItem; }
            set
            {
                if (value != _SelectedClubItem)
                {
                    _SelectedClubItem = value;
                    RaisePropertyChange("SelectedClubItem");
                }
            }
        }

        public ObservableCollection<ClubItemViewModel> ListClubItems
        {
            get { return this._ListClubItems; }
            set
            {
                if (value != _ListClubItems)
                {
                    _ListClubItems = value;
                    RaisePropertyChange("ListClubItems");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public void RaisePropertyChange(string propertyName)
        {
            if(PropertyChanged!=null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand AddField { get; set; }
    }
}
