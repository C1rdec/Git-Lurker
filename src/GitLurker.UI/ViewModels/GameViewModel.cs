namespace GitLurker.UI.ViewModels;

using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GitLurker.Core;
using GitLurker.Core.Models;
using GitLurker.Core.Services;
using GitLurker.UI.Messages;
using Lurker.Common.Models;

public class GameViewModel : ItemViewModelBase
{
    #region Fields

    private static readonly CloseMessage CloseMessage = new();
    private GameBase _game;
    private IEventAggregator _eventAggregator;
    private ProcessService _processService;

    #endregion

    #region Constructors

    public GameViewModel(GameBase game)
    {
        _game = game;
        _processService = new ProcessService(Path.GetDirectoryName(game.ExeFilePath));
        _eventAggregator = IoC.Get<IEventAggregator>();
    }

    #endregion

    #region Properties

    public override string Id => _game.Id;

    public BitmapSource IconSource
    {
        get
        {
            try
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                _game.GetIcon().GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

                return bitmapSource;
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

    public bool IsBattleNet => _game.Launcher == LauncherType.BattleNet;

    #endregion

    #region Methods

    public void Open()
    {
        var settings = new GameSettingsFile();
        settings.Initialize();
        settings.AddRecent(_game.Id);
        _eventAggregator.PublishOnCurrentThreadAsync(CloseMessage);

        if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_CONTROL))
        {
            if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_SHIFT))
            {
                _ = _processService.ExecuteCommandAsync($"explorer \"{Path.GetDirectoryName(_game.ExeFilePath)}\"");
                return;
            }
        }

        _game.Open();
    }

    #endregion
}
