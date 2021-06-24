#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["WFBot/WFBot.csproj", "WFBot/"]
COPY ["TextCommandCore/TextCommandCore.csproj", "TextCommandCore/"]
RUN dotnet restore "WFBot/WFBot.csproj"
COPY . .
#WORKDIR "/src/WFBot"
RUN dotnet build "WFBot" -c "Linux Release" -o /app/build

FROM build AS publish
RUN dotnet publish "WFBot" -c "Linux Release" -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WFBot.dll"]