# Git-Lurker
GitLurker is an overlay to ease the navigation between Git Repositories

## Hotkeys

`CTRL + G` : Is the Default Hotkey to open the overlay.

`Enter` : Will launch the selected project using your default editor. ([Visual Studio](https://visualstudio.microsoft.com/vs/community/) | [Rider](https://www.jetbrains.com/rider/) | [VSCode](https://code.visualstudio.com/)) 

`CTRL + Enter` : Will open [Windows Terminal](https://www.microsoft.com/en-ca/p/windows-terminal/9n0dx20hk701?activetab=pivot:overviewtab) to the Git Repository path.

`CTRL + Shift + Enter` : Will open Windows File Explorer to the Git Repository path.

![2022-03-18_23-45-15](https://user-images.githubusercontent.com/5436436/159105476-c1a2fd86-b49a-49e2-9be9-7e803f224de7.gif)

# Usage
- `git clone https://github.com/C1rdec/Git-Lurker.git`
- Build the Solution with your favorite IDE **or** run this command:
  - `dotnet run --project .\Git-Lurker\src\GitLurker.UI\GitLurker.UI.csproj -c release`

After the first Launch **GitLurker** will set a shortcut to the ***Startup Menu*** and ***Start with Windows***. 
<br/>(*Can be disabled in the Settings/Advanced*)

| System Tray | Add Workspace Folder Path |
| ------------- | ------------- |
| ![image](https://user-images.githubusercontent.com/5436436/159106241-eac5b233-10a4-4dbc-a781-3f1944c08c84.png)  | ![image](https://user-images.githubusercontent.com/5436436/173984452-3dcea779-0c56-429f-bda0-0dec451245ad.png)



