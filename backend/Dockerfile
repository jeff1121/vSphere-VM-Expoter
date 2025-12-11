FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./backend/src/*.csproj ./
RUN dotnet restore
COPY ./backend/src/. ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV LANG C.UTF-8
ENV LC_ALL C.UTF-8
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Install OVF Tool
COPY ./docs/ovftool /usr/lib/vmware-ovftool
RUN chmod +x /usr/lib/vmware-ovftool/ovftool && \
    ln -s /usr/lib/vmware-ovftool/ovftool /usr/local/bin/ovftool

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Backend.dll"]
