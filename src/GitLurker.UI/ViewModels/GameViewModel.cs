using System.IO;
using System.Windows.Media.Imaging;
using GitLurker.Models;
using Lurker.Common.Models;

namespace GitLurker.UI.ViewModels
{
    public class GameViewModel : ItemViewModelBase
    {
        #region Fields

        private GameBase _game;

        #endregion

        #region Constructors

        public GameViewModel(GameBase game)
        {
            _game = game;
        }

        #endregion

        #region Properties

        public string Id => _game.Id;

        public BitmapImage IconSource
        {
            get
            {
                try
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        _game.GetIcon().Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        BitmapImage bitmapimage = new BitmapImage();
                        bitmapimage.BeginInit();
                        bitmapimage.StreamSource = memory;
                        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapimage.EndInit();

                        return bitmapimage;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public string GameName => _game.Name;

        public bool IsSteam => _game.Launcher == LauncherType.Steam;

        public bool IsEpic => _game.Launcher == LauncherType.Epic;

        #endregion

        #region Methods

        public void Open()
        {
            var settings = new GameSettingsFile();
            settings.Initialize();
            settings.AddRecent(_game.Id);

            _game.Open();
        }

        #endregion
    }
}
