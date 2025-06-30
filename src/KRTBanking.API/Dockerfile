FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/KRTBanking.API/KRTBanking.API.csproj", "src/KRTBanking.API/"]
COPY ["src/KRTBanking.Application/KRTBanking.Application.csproj", "src/KRTBanking.Application/"]
COPY ["src/KRTBanking.Domain/KRTBanking.Domain.csproj", "src/KRTBanking.Domain/"]

# Restore packages
RUN dotnet restore "src/KRTBanking.API/KRTBanking.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/KRTBanking.API"
RUN dotnet build "./KRTBanking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KRTBanking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KRTBanking.API.dll"]
