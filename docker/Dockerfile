FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/OtelAwsConsoleDemo/*.csproj src/OtelAwsConsoleDemo/
RUN dotnet restore src/OtelAwsConsoleDemo/OtelAwsConsoleDemo.csproj


COPY . .
RUN dotnet publish src/OtelAwsConsoleDemo/OtelAwsConsoleDemo.csproj -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OtelAwsConsoleDemo.dll"]
