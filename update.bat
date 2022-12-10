@echo off
timeout 2 && dotnet build -c release && .\\src\\GitLurker.UI\\bin\\Release\\net6.0-windows\\GitLurker.UI.exe