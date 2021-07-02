@echo off
set NoGitVersion=hso
dotnet publish WFBot -o out/WFBotWindows -c "Windows Release"
dotnet publish WFBot -o out/WFBotLinux -c "Linux Release"