FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["conn_test/conn_test.csproj", "conn_test/"]
RUN dotnet restore "conn_test/conn_test.csproj"
COPY . .
WORKDIR "/src/conn_test"
RUN dotnet build "conn_test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "conn_test.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "conn_test.dll"]
