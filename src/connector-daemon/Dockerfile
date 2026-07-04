FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5739

ENV ASPNETCORE_HTTP_PORTS=5739
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "connector-daemon.dll"]