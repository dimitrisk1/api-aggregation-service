FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Core-Api/Core-Api.csproj", "src/Core-Api/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]

RUN dotnet restore "./src/Core-Api/Core-Api.csproj"

COPY . .
WORKDIR "/src/src/Core-Api"

RUN dotnet build "./Core-Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Core-Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Core-Api.dll"]
