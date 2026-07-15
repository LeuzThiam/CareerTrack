FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CareerTrack/CareerTrack.csproj CareerTrack/
RUN dotnet restore CareerTrack/CareerTrack.csproj

COPY CareerTrack/ CareerTrack/
RUN dotnet publish CareerTrack/CareerTrack.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Les images .NET officielles incluent déjà un utilisateur non-root nommé "app".
USER app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CareerTrack.dll"]
