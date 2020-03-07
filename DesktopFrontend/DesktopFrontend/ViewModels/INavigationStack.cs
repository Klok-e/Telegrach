namespace DesktopFrontend.ViewModels
{
    public interface INavigationStack
    {
        ViewModelBase Pop();
        void Push(ViewModelBase vm);
        ViewModelBase ReplaceTop(ViewModelBase vm);
    }
}