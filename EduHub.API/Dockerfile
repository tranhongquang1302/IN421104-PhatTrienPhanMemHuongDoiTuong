FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY EduHub.Domain/EduHub.Domain.csproj EduHub.Domain/
COPY EduHub.Application/EduHub.Application.csproj EduHub.Application/
COPY EduHub.Infrastructure/EduHub.Infrastructure.csproj EduHub.Infrastructure/
COPY EduHub.API/EduHub.API.csproj EduHub.API/
RUN dotnet restore EduHub.API/EduHub.API.csproj
COPY . .
RUN dotnet publish EduHub.API/EduHub.API.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "EduHub.API.dll"]
