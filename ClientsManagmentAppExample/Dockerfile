#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ClientsManagmentApp/ClientsManagmentApp.csproj", "ClientsManagmentApp/"]
RUN dotnet restore "ClientsManagmentApp/ClientsManagmentApp.csproj"
COPY . .
WORKDIR "/src/ClientsManagmentApp"
RUN dotnet build "ClientsManagmentApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClientsManagmentApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientsManagmentApp.dll"]