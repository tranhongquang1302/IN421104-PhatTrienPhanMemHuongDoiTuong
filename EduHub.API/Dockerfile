FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY EduHub.Domain/EduHub.Domain.csproj EduHub.Domain/
COPY EduHub.Application/EduHub.Application.csproj EduHub.Application/
COPY EduHub.Infrastructure/EduHub.Infrastructure.csproj EduHub.Infrastructure/
COPY EduHub.API/EduHub.API.csproj EduHub.API/
RUN dotnet restore EduHub.API/EduHub.API.csproj

COPY EduHub.Domain/ EduHub.Domain/
COPY EduHub.Application/ EduHub.Application/
COPY EduHub.Infrastructure/ EduHub.Infrastructure/
COPY EduHub.API/ EduHub.API/
RUN dotnet publish EduHub.API/EduHub.API.csproj -c Release --no-restore -o /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish/ ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "EduHub.API.dll"]
