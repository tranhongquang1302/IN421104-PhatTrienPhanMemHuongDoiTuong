# TODO (Nhiệm vụ 3): hiện chỉ có MỘT giai đoạn -> image rất nặng (chứa cả SDK).
# Hãy tách thành 2 giai đoạn:
#   Giai đoạn 1 (build):   FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
#                          chép .csproj -> dotnet restore -> chép code -> dotnet publish
#   Giai đoạn 2 (runtime): FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
#                          COPY --from=build ... rồi ENTRYPOINT
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
WORKDIR /src
COPY . .
RUN dotnet publish EduHub.API/EduHub.API.csproj -c Release -o /app
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EduHub.API.dll"]
