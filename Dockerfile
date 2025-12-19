FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000 51413/tcp 51413/udp

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Torri/Torri.csproj Torri/
RUN dotnet restore "Torri/Torri.csproj"
COPY src/ .
RUN dotnet build "Torri/Torri.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Torri/Torri.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

RUN mkdir data && chmod 777 data && mkdir cache && chmod 777 cache && mkdir movies && chmod 777 movies && mkdir series && chmod 777 series && mkdir downloads && chmod 777 downloads

USER 1000:1000
ENTRYPOINT ["dotnet", "Torri.dll"]
