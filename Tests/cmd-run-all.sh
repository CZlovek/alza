#!/bin/bash

# Clear the screen
clear

./cmd-get-v1-batch.sh & ./cmd-get-v2-batch.sh & ./cmd-get-v3-batch.sh & ./cmd-update-v1-batch.sh & ./cmd-update-v2-batch.sh & ./cmd-update-v3-batch.sh

# Display completion message
echo "All tests completed!"


