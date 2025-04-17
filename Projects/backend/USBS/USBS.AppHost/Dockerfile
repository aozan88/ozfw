FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AppHost/AppHost.csproj", "AppHost/"]
RUN dotnet restore "AppHost/AppHost.csproj"
COPY . .
WORKDIR "/src/AppHost"
RUN dotnet build "AppHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppHost.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
