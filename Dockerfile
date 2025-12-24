FROM mcr.microsoft.com/dotnet/aspnet:10.0

EXPOSE 5000 51413/tcp 51413/udp

WORKDIR /app

COPY publish/ .

RUN mkdir data cache movies series downloads \
  && chown 1000:1000 data cache movies series downloads

USER 1000:1000

ENTRYPOINT ["dotnet", "Torri.dll"]
