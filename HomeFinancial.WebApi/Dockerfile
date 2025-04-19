FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HomeFinancial.DataService/HomeFinancial.DataService.csproj", "HomeFinancial.DataService/"]
COPY ["HomeFinancial.Repository/HomeFinancial.Repository.csproj", "HomeFinancial.Repository/"]
RUN dotnet restore "HomeFinancial.DataService/HomeFinancial.DataService.csproj"
COPY . .
WORKDIR "/src/HomeFinancial.DataService"
RUN dotnet build "HomeFinancial.DataService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HomeFinancial.DataService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HomeFinancial.DataService.dll"]
