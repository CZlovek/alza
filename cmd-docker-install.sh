#!/bin/bash

# Clear the screen
clear

# Start Docker containers in detached mode and remove orphans
docker compose up -d --remove-orphans
