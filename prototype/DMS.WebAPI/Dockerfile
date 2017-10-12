FROM microsoft/aspnetcore:1.1
WORKDIR /app
COPY obj/Docker/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "DMS.WebAPI.dll"]
