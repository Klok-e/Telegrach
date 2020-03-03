using System;
using System.Collections.Generic;
using System.Text;
using DesktopFrontend.Models;

namespace DesktopFrontend.ViewModels
{
    public class LoginWindowViewModel : ViewModelBase
    {
        

        public void GetConnection()
        {
            Connection.Connect(null, 0);
        }
    }
}
