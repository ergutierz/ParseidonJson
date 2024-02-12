using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParseidonJson.Model;
using ParseidonJson.utilities;

namespace ParseidonJson.ViewModel
{
    class SettingVM : ViewModelBase
    {
        private readonly PageModel _pageModel;
        public bool Settings
        {
            get { return _pageModel.LocationStatus; }
            set { _pageModel.LocationStatus = value; OnPropertyChanged(); }
        }

        public SettingVM()
        {
            _pageModel = new PageModel();
            Settings = true;
        }
    }
}
