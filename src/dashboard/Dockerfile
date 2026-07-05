FROM node:20-alpine AS frontend-build

WORKDIR /client

COPY client-app/package*.json ./
RUN npm ci

COPY client-app/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 as final
WORKDIR /app

COPY --from=build /app/publish .

COPY --from=frontend-build /client/dist ./wwwroot

EXPOSE 5741

ENV ASPNETCORE_HTTP_PORTS=5741
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "dashboard.dll"]