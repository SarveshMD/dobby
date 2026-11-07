# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files
COPY src/Dobby/Dobby.csproj src/Dobby/
RUN dotnet restore src/Dobby/Dobby.csproj

# Install dotnet-ef tool to generate the bundle
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Create the self-contained migration executable
COPY src/Dobby/ src/Dobby/
WORKDIR /app/src/Dobby

RUN ConnectionStrings__DefaultConnection="Host=localhost;Database=dummy" \
    dotnet ef migrations bundle --output /out/efbundle -r linux-x64 --self-contained

# Copy remaining source code and publish in Release mode
RUN dotnet publish -c Release -o /out/app --no-restore

# Stage 2: Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out/app .

# Set environment defaults for the container
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000
ENTRYPOINT ["dotnet", "Dobby.dll"]

# Stage 3: Migration Runner Target
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migration
WORKDIR /app
COPY --from=build /out/efbundle .
RUN chmod +x ./efbundle
ENTRYPOINT ["./efbundle"]
