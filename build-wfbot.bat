@echo off
dotnet publish WFBot -p:DefineConstants="TRACE" -o out/WFBotWindows -c "Windows Release"
dotnet publish WFBot -p:DefineConstants="TRACE" -o out/WFBotLinux -c "Linux Release"