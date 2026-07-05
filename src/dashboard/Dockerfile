FROM node:20-alpine AS frontend-build

WORKDIR /client

COPY dashboard/client-app/package*.json ./
RUN npm ci

COPY dashboard/client-app/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY dashboard/dashboard.csproj dashboard/
COPY common/connector-sytem.Common/connector-sytem.Common.csproj common/connector-sytem.Common/

RUN dotnet restore dashboard/dashboard.csproj

COPY . .

RUN dotnet publish dashboard/dashboard.csproj -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

COPY --from=frontend-build /client/dist ./wwwroot

EXPOSE 5741

ENV ASPNETCORE_HTTP_PORTS=5741
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "dashboard.dll"]