#!/bin/sh

PORT=${PORT:-8080}
HOSTNAME=$(hostname)
WORKDIR=$(pwd)
USER=$(whoami)

echo "Server starting on port $PORT"
echo "Running as user: $USER"
echo "Working directory: $WORKDIR"
echo "Container hostname: $HOSTNAME"

# This is what keeps container running. Is PID1
while true; do
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

  echo -e "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: ${#BODY}\r\nConnection: close\r\n\r\n$BODY" | nc -l -p "$PORT" -q 1
done