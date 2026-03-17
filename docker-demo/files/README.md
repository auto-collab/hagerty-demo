# Docker Concepts Demo

A hands-on project demonstrating core container concepts through a simple
two-container setup — an API server and a client that calls it.

---

## Project Structure

```
docker-demo/
├── README.md               # This file
├── Containerfile           # Main build file - demonstrates layers, USER, WORKDIR, CMD/ENTRYPOINT
├── Containerfile.multi     # Multi-stage build demonstration
├── docker-compose.yml      # Container networking and resource limits
└── app/
    ├── server.sh           # Simple HTTP server (the API)
    └── client.sh           # Client that calls the API
```

---

## Concepts Demonstrated

### 1. Image Layers & Caching (Containerfile)

The `Containerfile` is ordered deliberately from **least to most frequently changed**:

```
Layer 1: FROM alpine        ← never changes
Layer 2: RUN apk install    ← changes rarely (dependency updates)
Layer 3: RUN groupadd/useradd ← changes rarely
Layer 4: WORKDIR            ← never changes
Layer 5: COPY app files     ← changes often (your code)
```

This means when you change your app code, only Layer 5 is rebuilt.
Layers 1-4 are pulled from cache instantly.

**RUN chaining** — cleanup happens in the same layer as install:
```dockerfile
# BAD - two layers, apt cache baked into layer 1 forever
RUN apk add curl
RUN rm -rf /var/cache/apk/*

# GOOD - one layer, cache files never make it in
RUN apk add --no-cache curl
```

---

### 2. USER, GROUP, and WORKDIR (Containerfile)

```dockerfile
# Create a non-root group and user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Set working directory (creates /app if it doesn't exist)
WORKDIR /app

# Copy files with correct ownership
COPY --chown=appuser:appgroup app/server.sh .

# Switch to non-root user - everything from here runs as appuser
USER appuser
```

Run this to verify the container is NOT running as root:
```bash
docker exec demo-api whoami
# Expected: appuser
```

---

### 3. ENTRYPOINT vs CMD (Containerfile)

```dockerfile
# ENTRYPOINT: fixed executable - cannot be replaced at runtime
# Exec form ["cmd"] makes our process PID 1 directly
# so it receives signals like SIGTERM correctly
ENTRYPOINT ["sh", "server.sh"]

# CMD: default arguments - CAN be overridden at runtime
CMD []
```

Override CMD at runtime:
```bash
# Normal run - uses CMD default
docker run demo-api

# Override CMD - passes --help as argument to entrypoint
docker run demo-api --help

# Override ENTRYPOINT entirely (requires --entrypoint flag)
docker run --entrypoint sh demo-api
```

---

### 4. Multi-Stage Build (Containerfile.multi)

Build the multi-stage image and compare sizes:
```bash
# Build single stage (with all tools)
docker build -f Containerfile -t demo:single .

# Build multi-stage (runtime only)
docker build -f Containerfile.multi -t demo:multi .

# Compare sizes
docker image ls | grep demo
```

The multi-stage build produces a smaller image because build tools
never make it into the final image.

---

### 5. Container Networking (docker-compose.yml)

The client container reaches the API by **service name**, not IP address:
```sh
# In client.sh
wget http://api:8080
#          ^^^
#          Docker's internal DNS resolves this to the API container's IP
```

Inspect the network:
```bash
# See all networks
docker network ls

# Inspect the demo network - shows both containers and their IPs
docker network inspect docker-demo_demo-network

# Exec into the client and manually ping the API by service name
docker exec -it demo-client sh
ping api
wget -q -O - http://api:8080
```

---

### 6. cgroups - Resource Limits (docker-compose.yml)

The compose file sets hard resource limits enforced by the Linux kernel:
```yaml
deploy:
  resources:
    limits:
      memory: 64m    # API container cannot use more than 64mb RAM
      cpus: "0.5"    # API container cannot use more than 0.5 CPU cores
```

Inspect actual resource usage:
```bash
docker stats
```

---

## Running the Demo

### Start everything
```bash
docker compose up --build
```

You should see the client calling the API every 3 seconds:
```
demo-client  | [10:23:01] Calling API...
demo-client  | Response: {"message": "Hello from the API container!", "hostname": "a3f8c2d1", ...}
```

### Call the API manually from your host
```bash
curl http://localhost:8080
```

### Inspect running containers
```bash
# See all running containers
docker ps

# See resource usage (cgroups in action)
docker stats

# Inspect the API container's configuration
docker inspect demo-api

# Check what user the process is running as
docker exec demo-api whoami

# Check the working directory
docker exec demo-api pwd

# See the process tree - note PID 1 is our shell script
docker exec demo-api ps aux
```

### Inspect image layers
```bash
# See all layers and their sizes
docker image history demo-docker-demo-api

# Detailed layer inspection
docker inspect demo-docker-demo-api
```

### Stop everything
```bash
docker compose down
```

---

## Key Takeaways

| Concept | What it Does | Why it Matters |
|---|---|---|
| Image Layers | Each instruction creates an immutable snapshot | Enables caching and sharing |
| Layer Ordering | Least → most frequently changed | Maximizes cache hits on rebuild |
| RUN chaining | Cleanup in same layer as install | Prevents layer bloat |
| USER | Sets process identity | Running as root is a security risk |
| GROUP | Controls file access | Principle of least privilege |
| WORKDIR | Sets working directory | Clean, explicit path management |
| ENTRYPOINT | Fixed executable | Defines what the container IS |
| CMD | Default arguments | Overridable defaults for flexibility |
| Multi-stage | Separate build from runtime | Smaller, more secure final images |
| Networking | Service name DNS resolution | Containers talk without hardcoded IPs |
| cgroups | Resource limits | Prevents one container starving the host |
