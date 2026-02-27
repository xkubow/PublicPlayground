# Kafka on WSL: Installation and Configuration

## Installing a New Named Distro to WSL

### Creating linux distro for wsl "Ubuntu-Kafka" by Exporting an Installed

To use a custom name and ensure an easy installation, follow these steps:

1. **Install the base distribution**
   ```powershell
   wsl --install -d Ubuntu-22.04
   ```

2. **Shut down the distribution**
   ```powershell
   wsl --terminate Ubuntu-22.04
   ```

3. **Export the distribution to a TAR file**
   ```powershell
   wsl --export Ubuntu-22.04 ubuntu-22.04.tar
   ```

4. **Import it with a custom name**
   ```powershell
   mkdir C:\WSL\Ubuntu-Kafka
   wsl --import Ubuntu-Kafka C:\WSL\Ubuntu-Kafka ubuntu-22.04.tar
   ```
   You now have a clean Ubuntu clone named **Ubuntu-Kafka** without having to search for specific URLs.


5. **(Optional) Unregister the original distribution if it is no longer needed:**
   ```powershell
   wsl --unregister Ubuntu-22.04
   ```

6. **Check installed distributions:**
   ```powershell
   wsl -l
   ```

---

## Kafka Docker Compose Configuration

The `docker-compose.yml` file is already created in the `Kafka/` directory. You can use it to start the Kafka and Zookeeper services.

```yaml
services:
  zookeeper:
    image: confluentinc/cp-zookeeper:7.6.1
    restart: unless-stopped
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000

  kafka:
    image: confluentinc/cp-kafka:7.6.1
    restart: unless-stopped
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
      - "29092:29092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:29092,PLAINTEXT_HOST://0.0.0.0:9092
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,PLAINTEXT_HOST://localhost:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR: 1
      KAFKA_TRANSACTION_STATE_LOG_MIN_ISR: 1
```

---

## 🚀 Automated Setup (Recommended)

You can use the provided `setup-kafka.sh` script to automate the deployment and startup of the Kafka environment.

### What the script does:
1.  Creates a target directory (default: `~/docker/kafka`).
2.  Copies the `docker-compose.yml` to that directory.
3.  Verifies Docker installation and permissions.
4.  Starts the containers in detached mode.
5.  Waits for Kafka to be fully ready before finishing.

### How to run:
1.  Open your WSL terminal.
2.  Navigate to the project's Kafka directory:
    ```bash
    cd Kafka/
    ```
3.  Make the script executable:
    ```bash
    chmod +x setup-kafka.sh
    ```
4.  Run the script:
    ```bash
    ./setup-kafka.sh
    ```
    *Note: You can optionally provide a different target path as an argument: `./setup-kafka.sh /your/custom/path`*

---

## Setup and Topic Management

### Add user to Docker group
```bash
sudo usermod -aG docker kubo
```

### Start Kafka
```bash
cd ~/docker/kafka
docker compose up -d
```

### Verify running containers
```bash
docker ps --format "table {{.Names}}\t{{.Image}}"
```

### Create a topic
```bash
docker exec -it kafka-kafka-1 kafka-topics --bootstrap-server localhost:9092 --create --topic test-events --partitions 3 --replication-factor 1
```

### List topics
```bash
docker exec -it kafka-kafka-1 kafka-topics --bootstrap-server localhost:9092 --list
```

### Inspect topic details (Recommended)
```bash
docker exec -it kafka-kafka-1 kafka-topics --bootstrap-server localhost:9092 --describe --topic test-events
```

---

## Messaging

### Registering a consumer
```bash
docker exec -it kafka-kafka-1 kafka-console-consumer --bootstrap-server localhost:9092 --topic test-events --from-beginning
```

### Sending messages (Producer)
```bash
docker exec -it kafka-kafka-1 kafka-console-producer --bootstrap-server localhost:9092 --topic test-events
```
*Type your messages and press Enter. Use `Ctrl+C` to exit.*

### Sending messages with keys
```bash
docker exec -it kafka-kafka-1 kafka-console-producer --bootstrap-server localhost:9092 --topic test-events --property parse.key=true --property key.separator=:
```
*Example: `user1:hello`*

---

## ✅ Summary (Copy-Paste Cheat Sheet)

### Consumer
```bash
docker exec -it kafka-kafka-1 kafka-console-consumer --bootstrap-server localhost:9092 --topic test-events --from-beginning
```

### Producer
```bash
docker exec -it kafka-kafka-1 kafka-console-producer --bootstrap-server localhost:9092 --topic test-events
```

### Topic Retention Settings
To see your topic’s retention settings, run (inside WSL):
```bash
docker exec -it kafka-kafka-1 kafka-configs --bootstrap-server localhost:9092 --entity-type topics --entity-name test-events --describe
```

### Broker Defaults
To check broker defaults:
```bash
docker exec -it kafka-kafka-1 bash -lc "grep -E 'retention|log.retention' /etc/kafka/server.properties || true"
```

---

## Restart Scenarios

### What happens when you restart WSL
When you start WSL again:
```powershell
wsl -d Ubuntu-Kafka
```

1. WSL starts.
2. Docker daemon reconnects.
3. Containers are still there, but their state depends on the **restart policy**.

#### Case 1: Containers restart automatically (Best Case)
If your containers have a restart policy:
```yaml
restart: unless-stopped
```
Then:
- Kafka starts automatically.
- Topics and messages are preserved.
- You can immediately connect from C#.
- **No action needed ✅**

#### Case 2: Containers are stopped (Manual Restart)
If no restart policy is set (default):
After WSL restart, running `docker ps` may show no running containers.

To start them:
```bash
cd ~/docker/kafka
docker compose up -d
```
