FROM node:20 AS frontend-build
WORKDIR /src/Client
COPY Client/package*.json ./
RUN npm ci
COPY Client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Server/API/API.csproj", "Server/API/"]
COPY ["Server/Core/Core.csproj", "Server/Core/"]
COPY ["Server/Infrastructure/Infrastructure.csproj", "Server/Infrastructure/"]
RUN dotnet restore "Server/API/API.csproj"

COPY . .
COPY --from=frontend-build /src/Client/dist /src/Server/API/wwwroot
WORKDIR /src/Server/API
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
