FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["DLDA.GUI.Local/DLDA.GUI/DLDA.GUI.csproj", "DLDA.GUI.Local/DLDA.GUI/"]
RUN dotnet restore "DLDA.GUI.Local/DLDA.GUI/DLDA.GUI.csproj"

COPY . .
WORKDIR "/src/DLDA.GUI.Local/DLDA.GUI"
RUN dotnet build "DLDA.GUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DLDA.GUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DLDA.GUI.dll"]
