FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG BUILD_CONFIGURATION
ENV BUILD_CONFIGURATION=${BUILD_CONFIGURATION:-Release}

COPY ["backend/Apps/AuthApi/Api/Api.csproj", "backend/Apps/AuthApi/Api/"]
COPY ["backend/Apps/AuthApi/Application/Application.csproj", "backend/Apps/AuthApi/Application/"]
COPY ["backend/Apps/AuthApi/Domain/Domain.csproj", "backend/Apps/AuthApi/Domain/"]
COPY ["backend/Apps/AuthApi/Infrastructure/Infrastructure.csproj", "backend/Apps/AuthApi/Infrastructure/"]
COPY ["backend/Libs/Infrastructure/Infrastructure.AutoMapper/Infrastructure.AutoMapper.csproj", "backend/Libs/Infrastructure/Infrastructure.AutoMapper/"]
COPY ["backend/Libs/Infrastructure/Infrastructure.Core/Infrastructure.Core.csproj", "backend/Libs/Infrastructure/Infrastructure.Core/"]
COPY ["backend/Libs/Infrastructure/Infrastructure.Database/Infrastructure.Database.csproj", "backend/Libs/Infrastructure/Infrastructure.Database/"]
COPY ["backend/Libs/Infrastructure/Infrastructure.Hosting/Infrastructure.Hosting.csproj", "backend/Libs/Infrastructure/Infrastructure.Hosting/"]
COPY ["backend/Libs/Infrastructure/Infrastructure.Logging/Infrastructure.Logging.csproj", "backend/Libs/Infrastructure/Infrastructure.Logging/"]
COPY ["backend/Libs/Infrastructure/Infrastructure.Options/Infrastructure.Options.csproj", "backend/Libs/Infrastructure/Infrastructure.Options/"]
COPY ["backend/Libs/Infrastructure/Infrastructure.Swagger/Infrastructure.Swagger.csproj", "backend/Libs/Infrastructure/Infrastructure.Swagger/"]
COPY ["backend/Libs/Security/Security.Host/Security.Host.csproj", "backend/Libs/Security/Security.Host/"]

RUN dotnet restore "backend/Apps/AuthApi/Api/Api.csproj"

COPY . .

WORKDIR "/src/backend/Apps/AuthApi/Api"

RUN dotnet publish "Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]