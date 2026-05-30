FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/BLL/BLL.csproj", "src/BLL/"]
COPY ["src/DAL/DAL.csproj", "src/DAL/"]
COPY ["src/API/API.csproj", "src/API/"]

RUN dotnet restore "src/API/API.csproj"

COPY . .

WORKDIR "/source/src/API"
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]

