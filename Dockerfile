FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Crystal.sln ./
COPY Crystal.Web/Crystal.Web.csproj Crystal.Web/
RUN dotnet restore Crystal.Web/Crystal.Web.csproj

COPY Crystal.Web/ Crystal.Web/
RUN dotnet publish Crystal.Web/Crystal.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Crystal.Web.dll"]
