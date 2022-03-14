﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace GitLurker.UI.Services
{
    public class WindowsStartupService
    {
        #region Fields

        private string _linkName;
        private string _description;

        #endregion

        #region Constructors

        public WindowsStartupService(string linkName, string description)
        {
            _linkName = linkName;
            _description = description;
        }

        #endregion

        #region Properties

        private string ApplicationDataFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private string ProgramsFolderPath => Path.Combine(ApplicationDataFolderPath, @"Microsoft\Windows\Start Menu\Programs");

        private string StartupFolderPath => Path.Combine(ProgramsFolderPath, "Startup");

        private string StartupFilePath => Path.Combine(StartupFolderPath, _linkName);

        private string MenuFilePath => Path.Combine(ProgramsFolderPath, _linkName);

        #endregion

        #region Methods

        public void AddStartup() => CreateLink(StartupFilePath);

        public void RemoveStartup() => RemoveLink(StartupFilePath);

        public void AddToStartMenu() => CreateLink(MenuFilePath);

        public void RemoveMenu() => RemoveLink(MenuFilePath);

        private void RemoveLink(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void CreateLink(string path)
        {
            RemoveLink(path);

            var link = (IShellLink)new ShellLink();
            link.SetDescription(_description);
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            link.SetPath(exePath);
            var file = (IPersistFile)link;
            file.Save(path, false);
        }

        #endregion

        #region Natives

#pragma warning disable
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        #endregion
    }
}
