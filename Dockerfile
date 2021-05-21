#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["HybridCachingWithEasyCaching.csproj", "."]
RUN dotnet restore "./HybridCachingWithEasyCaching.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "HybridCachingWithEasyCaching.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HybridCachingWithEasyCaching.csproj" -c Release -o /app/publish -r linux-x64 --self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HybridCachingWithEasyCaching.dll"]