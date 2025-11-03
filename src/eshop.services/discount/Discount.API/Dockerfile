FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 6064

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eshop.services/discount/Discount.API/Discount.API.csproj", "eshop.services/discount/Discount.API/"]
COPY ["eshop.services/discount/Discount.Grpc/Discount.Grpc.csproj", "eshop.services/discount/Discount.Grpc/"]
COPY ["eshop.services/discount/Discount.Grpc/Protos/discount.proto", "eshop.services/discount/Discount.Grpc/Protos/"]
RUN dotnet restore "eshop.services/discount/Discount.API/Discount.API.csproj"
COPY . .
WORKDIR "/src/eshop.services/discount/Discount.API"
RUN dotnet build "./Discount.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Discount.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discount.API.dll"]

