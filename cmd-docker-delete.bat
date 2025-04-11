@echo off

docker-compose down

docker builder prune -f
docker system prune -f