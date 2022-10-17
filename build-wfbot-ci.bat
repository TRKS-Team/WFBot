@echo off
REM dotnet publish WFBot -o out/WFBotLinuxArm64 -r linux-arm64 --self-contained false -c "Linux Release" -p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
REM dotnet publish WFBot -o out/WFBotLinux -r linux-x64 --self-contained false -c "Linux Release" -p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
REM dotnet publish WFBot -o out/WFBotWindows -r win-x86 --self-contained false -c "Linux Release" -p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

powershell -Command "(gc WFBot/WFBot.csproj) -replace 'net6.0', 'net7.0' | Out-File -encoding ASCII WFBot/WFBot.csproj"
dotnet publish WFBot -o out/WFBotLinuxArm64S -r linux-arm64 --self-contained true -c "Linux Release" -p:PublishSingleFile=true /p:TargetFramework=net7.0 /p:IncludeNativeLibrariesForSelfExtract=true 
dotnet publish WFBot -o out/WFBotLinuxX64S -r linux-x64 --self-contained true -c "Linux Release" -p:PublishSingleFile=true /p:TargetFramework=net7.0 /p:IncludeNativeLibrariesForSelfExtract=true 
dotnet publish WFBot -o out/WFBotWindowsS -r win-x86 --self-contained true -c "Linux Release" -p:PublishSingleFile=true /p:TargetFramework=net7.0 /p:IncludeNativeLibrariesForSelfExtract=true 
