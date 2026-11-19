FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src
COPY backend/ backend/
RUN dotnet build backend/Apps/CyberBackup/Tests/Integration.Tests/Integration.Tests.csproj -c Release
CMD ["dotnet", "test", "backend/Apps/CyberBackup/Tests/Integration.Tests/Integration.Tests.csproj", "-c", "Release", "--no-build"]
