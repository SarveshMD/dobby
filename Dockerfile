# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files
COPY src/Dobby/Dobby.csproj src/Dobby/
RUN dotnet restore src/Dobby/Dobby.csproj

# Copy remaining source code and publish in Release mode
COPY src/Dobby/ src/Dobby/
WORKDIR /app/src/Dobby
RUN dotnet publish -c Release -o /out --no-restore

# Stage 2: Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

# Set environment defaults for the container
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 5000
ENTRYPOINT ["dotnet", "Dobby.dll"]
