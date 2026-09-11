FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MyApp.Api/MyApp.Api.csproj", "MyApp.Api/"]
COPY ["MyApp.Application/MyApp.Application.csproj", "MyApp.Application/"]
COPY ["MyApp.Domain/MyApp.Domain.csproj", "MyApp.Domain/"]
COPY ["MyApp.Infrastructure/MyApp.Infrastructure.csproj", "MyApp.Infrastructure/"]
RUN dotnet restore "MyApp.Api/MyApp.Api.csproj"

COPY . .
WORKDIR "/src/MyApp.Api"
RUN dotnet build "MyApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
