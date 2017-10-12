dotnet restore ../
dotnet publish ../  -c release -o ./obj/Docker/publish

docker build ../ -t dms-web-api

docker-compose run

#docker-compose -f compose-dms-mongodb.yml up
#ipconfig to get ip