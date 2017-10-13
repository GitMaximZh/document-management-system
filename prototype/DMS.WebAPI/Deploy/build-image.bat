dotnet restore ../
dotnet publish ../  -c release -o ./obj/Docker/publish

docker build ../ -t dms-web-api

#ipconfig to get ip

docker-compose -f compose-dms-mongodb.yml up
#docker-compose run

#should be removed otherwise docker container does not work as it can not to bind to localhost:5001
#.UseUrls("http://localhost:5001/")
#docker run -p 8080:80 dms-web-api