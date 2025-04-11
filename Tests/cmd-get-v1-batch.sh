#!/bin/bash

# Clear the screen
clear

# Set counter based on argument
if [ -z "$1" ]; then
    counter=10000
else
    counter=$1
fi

# Function to generate a random number between min and max
rand() {
    local min=$1
    local max=$2
    echo $(( $RANDOM % ($max - $min + 1) + $min ))
}

# Main loop
for (( i=1; i<=$counter; i++ ))
do
    # Generate random productId between 1 and 1000
    productId=$(rand 1 1000)
    
    # Call the API
    curl --connect-timeout 10 --location --request GET "http://localhost:5167/api/product/$productId" --header "Content-Type: application/json;"
    
    echo "Calling API GET with counter:$i productId:$productId"
done
