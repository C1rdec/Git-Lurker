﻿namespace GitLurker.UI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Caliburn.Micro;
    using GitLurker.Core.Models;
    using GitLurker.Core.Services;
    using GitLurker.UI.Extensions;
    using GitLurker.UI.Services;
    using GitLurker.UI.ViewModels;
    using Lurker.Windows;

    public class AppBootstrapper : BootstrapperBase
    {
        #region Fields

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private SimpleContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AppBootstrapper"/> class.
        /// </summary>
        public AppBootstrapper()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected async override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            if (RunningInstance() != null)
            {
                System.Windows.MessageBox.Show("Another instance is running");
                System.Windows.Application.Current.Shutdown();

                return;
            }

            await IoC.Get<PatronService>().CheckPledgeStatusAsync();

            await DisplayRootViewForAsync<ShellViewModel>();
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected override void Configure()
        {
            _container = new SimpleContainer();

            // Services
            _container.Singleton<PatronService, PatronService>();
            _container.Singleton<PopupService, PopupService>();
            _container.Singleton<ConsoleService, ConsoleService>();
            _container.Singleton<RepositoryService, RepositoryService>();
            _container.Singleton<ThemeService, ThemeService>();
            _container.Singleton<FlyoutService, FlyoutService>();
            _container.Singleton<DialogService, DialogService>();
            _container.Singleton<KeyboardService, KeyboardService>();
            _container.Singleton<GithubUpdateManager, GithubUpdateManager>();
            _container.Singleton<IDebounceService, DebounceService>();
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();

            // ViewModels
            _container.PerRequest<ShellViewModel, ShellViewModel>();
            _container.Singleton<PatreonSettingsViewModel, PatreonSettingsViewModel>();
            _container.Singleton<SettingsViewModel, SettingsViewModel>();
            _container.Singleton<PatreonViewModel, PatreonViewModel>();
            _container.Singleton<WelcomeViewModel, WelcomeViewModel>();
            _container.Singleton<ConsoleViewModel, ConsoleViewModel>();

            var settings = new SettingsFile();
            settings.Initialize();
            _container.Instance(settings);

            var customActions = new CustomActionSettingsFile();
            customActions.Initialize();
            _container.Instance(customActions);

            var startupService = new WindowsLink("GitLurker.lnk", "GitLurker");
            _container.Instance(startupService);
            _container.Instance(Application);
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        /// The located service.
        /// </returns>
        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <returns>
        /// The located services.
        /// </returns>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation.
        /// </summary>
        /// <param name="instance">The instance to perform injection on.</param>
        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        public static Process RunningInstance()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);

            var currentFilePath = currentProcess.GetMainModuleFileName();
            foreach (var process in processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    if (process.GetMainModuleFileName() == currentFilePath)
                    {
                        return process;
                    }
                }
            }

            return null;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Logger.Error(exception, exception.Message);
        }

        #endregion
    }
}
