#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
ENV TZ=Asia/Shanghai
RUN printf "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ bullseye main contrib non-free\ndeb https://mirrors.tuna.tsinghua.edu.cn/debian/ bullseye-updates main contrib non-free\ndeb https://mirrors.tuna.tsinghua.edu.cn/debian/ bullseye-backports main contrib non-free\ndeb https://mirrors.tuna.tsinghua.edu.cn/debian-security bullseye-security main contrib non-free" > /etc/apt/sources.list
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN apt update && apt install -y procps && rm -rf /var/lib/apt/lists/*
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
WORKDIR /src
COPY ["WFBot/WFBot.csproj", "WFBot/"]
RUN sed -i -e 's/net6.0/net7.0/g' WFBot/WFBot.csproj
RUN dotnet restore "WFBot/WFBot.csproj"
COPY . .
ENV OFFICIAL_BUILD=official
#WORKDIR "/src/WFBot"
RUN dotnet publish "WFBot" -c "Linux Release" -o /app/publish && rm -rf .git

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WFBot.dll", "--", "--use-config-folder"]
