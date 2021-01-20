@echo off
dotnet publish WFBot -p:DefineConstants="NoGitVersion,TRACE" -o out/WFBotWindows -c "Windows Release"
dotnet publish WFBot -p:DefineConstants="NoGitVersion,TRACE" -o out/WFBotLinux -c "Linux Release"