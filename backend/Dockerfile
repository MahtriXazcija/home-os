FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY HomeOS.sln .
COPY src/HomeOS.Domain/HomeOS.Domain.csproj src/HomeOS.Domain/
COPY src/HomeOS.Application/HomeOS.Application.csproj src/HomeOS.Application/
COPY src/HomeOS.Infrastructure/HomeOS.Infrastructure.csproj src/HomeOS.Infrastructure/
COPY src/HomeOS.Api/HomeOS.Api.csproj src/HomeOS.Api/
RUN dotnet restore src/HomeOS.Api/HomeOS.Api.csproj

COPY . .
RUN dotnet publish src/HomeOS.Api/HomeOS.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HomeOS.Api.dll"]
