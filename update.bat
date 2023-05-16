@echo off
timeout 10 && dotnet build -c release && .\\src\\GitLurker.UI\\bin\\Release\\net6.0-windows\\GitLurker.exe