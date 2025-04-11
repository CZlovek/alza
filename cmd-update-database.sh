#!/bin/bash

# Clear the screen
clear

# Run database update with correct path format
dotnet ef database update --project AlzaShopApi/AlzaShopApi.csproj
