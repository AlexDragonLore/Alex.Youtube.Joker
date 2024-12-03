FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Alex.YouTube.Joker.Host/Alex.YouTube.Joker.Host.csproj", "Alex.YouTube.Joker.Host/"]
COPY ["Alex.YouTube.Joker.Domain/Alex.YouTube.Joker.Domain.csproj", "Alex.YouTube.Joker.Domain/"]
COPY ["Alex.YouTube.Joker.DomainServices/Alex.YouTube.Joker.DomainServices.csproj", "Alex.YouTube.Joker.DomainServices/"]
RUN dotnet restore "Alex.YouTube.Joker.Host/Alex.YouTube.Joker.Host.csproj"
COPY . .
WORKDIR "/src/Alex.YouTube.Joker.Host"
RUN dotnet build "Alex.YouTube.Joker.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Alex.YouTube.Joker.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alex.YouTube.Joker.Host.dll"]
