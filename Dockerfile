# Базовый образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Установка FFmpeg
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

# Установка зависимостей для SkiaSharp
RUN apt-get update && apt-get install -y \
    libc6 \
    libfontconfig1 \
    libfreetype6 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libxrandr2 \
    libxcb1 \
    libxfixes3 \
    libxi6 \
    libgl1-mesa-glx \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081

# Образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копирование csproj-файлов
COPY ["Alex.YouTube.Joker.Host/Alex.YouTube.Joker.Host.csproj", "Alex.YouTube.Joker.Host/"]
COPY ["Alex.YouTube.Joker.Domain/Alex.YouTube.Joker.Domain.csproj", "Alex.YouTube.Joker.Domain/"]
COPY ["Alex.YouTube.Joker.DomainServices/Alex.YouTube.Joker.DomainServices.csproj", "Alex.YouTube.Joker.DomainServices/"]

# Восстановление зависимостей
RUN dotnet restore "Alex.YouTube.Joker.Host/Alex.YouTube.Joker.Host.csproj"

# Копирование исходного кода и сборка
COPY . .
WORKDIR "/src/Alex.YouTube.Joker.Host"
RUN dotnet build "Alex.YouTube.Joker.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация сборки
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Alex.YouTube.Joker.Host.csproj" -c $BUILD_CONFIGURATION -r linux-x64 -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN ls -l /app  # Временная проверка на наличие libSkiaSharp.so
ENTRYPOINT ["dotnet", "Alex.YouTube.Joker.Host.dll"]
