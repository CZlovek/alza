#!/bin/bash

# Stop and remove containers, networks, and volumes defined in docker-compose
docker-compose down

# Remove unused build cache
docker builder prune -f

# Remove unused data (containers, networks, images)
docker system prune -f
