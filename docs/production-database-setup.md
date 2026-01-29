# Production Database Setup Guide

This guide provides SQL Server optimization configurations for production-scale bulk operations and training data generation workloads.

## Overview

NemesisEuchre is designed for large-scale ML training data generation (100K-1M+ games). Production deployments should use SQL Server instead of LocalDB for optimal performance and reliability.

## Database Configuration

### Connection String

For production SQL Server instances, use this connection string format in `appsettings.json`:

```json
"ConnectionStrings": {
  "NemesisEuchreDb": "Server=YOUR_SERVER;Database=NemesisEuchre;User Id=YOUR_USER;Password=YOUR_PASSWORD;Min Pool Size=4;Max Pool Size=64;Pooling=true;Connection Timeout=30;TrustServerCertificate=true"
}
```

**Connection Pool Settings:**
- `Min Pool Size=4`: Maintain 4 warm connections to eliminate connection latency
- `Max Pool Size=64`: Support high parallelism (adjust based on CPU core count)
- `Pooling=true`: Enable connection pooling
- `Connection Timeout=30`: 30-second timeout for connection establishment

### Bulk Load Optimization

**IMPORTANT:** These optimizations should ONLY be applied during bulk data generation operations. They sacrifice durability and consistency checks for maximum write throughput.

#### Before Bulk Load

Execute these commands to optimize for bulk insert performance:

```sql
USE master;
GO

-- Set database to SIMPLE recovery during bulk load
-- WARNING: Minimally logged operations, no point-in-time recovery
ALTER DATABASE NemesisEuchre SET RECOVERY SIMPLE;
GO

-- Disable auto-statistics during bulk load
-- Statistics updates add overhead; rebuild after load completes
ALTER DATABASE NemesisEuchre SET AUTO_CREATE_STATISTICS OFF;
ALTER DATABASE NemesisEuchre SET AUTO_UPDATE_STATISTICS OFF;
GO

-- Ensure database is in SIMPLE recovery before bulk operations
SELECT name, recovery_model_desc
FROM sys.databases
WHERE name = 'NemesisEuchre';
GO
```

#### After Bulk Load

**CRITICAL:** Restore production database settings after bulk operations complete:

```sql
USE master;
GO

-- Restore FULL recovery model for production durability
ALTER DATABASE NemesisEuchre SET RECOVERY FULL;
GO

-- Re-enable auto-statistics
ALTER DATABASE NemesisEuchre SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE NemesisEuchre SET AUTO_UPDATE_STATISTICS ON;
GO

-- Rebuild all statistics for query optimization
USE NemesisEuchre;
GO
EXEC sp_updatestats;
GO

-- Take a full backup to establish recovery chain
BACKUP DATABASE NemesisEuchre
TO DISK = 'C:\Backups\NemesisEuchre_PostBulkLoad.bak'
WITH FORMAT, INIT, COMPRESSION;
GO
```

### SQL Server Configuration

Ensure SQL Server is configured for optimal parallelism and memory usage:

```sql
-- Check current max degree of parallelism (0 = use all processors)
EXEC sp_configure 'max degree of parallelism';
GO

-- Set to 0 for maximum parallelism during bulk operations
EXEC sp_configure 'max degree of parallelism', 0;
RECONFIGURE;
GO

-- Verify minimum server memory (adjust based on available RAM)
EXEC sp_configure 'min server memory (MB)';
GO

-- Example: Set min memory to 2GB for dedicated SQL Server
EXEC sp_configure 'min server memory (MB)', 2048;
RECONFIGURE;
GO
```

### Index Maintenance

After bulk load operations, rebuild indexes to optimize query performance:

```sql
USE NemesisEuchre;
GO

-- Rebuild all indexes with online option (if Enterprise Edition)
ALTER INDEX ALL ON dbo.Games REBUILD WITH (ONLINE = ON);
ALTER INDEX ALL ON dbo.Deals REBUILD WITH (ONLINE = ON);
ALTER INDEX ALL ON dbo.Tricks REBUILD WITH (ONLINE = ON);
ALTER INDEX ALL ON dbo.Decisions REBUILD WITH (ONLINE = ON);
GO

-- For Standard Edition, use offline rebuild:
-- ALTER INDEX ALL ON dbo.Games REBUILD;
-- ALTER INDEX ALL ON dbo.Deals REBUILD;
-- ALTER INDEX ALL ON dbo.Tricks REBUILD;
-- ALTER INDEX ALL ON dbo.Decisions REBUILD;
```

## Performance Monitoring

Monitor database performance during bulk operations:

```sql
-- Check for blocking sessions
SELECT
    session_id,
    blocking_session_id,
    wait_type,
    wait_time,
    wait_resource
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;
GO

-- Monitor transaction log usage
DBCC SQLPERF(LOGSPACE);
GO

-- Check database file sizes
SELECT
    name,
    size * 8 / 1024 AS SizeMB,
    max_size,
    growth
FROM sys.database_files;
GO
```

## Expected Performance

With production SQL Server and bulk insert optimizations:

| Batch Size | LocalDB (Standard) | SQL Server (Optimized) | Speedup |
|------------|-------------------|------------------------|---------|
| 1,000      | 45 seconds        | 5 seconds              | 9x      |
| 10,000     | 7 minutes         | 45 seconds             | 9x      |
| 100,000    | 90 minutes        | 5-8 minutes            | 11-18x  |
| 1,000,000  | N/A (OOM)         | 50-80 minutes          | -       |

**Note:** Performance varies based on:
- CPU core count (parallelism)
- Disk I/O speed (SSD vs HDD)
- Available memory
- Network latency (local vs remote SQL Server)

## Automation Scripts

### Bulk Load Preparation Script

```sql
-- Save as: prepare-bulk-load.sql
USE master;
GO
ALTER DATABASE NemesisEuchre SET RECOVERY SIMPLE;
ALTER DATABASE NemesisEuchre SET AUTO_CREATE_STATISTICS OFF;
ALTER DATABASE NemesisEuchre SET AUTO_UPDATE_STATISTICS OFF;
GO
PRINT 'Database prepared for bulk load operations';
```

### Post-Load Recovery Script

```sql
-- Save as: finalize-bulk-load.sql
USE master;
GO
ALTER DATABASE NemesisEuchre SET RECOVERY FULL;
ALTER DATABASE NemesisEuchre SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE NemesisEuchre SET AUTO_UPDATE_STATISTICS ON;
GO

USE NemesisEuchre;
GO
EXEC sp_updatestats;
GO

BACKUP DATABASE NemesisEuchre
TO DISK = 'C:\Backups\NemesisEuchre_$(TIMESTAMP).bak'
WITH FORMAT, INIT, COMPRESSION;
GO
PRINT 'Database finalized and backed up';
```

## Troubleshooting

### High Transaction Log Growth

If transaction log grows excessively during bulk operations:

```sql
-- Check log space usage
DBCC SQLPERF(LOGSPACE);

-- In SIMPLE recovery, checkpoint to truncate log
CHECKPOINT;

-- If log is still large, shrink it (USE SPARINGLY)
USE NemesisEuchre;
DBCC SHRINKFILE (NemesisEuchre_log, 1);
```

### Connection Pool Exhaustion

If you see "Timeout expired" errors:

1. Increase `Max Pool Size` in connection string (64 → 128)
2. Reduce `MaxDegreeOfParallelism` in `appsettings.json`
3. Increase `Connection Timeout` (30 → 60 seconds)

### Memory Pressure

If SQL Server experiences memory pressure during bulk operations:

```sql
-- Clear procedure cache (forces recompilation)
DBCC FREEPROCCACHE;

-- Clear clean buffers (do NOT use in production under load)
DBCC DROPCLEANBUFFERS;
```

## Security Considerations

1. Use dedicated service accounts with minimum required permissions
2. Encrypt connections with TLS (remove `TrustServerCertificate=true` after cert setup)
3. Store connection strings in secure configuration (Azure Key Vault, etc.)
4. Audit bulk operations with SQL Server Audit
5. Restrict `ALTER DATABASE` permissions to DBA accounts only

## References

- [SQL Server Recovery Models](https://learn.microsoft.com/en-us/sql/relational-databases/backup-restore/recovery-models-sql-server)
- [Connection Pooling](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-connection-pooling)
- [EFCore.BulkExtensions Documentation](https://github.com/borisdj/EFCore.BulkExtensions)
