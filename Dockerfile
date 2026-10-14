FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AkarangaBank.Web.csproj", "./"]
RUN dotnet restore "AkarangaBank.Web.csproj"
COPY . .
RUN dotnet publish "AkarangaBank.Web.csproj" -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AkarangaBank.Web.dll"]
