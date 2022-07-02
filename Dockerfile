#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS publish
WORKDIR /src
COPY ["WFBot/WFBot.csproj", "WFBot/"]
RUN dotnet restore "WFBot/WFBot.csproj"
COPY . .
#WORKDIR "/src/WFBot"
RUN dotnet publish "WFBot" -c "Linux Release" -o /app/publish && rm -rf .git

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WFBot.dll", "--", "--use-config-folder"]
