# Git-Lurker <a href="https://apps.microsoft.com/store/detail/9N2MN78QLVKB?launch=true&mode=mini" target="_blank"><img align='right' src="https://get.microsoft.com/images/en-US%20dark.svg" height="100"></a>

GitLurker is an overlay to ease the navigation between Git Repositories

## Hotkeys

`CTRL + G` : Is the Default Hotkey to open the overlay.

`Enter` : Will launch the selected project using your default editor. ([Visual Studio](https://visualstudio.microsoft.com/vs/community/) | [Rider](https://www.jetbrains.com/rider/) | [VSCode](https://code.visualstudio.com/)) 

`CTRL + Enter` : Will open [Windows Terminal](https://www.microsoft.com/en-ca/p/windows-terminal/9n0dx20hk701?activetab=pivot:overviewtab) to the Git Repository path.

`CTRL + Shift + Enter` : Will open Windows File Explorer to the Git Repository path.

![image](https://user-images.githubusercontent.com/5436436/173985733-0d8dc9c6-fbad-4d79-a83d-36d6f746aa85.png)

# Usage
- `git clone https://github.com/C1rdec/Git-Lurker.git`
- Build the Solution with your favorite IDE **or** run this command:
  - `dotnet run --project .\Git-Lurker\src\GitLurker.UI\GitLurker.UI.csproj -c release`

After the first Launch **GitLurker** will set a shortcut to the ***Startup Menu*** and ***Start with Windows***. 
<br/>(*Can be disabled in the Settings/Advanced*)

| System Tray | Add Workspace Folder Path |
| ------------- | ------------- |
| ![image](https://user-images.githubusercontent.com/5436436/159106241-eac5b233-10a4-4dbc-a781-3f1944c08c84.png)  | ![image](https://user-images.githubusercontent.com/5436436/173984452-3dcea779-0c56-429f-bda0-0dec451245ad.png)

![GitLurker](https://user-images.githubusercontent.com/5436436/173988706-939889f4-a76d-42b3-abbe-0ee6a5e45a8b.gif)
