FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY connector-daemon/connector-daemon.csproj connector-daemon/
COPY common/connector-sytem.Common/connector-sytem.Common.csproj common/connector-sytem.Common/

RUN dotnet restore connector-daemon/connector-daemon.csproj

COPY . .

RUN dotnet publish connector-daemon/connector-daemon.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5739
ENV ASPNETCORE_HTTP_PORTS=5739
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "connector-daemon.dll"]