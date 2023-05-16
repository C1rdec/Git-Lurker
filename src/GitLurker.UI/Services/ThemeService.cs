using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GitLurker.Core.Models;

namespace GitLurker.UI.Services
{
    public class ThemeService
    {
        #region Fields

        private Application _application;
        private SettingsFile _settingsService;

        #endregion

        #region Constructors

        public ThemeService(Application application, SettingsFile settingsFile)
        {
            _settingsService = settingsFile;
            _application = application;
        }

        #endregion

        #region Properties

        public Scheme Scheme => _settingsService.Entity.Scheme;

        #endregion

        #region Methods

        public void Apply()
            => Apply(_settingsService.Entity.Scheme);

        public void Apply(Scheme scheme)
        {
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(_application, $"{Theme.Dark}.{scheme}");
        }

        public void Change(Theme theme, Scheme scheme)
        {
            _settingsService.Entity.Scheme = scheme;
            _settingsService.Save();
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(_application, $"{theme}.{scheme}");
        }

        public IEnumerable<Scheme> GetSchemes() => GetEnumValues<Scheme>();

        private static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray().OrderBy(t => t);
        }

        #endregion
    }

    public enum Theme
    {
        Light,
        Dark
    }
}
