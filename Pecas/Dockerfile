FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Pecas.csproj && \
	dotnet publish Pecas.csproj \
		--configuration Release \
		--output /app/publish \
		--no-restore \
		/p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
	ASPNETCORE_CONTENTROOT=/app
EXPOSE 8080
VOLUME ["/data"]

RUN mkdir -p /data && chown app:app /app /data
COPY --from=build --chown=app:app /app/publish .

USER app
WORKDIR /data
ENTRYPOINT ["dotnet", "/app/Pecas.dll"]
