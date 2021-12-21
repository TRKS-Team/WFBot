@echo off
dotnet publish Connectors/MiraiHTTPConnector -p:DefineConstants="NoGitVersion" -o out/MiraiConnector -c "Linux Release"
dotnet publish Connectors/MiraiHTTPConnectorWithImageRendering -p:DefineConstants="NoGitVersion" -o out/MiraiConnectorWithImageRendering -c "Linux Release"
dotnet publish Connectors/TestConnector -p:DefineConstants="NoGitVersion" -o out/TestConnector -c "Linux Release"
dotnet publish Connectors/OneBotConnector -p:DefineConstants="NoGitVersion" -o out/OneBotConnector -c "Linux Release"
