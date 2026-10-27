#!/bin/bash
set -euo pipefail
umask 077
test -s /run/secrets/database-password
chmod 0700 "$PGDATA"
if [[ ! -s "$PGDATA/PG_VERSION" ]]; then
    initdb --username=talkwithtiles_bootstrap --auth-local=trust --auth-host=scram-sha-256
fi
if [[ "$(cat "$PGDATA/PG_VERSION")" != 18 ]]; then
    echo "Expected PostgreSQL 18; restore using the documented upgrade procedure." >&2
    exit 78
fi
cat >"$PGDATA/pg_hba.conf" <<'HBA'
local all talkwithtiles_bootstrap trust
host talkwithtiles talkwithtiles 0.0.0.0/0 scram-sha-256
host talkwithtiles talkwithtiles ::/0 scram-sha-256
HBA
pg_ctl --pgdata="$PGDATA" --wait \
    --options="-c listen_addresses=127.0.0.1 -c unix_socket_directories=/tmp" start
trap 'pg_ctl --pgdata="$PGDATA" --mode=fast --wait stop' EXIT
psql --host=/tmp --username=talkwithtiles_bootstrap --dbname=postgres --set=ON_ERROR_STOP=1 <<'SQL'
SET log_min_error_statement=panic;
\set app_password `cat /run/secrets/database-password`
SELECT 'CREATE ROLE talkwithtiles LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION'
    WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname='talkwithtiles') \gexec
SELECT format('ALTER ROLE talkwithtiles PASSWORD %L', :'app_password') \gexec
SELECT 'CREATE DATABASE talkwithtiles OWNER talkwithtiles'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname='talkwithtiles') \gexec
SQL
pg_ctl --pgdata="$PGDATA" --mode=fast --wait stop
trap - EXIT
exec postgres -D "$PGDATA" -c listen_addresses='*' -c unix_socket_directories=/tmp \
    -c shared_buffers=128MB -c max_connections=50 -c log_min_error_statement=panic
