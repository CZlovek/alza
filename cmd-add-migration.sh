#!/bin/bash

# Check if migration name parameter is provided
if [ -z "$1" ]; then
    echo "Parameter Migration name is not provided"
    exit 1
fi

# Clear the screen
clear

# Build and add migration
dotnet build && dotnet ef migrations add "$1" --project AlzaShopApi/AlzaShopApi.csproj

