#!/bin/sh

API_HOST=${API_HOST:-api}
API_PORT=${API_PORT:-8080}
INTERVAL=${INTERVAL:-3}

echo "Client starting..."
echo "Running as user: $(whoami)"
echo "Working directory: $(pwd)"

# Bridge networking:
# This is constructing the URL the client will use to call the API. 
# Notice it uses $API_HOST which defaults to api — that's the Docker Compose service name, not an IP address. 
# When this script actually makes the call, Docker's internal DNS will resolve api to whatever IP address the API container is running at.
echo "Will call: http://$API_HOST:$API_PORT every ${INTERVAL}s"
echo "-------------------------------------------"

while true; do
  echo "[$(date +%H:%M:%S)] Calling API..."

  # HTTP call to the API container. -q means quiet mode, 
  # -O - means write the response to stdout instead of a file
  RESPONSE=$(wget -q -O - "http://$API_HOST:$API_PORT" 2>&1)

  if [ $? -eq 0 ]; then
    echo "Response: $RESPONSE"
  else
    echo "Could not reach API - retrying in ${INTERVAL}s..."
  fi

  echo "-------------------------------------------"
  sleep "$INTERVAL"
done