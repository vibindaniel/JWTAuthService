FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
ENV ASPNETCORE_URLS=http://+:5000
WORKDIR /app
EXPOSE 53757
EXPOSE 44361

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["JWTAuth.csproj", ""]
RUN dotnet restore "/JWTAuth.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "JWTAuth.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "JWTAuth.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN apt-get update
RUN apt-get install libsqlite3-dev -y
ENTRYPOINT ["dotnet", "JWTAuth.dll"]