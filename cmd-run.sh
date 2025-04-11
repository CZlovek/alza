#!/bin/bash

# Clear the screen
clear

./cmd-docker-install.sh

# Run database update with correct path format
dotnet run --project AlzaShopApi/AlzaShopApi.csproj
