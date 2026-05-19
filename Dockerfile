FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DevOpsBoard.sln ./
COPY src/DevOpsBoard.Api/DevOpsBoard.Api.csproj src/DevOpsBoard.Api/
RUN dotnet restore DevOpsBoard.sln

COPY . .
RUN dotnet publish src/DevOpsBoard.Api/DevOpsBoard.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DevOpsBoard.Api.dll"]
