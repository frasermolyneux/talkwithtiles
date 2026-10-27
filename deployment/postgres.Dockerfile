FROM postgres:18-bookworm AS postgres

# Copy the filesystem without inheriting the upstream anonymous VOLUME.
FROM scratch
COPY --from=postgres / /
ENV PATH=/usr/lib/postgresql/18/bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin \
    LANG=en_US.utf8 PGDATA=/var/lib/postgresql/data
COPY --chmod=0555 deployment/postgres-entrypoint.sh /usr/local/bin/talkwithtiles-postgres
USER 999:1654
EXPOSE 5432
HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
    CMD ["pg_isready", "--host=127.0.0.1", "--username=talkwithtiles", "--dbname=talkwithtiles"]
ENTRYPOINT ["/usr/local/bin/talkwithtiles-postgres"]
