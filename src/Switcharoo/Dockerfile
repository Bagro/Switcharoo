FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENV CorsOrigins="http://localhost"
ENV ConnectionStrings__SwitcharooDb="Data Source=/data/switcharoo.db"
ENV HTTP_Only=true

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Switcharoo.csproj", "Switcharoo/"]
RUN dotnet restore "Switcharoo/Switcharoo.csproj"
WORKDIR "/src/Switcharoo"
COPY . .

RUN dotnet build "Switcharoo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Switcharoo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir /data
VOLUME /data

ENTRYPOINT ["dotnet", "Switcharoo.dll"]
