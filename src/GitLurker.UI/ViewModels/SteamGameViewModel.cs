﻿using System.IO;
using System.Windows.Media.Imaging;
using GitLurker.Models;
using SteamLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class SteamGameViewModel : ItemViewModelBase
    {
        #region Fields

        private SteamGame _game;

        #endregion

        #region Constructors

        public SteamGameViewModel(SteamGame game)
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
        }

        public string GameName => _game.Name;

        #endregion

        #region Methods

        public void Open()
        {
            var settings = new SteamSettingsFile();
            settings.Initialize();
            settings.AddRecent(_game.Id);

            _game.Open();
        }

        #endregion
    }
}