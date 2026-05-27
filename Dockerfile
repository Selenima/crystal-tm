# build stage собирает проект через sdk образ, тут есть dotnet restore и publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# сначала копируются sln и csproj, так docker может кешировать restore
COPY Crystal.sln ./
COPY Crystal.Web/Crystal.Web.csproj Crystal.Web/

# подтягиваем nuget пакеты до копирования всего кода
RUN dotnet restore Crystal.Web/Crystal.Web.csproj

# теперь копируется сам код приложения
COPY Crystal.Web/ Crystal.Web/

# publish собирает release версию без повторного restore
RUN dotnet publish Crystal.Web/Crystal.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# final stage легче, тут только runtime без sdk
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# приложение слушает 8080 порт внутри контейнера
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# берем готовые файлы из build stage
COPY --from=build /app/publish .

# команда которая стартует сайт в контейнере
ENTRYPOINT ["dotnet", "Crystal.Web.dll"]
