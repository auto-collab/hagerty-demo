#!/bin/sh

# Simple client that calls the API container
# Demonstrates: container networking - this container reaches the API by service name

API_HOST=${API_HOST:-api}
API_PORT=${API_PORT:-8080}
INTERVAL=${INTERVAL:-3}

echo "Client starting..."
echo "Running as user: $(whoami)"
echo "Working directory: $(pwd)"
echo "Will call: http://$API_HOST:$API_PORT every ${INTERVAL}s"
echo "-------------------------------------------"

while true; do
  echo "[$(date +%H:%M:%S)] Calling API..."

  # Call the API container by its Docker Compose service name
  RESPONSE=$(wget -q -O - "http://$API_HOST:$API_PORT" 2>&1)

  if [ $? -eq 0 ]; then
    echo "Response: $RESPONSE"
  else
    echo "Could not reach API - retrying in ${INTERVAL}s..."
  fi

  echo "-------------------------------------------"
  sleep "$INTERVAL"
done
