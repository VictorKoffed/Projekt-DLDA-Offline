# Steg 1: Kör-miljön 
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Steg 2: Bygg-miljön 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["DLDA.API.Local/DLDA.API/DLDA.API.csproj", "DLDA.API.Local/DLDA.API/"]
RUN dotnet restore "DLDA.API Local/DLDA.API/DLDA.API.csproj"
COPY . .
WORKDIR "/src/DLDA.API.Local/DLDA.API"
RUN dotnet build "DLDA.API.csproj" -c Release -o /app/build

# Steg 3: Publicera
FROM build AS publish
RUN dotnet publish "DLDA.API.csproj" -c Release -o /app/publish

# Steg 4: Slå ihop till slutgiltig image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DLDA.API.dll"]
