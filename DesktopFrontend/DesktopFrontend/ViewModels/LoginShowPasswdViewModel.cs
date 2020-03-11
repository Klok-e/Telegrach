using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class LoginShowPasswdViewModel : ViewModelBase
    {
        private string _password = "";

        public string Password
        {
            get => _password;
            private set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string _login = "";

        public string Login
        {
            get => _login;
            private set => this.RaiseAndSetIfChanged(ref _login, value);
        }

        public LoginShowPasswdViewModel(string login, string passwd)
        {
            Password = passwd;
            Login = login;
        }
    }
}