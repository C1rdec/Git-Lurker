namespace GitLurker.UI.ViewModels;

using Caliburn.Micro;

internal class WelcomeViewModel : Screen
{
    public async void Start()
    {
        var shellViewModel = IoC.Get<ShellViewModel>();
        await IoC.Get<IWindowManager>().ShowWindowAsync(shellViewModel);
    }
}
