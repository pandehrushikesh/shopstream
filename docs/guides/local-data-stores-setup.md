# Local data stores: setup guide (Windows)

Applies to ShopStream development on a Windows 11 machine. Engines per ADR-006: PostgreSQL (Ordering, Inventory, Payment, Keycloak), MongoDB (Catalogue), Redis-compatible store (Basket, cache).

Two routes. **Route A (Docker) is recommended**: one command, same versions as the project files, easy to wipe and rebuild, nothing left on Windows. **Route B (native install)** is for learning how each engine is installed and run as a Windows service.

> These setups are for development only: no TLS, weak passwords, MongoDB without authentication. Never reuse them for anything real.

## Route A: Docker Desktop (recommended)

1. Install Docker Desktop for Windows (WSL 2 backend). Restart if asked. Check: `docker version` and `docker compose version` in PowerShell.
2. Open PowerShell in the repo and start only the data stores:
   ```powershell
   cd C:\Users\Prachi\source\repos\shopstream\deploy
   copy .env.example .env
   docker compose up -d mongo postgres redis
   docker compose ps
   ```
   Wait until all three show `healthy`.
3. Verify each engine (see section "Verify" below). Containers run `mongosh`, `psql` and `redis-cli` for you, so you need no local tools:
   ```powershell
   docker compose exec mongo mongosh --eval "db.adminCommand('ping')"
   docker compose exec postgres psql -U shopstream -c "select version();"
   docker compose exec redis redis-cli ping
   ```
4. Stop: `docker compose stop`. Remove containers but keep data: `docker compose down`. Wipe everything including data: `docker compose down -v`.

Image versions: MongoDB 8.0 (supported to Oct 2029; 7.0 ends Aug 2027; 9.0 was released Sept 2026 and is also available), PostgreSQL 16, Redis 7.

## Route B: native install

### B1. MongoDB Community (steps from the MongoDB 9.0 documentation)

Supported: Windows 11 and Windows Server 2022, 64-bit. Not supported on WSL.

1. Download the **msi** from https://www.mongodb.com/try/download/community (Platform: Windows, Package: msi). Choose version 8.0 to match Route A, or 9.0 for the newest.
2. Run the installer. Choose **Complete**.
3. Service configuration: tick **Install MongoD as a Service**, run as **Network Service user**, service name `MongoDB`, accept the default data and log folders.
4. Keep **Install MongoDB Compass** ticked (graphical browser for collections).
5. The service starts automatically. Check in `services.msc` that **MongoDB Server** is Running, or in an Administrator prompt: `net start MongoDB`.
6. Install **mongosh** separately (the msi does not include it): https://www.mongodb.com/docs/mongodb-shell/install/ . Tick "add to PATH".
7. Open a new terminal and run `mongosh`. Expect a prompt like `test>`.
8. Configuration file: `C:\Program Files\MongoDB\Server\<version>\bin\mongod.cfg`. Stop the service before editing, start it afterwards. Defaults are right for us: port 27017, bound to 127.0.0.1 (this machine only).
9. Authentication is off by default. That is acceptable for local development because of the localhost binding. Do not change `bindIp` to a public address.

### B2. PostgreSQL (general knowledge, not re-fetched; check the installer screens)

1. Download the Windows installer from https://www.postgresql.org/download/windows/ (EDB installer). Version 16 matches Route A (17 also works).
2. Run it with all components ticked (server, pgAdmin, command line tools).
3. Set a password for the `postgres` superuser. Write it down. Keep port **5432** and the default locale.
4. Verify: open "SQL Shell (psql)" from the Start menu, press Enter through the defaults, enter the password, run `select version();`.
5. Create the ShopStream role and databases:
   ```sql
   create role shopstream login password 'shopstream_dev';
   create database ordering owner shopstream;
   create database inventory owner shopstream;
   create database payment owner shopstream;
   create database keycloak owner shopstream;
   ```
   (Per C1, each service owns its own database. They share one server only in development.)

### B3. Redis

Redis has no supported native Windows build. Use Docker for this one even on Route B (`docker compose up -d redis`). Alternatives: run it inside WSL 2, or a Redis-compatible Windows port such as Memurai (check its licence first).

## Verify

| Engine | Check | Expected |
|---|---|---|
| MongoDB | `mongosh` then `db.adminCommand('ping')` | `{ ok: 1 }` |
| MongoDB | Compass: connect to `mongodb://localhost:27017` | Databases listed |
| PostgreSQL | `psql -h localhost -U shopstream -d postgres -c "select 1;"` | One row |
| Redis | `redis-cli ping` (or `docker compose exec redis redis-cli ping`) | `PONG` |

## Connection strings (development)

```
MongoDB     mongodb://localhost:27017
PostgreSQL  Host=localhost;Port=5432;Database=ordering;Username=shopstream;Password=shopstream_dev
Redis       localhost:6379
```

Keep passwords out of source control: use `deploy/.env` (ignored by git) and .NET user-secrets.

## Common problems

| Symptom | Likely cause and fix |
|---|---|
| Port 27017, 5432 or 6379 already in use | A native service and a container are both running. Use one route at a time, or stop the service in `services.msc` |
| `docker compose up` says daemon not running | Start Docker Desktop and wait for the whale icon to settle |
| MongoDB service will not start | Check the log in `C:\Program Files\MongoDB\Server\<version>\log\mongod.log`. Often the data folder lacks permission for Network Service |
| `psql` not recognised | Add `C:\Program Files\PostgreSQL\16\bin` to PATH, or use the SQL Shell |
| Windows Firewall prompt for mongod | Allow on Private networks only |

## Source
- [Install MongoDB Community on Windows, MongoDB documentation](https://www.mongodb.com/docs/manual/administration/install-community-windows/)
- [MongoDB support lifecycle](https://www.mongodb.com/legal/support-policy/lifecycles)
