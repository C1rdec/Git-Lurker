using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GitLurker.Models;

namespace GitLurker.UI.Services
{
    public class ThemeService
    {
        #region Fields

        private static Random _random = new();
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
        {
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(_application, $"{Theme.Dark}.{_settingsService.Entity.Scheme}");
        }

        public void Change(Theme theme, Scheme scheme)
        {
            _settingsService.Entity.Scheme = scheme;
            _settingsService.Save();
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(_application, $"{Theme.Dark}.{scheme}");
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
