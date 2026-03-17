#!/bin/sh

# Simple HTTP server using netcat
# Demonstrates: a real running process as PID 1 inside a container

PORT=${PORT:-8080}
HOSTNAME=$(hostname)

echo "Server starting on port $PORT"
echo "Running as user: $(whoami)"
echo "Working directory: $(pwd)"
echo "Container hostname: $HOSTNAME"

# Keep serving requests
while true; do
  # Build response body
  BODY=$(cat <<EOF
{
  "message": "Hello from the API container!",
  "hostname": "$HOSTNAME",
  "user": "$(whoami)",
  "workdir": "$(pwd)",
  "timestamp": "$(date -Iseconds)"
}
EOF
)

  # Serve one HTTP response via netcat
  echo -e "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: ${#BODY}\r\nConnection: close\r\n\r\n$BODY" | nc -l -p "$PORT" -q 1
done
