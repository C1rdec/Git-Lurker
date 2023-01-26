using Caliburn.Micro;

namespace GitLurker.UI.ViewModels
{
    internal class WelcomeViewModel : Screen
    {
        public async void Start()
        {
            var shellViewModel = IoC.Get<ShellViewModel>();
            await IoC.Get<IWindowManager>().ShowWindowAsync(shellViewModel);
        }
    }
}
