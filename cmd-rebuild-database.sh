#!/bin/bash

# Clear the screen
clear

# Run migration and update database scripts
./add-migration.sh DbInit
./update-database.sh
