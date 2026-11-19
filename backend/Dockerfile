FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["thirdpartynetwork/thirdpartynetwork.csproj", "thirdpartynetwork/"]
RUN dotnet restore "thirdpartynetwork/thirdpartynetwork.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/thirdpartynetwork"
RUN dotnet publish "thirdpartynetwork.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Use the much smaller ASP.NET 9 Runtime image to actually run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080 (the default for .NET 8/9 containers)
EXPOSE 8080

ENTRYPOINT ["dotnet", "thirdpartynetwork.dll"]