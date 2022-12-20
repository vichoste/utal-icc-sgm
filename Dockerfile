#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Utal.Icc.Sgm.csproj", "."]
RUN dotnet restore "./Utal.Icc.Sgm.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Utal.Icc.Sgm.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Utal.Icc.Sgm.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utal.Icc.Sgm.dll"]