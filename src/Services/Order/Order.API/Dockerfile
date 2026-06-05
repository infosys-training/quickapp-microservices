FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for restore
COPY Shared/Shared.Contracts/Shared.Contracts.csproj Shared/Shared.Contracts/
COPY Shared/Shared.Infrastructure/Shared.Infrastructure.csproj Shared/Shared.Infrastructure/
COPY Services/Order/Order.Domain/Order.Domain.csproj Services/Order/Order.Domain/
COPY Services/Order/Order.Infrastructure/Order.Infrastructure.csproj Services/Order/Order.Infrastructure/
COPY Services/Order/Order.API/Order.API.csproj Services/Order/Order.API/

RUN dotnet restore Services/Order/Order.API/Order.API.csproj

# Copy source files
COPY Shared/ Shared/
COPY Services/Order/ Services/Order/

WORKDIR /src/Services/Order/Order.API
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5003
ENTRYPOINT ["dotnet", "Order.API.dll"]
