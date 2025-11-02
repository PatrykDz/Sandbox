# Databricks Data Engineer Associate Certification Study Guide

## Table of Contents
1. [Databricks Lakehouse Platform](#databricks-lakehouse-platform)
2. [Apache Spark Fundamentals](#apache-spark-fundamentals)
3. [Delta Lake](#delta-lake)
4. [Spark SQL and DataFrames](#spark-sql-and-dataframes)
5. [ETL with Spark](#etl-with-spark)
6. [Delta Live Tables (DLT)](#delta-live-tables-dlt)
7. [Databricks Workflows and Jobs](#databricks-workflows-and-jobs)
8. [Incremental Data Processing](#incremental-data-processing)
9. [Production Pipelines](#production-pipelines)
10. [Data Governance and Security](#data-governance-and-security)
11. [Performance Optimization](#performance-optimization)
12. [Structured Streaming](#structured-streaming)

---

## Databricks Lakehouse Platform

### Core Concepts
- **Lakehouse** = Data Lake + Data Warehouse
  - Combines low-cost storage (data lake) with ACID transactions and schema enforcement (data warehouse)
  - Single platform for all data workloads (BI, ML, streaming, batch)

### Key Components
- **Databricks Workspace**: Web-based interface for collaboration
- **Clusters**: Computation resources (driver + workers)
- **Notebooks**: Interactive development environment
- **Jobs**: Scheduled production workloads
- **DBFS (Databricks File System)**: Abstraction layer over cloud storage

### Cluster Types
- **All-Purpose Clusters**: Interactive development, manually terminated
- **Job Clusters**: Automated jobs, automatically terminated after job completion
- **Pools**: Pre-configured VMs for faster cluster startup

### Cluster Modes
- **Standard**: Multi-user, supports Python, SQL, R, Scala
- **High Concurrency**: Multi-user with fault isolation
- **Single Node**: Driver only, no workers (for lightweight processing)

---

## Apache Spark Fundamentals

### Architecture
- **Driver**: Orchestrates execution, maintains SparkContext
- **Executors**: Execute tasks, store data for processing
- **Cluster Manager**: Allocates resources

### Core Abstractions
- **RDD (Resilient Distributed Dataset)**: Low-level, immutable distributed collection
- **DataFrame**: Distributed collection with named columns (most common)
- **Dataset**: Type-safe DataFrame (Scala/Java only)

### Execution Model
- **Transformation**: Lazy operation (map, filter, select, join)
- **Action**: Triggers computation (count, collect, show, write)
- **DAG (Directed Acyclic Graph)**: Logical execution plan
- **Stage**: Set of tasks that can run in parallel
- **Task**: Unit of work sent to executor

### Partitions
- Unit of parallelism in Spark
- Each partition processed by one task
- Default: 200 partitions for shuffles
- Control with `repartition()` or `coalesce()`

---

## Delta Lake

### What is Delta Lake?
- Open-source storage layer providing ACID transactions
- Built on top of Parquet files
- Transaction log (`_delta_log/`) tracks all changes

### Key Features
1. **ACID Transactions**: Multiple concurrent reads/writes
2. **Schema Enforcement**: Prevents bad data from being written
3. **Schema Evolution**: Safely add new columns
4. **Time Travel**: Query historical versions of data
5. **Upserts/Deletes**: MERGE, UPDATE, DELETE operations
6. **Unified Batch and Streaming**: Same table for both

### Creating Delta Tables

```sql
-- SQL
CREATE TABLE students (id INT, name STRING, age INT)
USING DELTA
LOCATION '/path/to/table';

-- Or
CREATE TABLE students (id INT, name STRING, age INT);
```

```python
# Python
df.write.format("delta").mode("overwrite").save("/path/to/table")
df.write.format("delta").saveAsTable("students")
```

### Time Travel

```sql
-- Query by version
SELECT * FROM students VERSION AS OF 5;
SELECT * FROM students@v5;

-- Query by timestamp
SELECT * FROM students TIMESTAMP AS OF '2024-01-01';
```

```python
# Python
df = spark.read.format("delta").option("versionAsOf", 5).load("/path")
df = spark.read.format("delta").option("timestampAsOf", "2024-01-01").load("/path")
```

### MERGE (Upsert)

```sql
MERGE INTO target
USING source
ON target.id = source.id
WHEN MATCHED THEN UPDATE SET *
WHEN NOT MATCHED THEN INSERT *;
```

### Optimization Commands

```sql
-- Optimize small files
OPTIMIZE students;

-- Z-ordering (co-locate related data)
OPTIMIZE students ZORDER BY (age);

-- Vacuum old files (default 7 days retention)
VACUUM students;
VACUUM students RETAIN 168 HOURS;
```

### Delta Lake Constraints

```sql
-- NOT NULL
ALTER TABLE students CHANGE COLUMN age SET NOT NULL;

-- CHECK constraints
ALTER TABLE students ADD CONSTRAINT valid_age CHECK (age > 0);
```

---

## Spark SQL and DataFrames

### Reading Data

```python
# CSV
df = spark.read.csv("path/to/file.csv", header=True, inferSchema=True)

# JSON
df = spark.read.json("path/to/file.json")

# Parquet
df = spark.read.parquet("path/to/file.parquet")

# Delta
df = spark.read.format("delta").load("/path/to/delta/table")
df = spark.table("table_name")
```

### Schema Definition

```python
from pyspark.sql.types import StructType, StructField, StringType, IntegerType

schema = StructType([
    StructField("id", IntegerType(), True),
    StructField("name", StringType(), True),
    StructField("age", IntegerType(), True)
])

df = spark.read.schema(schema).csv("path/to/file.csv")
```

### Common DataFrame Operations

```python
# Select columns
df.select("name", "age")
df.selectExpr("name", "age + 1 as age_next_year")

# Filter
df.filter(df.age > 18)
df.where("age > 18")

# GroupBy
df.groupBy("age").count()
df.groupBy("age").agg({"salary": "avg"})

# Join
df1.join(df2, df1.id == df2.id, "inner")
# Join types: inner, outer, left, right, left_semi, left_anti, cross

# Union
df1.union(df2)  # Must have same schema
df1.unionByName(df2)  # Match by column name

# Distinct
df.distinct()
df.dropDuplicates(["id"])

# Sort
df.orderBy("age")
df.sort(df.age.desc())
```

### Built-in Functions

```python
from pyspark.sql.functions import col, lit, concat, when, current_date, datediff

df.select(
    col("name"),
    lit("USA").alias("country"),
    concat(col("first_name"), lit(" "), col("last_name")).alias("full_name"),
    when(col("age") >= 18, "Adult").otherwise("Minor").alias("category"),
    current_date().alias("today"),
    datediff(current_date(), col("birth_date")).alias("days_old")
)
```

### Window Functions

```python
from pyspark.sql.window import Window
from pyspark.sql.functions import row_number, rank, dense_rank, lag, lead

windowSpec = Window.partitionBy("department").orderBy("salary")

df.select(
    "*",
    row_number().over(windowSpec).alias("row_num"),
    rank().over(windowSpec).alias("rank"),
    lag("salary", 1).over(windowSpec).alias("prev_salary")
)
```

### Writing Data

```python
# Modes: overwrite, append, ignore, error (default)
df.write.mode("overwrite").parquet("path/to/output")

# Partitioning
df.write.partitionBy("year", "month").parquet("path/to/output")

# Delta table
df.write.format("delta").mode("append").saveAsTable("table_name")
```

---

## ETL with Spark

### Extract, Transform, Load Pattern

```python
# Extract
source_df = spark.read.format("json").load("/source/path")

# Transform
transformed_df = (source_df
    .filter(col("status") == "active")
    .withColumn("processed_date", current_date())
    .select("id", "name", "processed_date")
)

# Load
transformed_df.write.format("delta").mode("append").save("/target/path")
```

### ELT Pattern
- Load raw data first (Extract & Load)
- Transform within the lakehouse (Transform)
- Better for big data scenarios

### Multi-hop Architecture (Medallion)

```
Bronze (Raw) → Silver (Cleaned) → Gold (Aggregated)
```

- **Bronze**: Raw ingestion, minimal transformation
- **Silver**: Cleaned, validated, deduplicated
- **Gold**: Business-level aggregations for analytics

### Working with Dates

```python
from pyspark.sql.functions import to_date, date_format, year, month, dayofmonth

df.select(
    to_date(col("date_string"), "yyyy-MM-dd").alias("date"),
    date_format(col("timestamp"), "yyyy-MM-dd").alias("formatted"),
    year(col("date")).alias("year"),
    month(col("date")).alias("month")
)
```

### Handling Null Values

```python
# Drop rows with nulls
df.dropna()
df.dropna(subset=["name", "age"])

# Fill nulls
df.fillna({"age": 0, "name": "Unknown"})

# Filter nulls
df.filter(col("name").isNotNull())
```

---

## Delta Live Tables (DLT)

### What is DLT?
- Declarative framework for building reliable data pipelines
- Automatic dependency management
- Built-in quality controls and monitoring
- Auto-scaling and error handling

### Table Types

1. **Streaming Live Table**: For streaming data
2. **Live Table**: For batch processing
3. **View**: Temporary, not persisted

### Creating DLT Pipelines

```python
import dlt
from pyspark.sql.functions import col

# Bronze layer - streaming raw data
@dlt.table(
    comment="Raw sensor data",
    table_properties={"quality": "bronze"}
)
def sensor_raw():
    return spark.readStream.format("cloudFiles") \
        .option("cloudFiles.format", "json") \
        .load("/source/sensors/")

# Silver layer - cleaned data
@dlt.table(
    comment="Cleaned sensor data",
    table_properties={"quality": "silver"}
)
@dlt.expect_or_drop("valid_id", "id IS NOT NULL")
@dlt.expect_or_fail("valid_reading", "reading >= 0")
def sensor_cleaned():
    return dlt.read_stream("sensor_raw") \
        .filter(col("status") == "active") \
        .select("id", "reading", "timestamp")

# Gold layer - aggregated
@dlt.table(
    comment="Hourly sensor averages",
    table_properties={"quality": "gold"}
)
def sensor_hourly():
    return dlt.read("sensor_cleaned") \
        .groupBy("id", window("timestamp", "1 hour")) \
        .agg({"reading": "avg"})
```

### Data Quality Expectations

```python
# Drop records that violate
@dlt.expect_or_drop("valid_email", "email IS NOT NULL")

# Fail pipeline if violated
@dlt.expect_or_fail("valid_id", "id > 0")

# Track violations but keep records
@dlt.expect("valid_name", "name IS NOT NULL")

# Multiple expectations
@dlt.expect_all({
    "valid_id": "id IS NOT NULL",
    "valid_age": "age >= 0 AND age <= 150"
})
```

### DLT Pipeline Configuration

```python
# Development mode: faster, less expensive
# Production mode: fault tolerance, error handling

{
  "clusters": [{
    "label": "default",
    "num_workers": 2
  }],
  "development": true,  # or false for production
  "continuous": false,  # or true for always-on streaming
  "target": "database_name",
  "storage": "/path/to/storage"
}
```

---

## Databricks Workflows and Jobs

### Jobs
- Orchestrate tasks (notebooks, JARs, Python scripts)
- Schedule or trigger manually
- Monitor execution and retries

### Task Dependencies
- Linear: Task B runs after Task A
- Fan-out: Multiple tasks run in parallel
- Fan-in: Multiple tasks converge to one

### Job Configuration
- **Cluster**: New or existing
- **Schedule**: Cron or manual
- **Retries**: Automatic retry on failure
- **Notifications**: Email/webhook on success/failure
- **Parameters**: Pass parameters to notebooks

### Creating Jobs (UI)
1. Workflows → Create Job
2. Add tasks (notebook, Python, JAR, SQL)
3. Configure cluster
4. Set schedule
5. Configure alerts

### Running Jobs Programmatically

```python
# Using Databricks CLI
databricks jobs create --json-file job-config.json
databricks jobs run-now --job-id 123
```

---

## Incremental Data Processing

### Why Incremental?
- Process only new/changed data
- Reduce processing time and cost
- Enable near real-time analytics

### Techniques

#### 1. Using Timestamps

```python
# Get last processed timestamp
last_timestamp = spark.sql("SELECT MAX(processed_at) FROM target_table").collect()[0][0]

# Read only new data
new_data = spark.read.parquet("/source") \
    .filter(col("created_at") > last_timestamp)

# Process and write
new_data.write.mode("append").save("/target")
```

#### 2. Using Auto Loader (Cloud Files)

```python
# Auto Loader automatically tracks processed files
df = spark.readStream.format("cloudFiles") \
    .option("cloudFiles.format", "json") \
    .option("cloudFiles.schemaLocation", "/schema/location") \
    .load("/source/path")

df.writeStream \
    .format("delta") \
    .option("checkpointLocation", "/checkpoint/path") \
    .table("target_table")
```

#### 3. Using Change Data Feed (CDF)

```sql
-- Enable CDF on table
CREATE TABLE students (id INT, name STRING)
TBLPROPERTIES (delta.enableChangeDataFeed = true);

-- Read changes
SELECT * FROM table_changes('students', 2)  -- From version 2
SELECT * FROM table_changes('students', '2024-01-01')  -- From timestamp
```

```python
# Python
changes = spark.read.format("delta") \
    .option("readChangeDataFeed", "true") \
    .option("startingVersion", 2) \
    .table("students")
```

---

## Production Pipelines

### Best Practices

1. **Idempotency**: Same input produces same output
   - Use `MERGE` instead of `INSERT`
   - Use `overwrite` with partition filters

2. **Error Handling**: Graceful failure recovery
   - Use try-except in Python
   - Configure job retries
   - Dead letter queues for bad records

3. **Monitoring**: Track pipeline health
   - DLT event logs
   - Job run metrics
   - Data quality metrics

4. **Testing**: Validate before production
   - Unit tests for transformations
   - Integration tests for pipelines
   - Development/staging environments

5. **Checkpoint Locations**: For streaming
   - Store processing state
   - Enable exactly-once semantics
   - Stored in DBFS or cloud storage

### Example Production Pattern

```python
from pyspark.sql.functions import current_timestamp

try:
    # Read with schema enforcement
    df = spark.read \
        .schema(predefined_schema) \
        .option("mode", "FAILFAST") \
        .parquet("/source/path")

    # Add metadata
    df_with_metadata = df.withColumn("processed_at", current_timestamp())

    # Write with idempotency
    df_with_metadata.write \
        .format("delta") \
        .mode("overwrite") \
        .option("replaceWhere", "date = current_date()") \
        .saveAsTable("target_table")

    # Log success
    print(f"Processed {df.count()} records successfully")

except Exception as e:
    # Log error
    print(f"Pipeline failed: {str(e)}")
    # Write to error table
    error_df = spark.createDataFrame([(str(e), current_timestamp())])
    error_df.write.mode("append").saveAsTable("error_log")
    raise
```

---

## Data Governance and Security

### Unity Catalog
- Unified governance for data and AI assets
- Fine-grained access control
- Data lineage and audit logs
- Cross-cloud data sharing

### Three-Level Namespace
```
catalog.schema.table
```

- **Catalog**: Top-level container
- **Schema (Database)**: Logical grouping
- **Table/View**: Data object

### Managing Objects

```sql
-- Create catalog
CREATE CATALOG IF NOT EXISTS production;

-- Create schema
CREATE SCHEMA IF NOT EXISTS production.sales;

-- Create table
CREATE TABLE production.sales.orders (
    order_id INT,
    customer_id INT,
    amount DECIMAL(10,2)
) USING DELTA;
```

### Access Control (GRANT/REVOKE)

```sql
-- Grant privileges
GRANT SELECT ON TABLE production.sales.orders TO user@company.com;
GRANT USAGE ON SCHEMA production.sales TO data_analysts;
GRANT CREATE TABLE ON SCHEMA production.sales TO data_engineers;

-- Revoke privileges
REVOKE SELECT ON TABLE production.sales.orders FROM user@company.com;

-- View grants
SHOW GRANTS ON TABLE production.sales.orders;
```

### Privilege Types
- **SELECT**: Read data
- **INSERT**: Insert data
- **UPDATE**: Modify data
- **DELETE**: Delete data
- **CREATE**: Create objects
- **USAGE**: Use catalog/schema
- **ALL PRIVILEGES**: All permissions

### Views for Security

```sql
-- Create view with filtered data
CREATE VIEW sales_filtered AS
SELECT order_id, amount
FROM sales.orders
WHERE region = 'US';

-- Grant access to view instead of table
GRANT SELECT ON VIEW sales_filtered TO analysts;
```

### Dynamic Views (Row/Column Level Security)

```sql
CREATE VIEW customer_data_filtered AS
SELECT
    customer_id,
    name,
    CASE
        WHEN is_member('managers') THEN ssn
        ELSE 'REDACTED'
    END AS ssn
FROM customers
WHERE is_member('sales') OR region = current_user();
```

---

## Performance Optimization

### 1. Partition Pruning
- Filter on partition columns to skip reading irrelevant partitions

```python
# Write with partitioning
df.write.partitionBy("year", "month").parquet("/path")

# Read with filter on partition columns
df = spark.read.parquet("/path").filter("year = 2024 AND month = 1")
```

### 2. Predicate Pushdown
- Filters applied at source, reducing data read

```python
# Good - filter pushed to source
df = spark.read.parquet("/path").filter("age > 18")

# Less efficient - filter after reading
df = spark.read.parquet("/path")
df = df.filter("age > 18")
```

### 3. Column Pruning
- Read only necessary columns

```python
# Good - read only needed columns
df = spark.read.parquet("/path").select("id", "name")

# Bad - read all columns
df = spark.read.parquet("/path")
```

### 4. Broadcast Joins
- Small table broadcasted to all executors

```python
from pyspark.sql.functions import broadcast

# Explicitly broadcast small table
large_df.join(broadcast(small_df), "key")
```

### 5. Caching
- Store frequently used DataFrames in memory

```python
df.cache()  # or df.persist()
df.count()  # Trigger caching

# Use cached DataFrame multiple times
result1 = df.filter("age > 18").count()
result2 = df.filter("age < 30").count()

df.unpersist()  # Release memory
```

### 6. Repartitioning
- Control parallelism

```python
# Increase partitions (with shuffle)
df.repartition(100)
df.repartition("column_name")  # Hash partitioning

# Decrease partitions (no shuffle)
df.coalesce(10)
```

### 7. Adaptive Query Execution (AQE)
- Enabled by default in Databricks
- Dynamically optimizes execution plan
- Automatically coalesces partitions, handles skew

```python
spark.conf.set("spark.sql.adaptive.enabled", "true")
```

### 8. Z-Ordering (Delta Lake)
- Co-locate related data in same files

```sql
OPTIMIZE table_name ZORDER BY (column1, column2);
```

### 9. File Compaction

```sql
-- Combine small files into larger ones
OPTIMIZE table_name;
```

### 10. Avoiding Shuffles
- Minimize wide transformations (join, groupBy, repartition)
- Use `reduceByKey()` instead of `groupByKey()`

---

## Structured Streaming

### Core Concepts
- **Input Source**: Where streaming data comes from
- **Streaming DataFrame**: Unbounded table of data
- **Output Sink**: Where results are written
- **Trigger**: When to process data
- **Checkpoint**: Stores processing state

### Reading Streams

```python
# From files (Auto Loader)
df = spark.readStream.format("cloudFiles") \
    .option("cloudFiles.format", "json") \
    .load("/source/path")

# From Delta table
df = spark.readStream.format("delta").table("source_table")

# From Kafka
df = spark.readStream.format("kafka") \
    .option("kafka.bootstrap.servers", "host:port") \
    .option("subscribe", "topic") \
    .load()
```

### Transformations
- Same as batch DataFrames
- Some operations not supported (sorting entire stream, multiple aggregations)

```python
streaming_df = df.filter(col("status") == "active") \
    .select("id", "value", "timestamp") \
    .withWatermark("timestamp", "10 minutes") \
    .groupBy(window("timestamp", "5 minutes"), "id") \
    .count()
```

### Writing Streams

```python
query = df.writeStream \
    .format("delta") \
    .outputMode("append") \
    .option("checkpointLocation", "/checkpoint/path") \
    .table("target_table")

# Or to path
query = df.writeStream \
    .format("delta") \
    .outputMode("append") \
    .option("checkpointLocation", "/checkpoint/path") \
    .start("/target/path")
```

### Output Modes
- **Append**: Only new rows (default)
- **Complete**: Entire result table (only for aggregations)
- **Update**: Only updated rows

### Triggers
```python
# Default: micro-batches as soon as possible
.trigger(processingTime="10 seconds")  # Every 10 seconds
.trigger(once=True)  # Single micro-batch (for testing)
.trigger(availableNow=True)  # Process all available data then stop
```

### Watermarking
- Handle late data in event-time processing

```python
df.withWatermark("event_time", "10 minutes") \
    .groupBy(window("event_time", "5 minutes")) \
    .count()
```

### Managing Streams

```python
# Start stream
query = df.writeStream.start()

# Check status
query.status
query.recentProgress

# Stop stream
query.stop()

# Wait for termination
query.awaitTermination()
```

### Stream-Stream Joins

```python
# Both streams need watermarks
stream1.withWatermark("timestamp", "10 minutes")
stream2.withWatermark("timestamp", "20 minutes")

joined = stream1.join(stream2, "id")
```

### Deduplication

```python
# Dedup within watermark window
df.withWatermark("timestamp", "10 minutes") \
    .dropDuplicates(["id", "timestamp"])
```

---

## Key SQL Commands Summary

### Database/Schema Operations
```sql
CREATE DATABASE IF NOT EXISTS db_name;
USE db_name;
SHOW DATABASES;
DESCRIBE DATABASE db_name;
DROP DATABASE db_name;
```

### Table Operations
```sql
-- Create
CREATE TABLE table_name (col1 INT, col2 STRING) USING DELTA;
CREATE TABLE table_name AS SELECT * FROM source;
CREATE OR REPLACE TABLE table_name AS SELECT ...;

-- Modify
ALTER TABLE table_name ADD COLUMNS (new_col STRING);
ALTER TABLE table_name RENAME COLUMN old_name TO new_name;
ALTER TABLE table_name DROP COLUMN col_name;

-- Info
DESCRIBE TABLE table_name;
DESCRIBE EXTENDED table_name;
SHOW TABLES;
SHOW CREATE TABLE table_name;

-- Drop
DROP TABLE table_name;
```

### Data Manipulation
```sql
-- Insert
INSERT INTO table_name VALUES (1, 'a'), (2, 'b');
INSERT INTO table_name SELECT * FROM source;
INSERT OVERWRITE table_name SELECT * FROM source;

-- Update
UPDATE table_name SET col1 = value WHERE condition;

-- Delete
DELETE FROM table_name WHERE condition;

-- Merge
MERGE INTO target USING source ON condition
WHEN MATCHED THEN UPDATE SET *
WHEN NOT MATCHED THEN INSERT *;
```

### Views
```sql
CREATE VIEW view_name AS SELECT * FROM table_name WHERE condition;
CREATE OR REPLACE TEMP VIEW temp_view AS SELECT ...;
DROP VIEW view_name;
```

---

## Quick Reference: PySpark vs SQL

| Task | PySpark | SQL |
|------|---------|-----|
| Read Delta | `spark.table("table")` | `SELECT * FROM table` |
| Filter | `df.filter("age > 18")` | `WHERE age > 18` |
| Select | `df.select("name", "age")` | `SELECT name, age FROM table` |
| GroupBy | `df.groupBy("dept").count()` | `GROUP BY dept` |
| Join | `df1.join(df2, "id")` | `JOIN table2 ON table1.id = table2.id` |
| Write | `df.write.saveAsTable("table")` | `CREATE TABLE AS SELECT` |

---

## Exam Tips

### Key Focus Areas (by weight)
1. **Databricks Lakehouse Platform** (~20%)
   - Cluster configuration
   - DBFS and storage

2. **Delta Lake** (~25%)
   - ACID transactions
   - Time Travel
   - Optimize/Vacuum
   - MERGE operations

3. **ETL with Spark SQL** (~25%)
   - DataFrame operations
   - Reading/writing data
   - Transformations

4. **Incremental Data Processing** (~20%)
   - Structured Streaming
   - Auto Loader
   - Change Data Feed

5. **Production Pipelines** (~10%)
   - Delta Live Tables
   - Jobs and workflows
   - Error handling

### Common Gotchas
1. **Lazy Evaluation**: Transformations don't execute until action
2. **VACUUM**: Default retention is 7 days
3. **Time Travel**: Doesn't work after VACUUM removes old files
4. **Partitioning**: Over-partitioning creates small files
5. **Caching**: Must trigger with action (count, show)
6. **Checkpoint**: Required for structured streaming production
7. **Z-ORDER**: Use on commonly filtered columns, limit to 3-4 columns
8. **MERGE**: Can do INSERT, UPDATE, DELETE in one operation

### Important Commands to Remember
```sql
-- Delta Lake
OPTIMIZE table ZORDER BY (col);
VACUUM table RETAIN 168 HOURS;
DESCRIBE HISTORY table;

-- Constraints
ALTER TABLE table ADD CONSTRAINT name CHECK (condition);

-- Unity Catalog
GRANT SELECT ON TABLE catalog.schema.table TO user;
```

### Best Practices
1. Use Delta Lake for all tables
2. Partition large tables by commonly filtered columns
3. Use Auto Loader for incremental file ingestion
4. Always specify checkpoint locations for streaming
5. Use DLT for complex multi-hop pipelines
6. Implement data quality checks
7. Use Z-ORDER for frequently queried columns
8. Regular OPTIMIZE to compact small files
9. Set appropriate retention before VACUUM
10. Use Unity Catalog for governance

---

## Practice Questions

### Delta Lake
1. What is the default retention period for VACUUM? **7 days**
2. Can you query a Delta table after vacuuming old versions? **Only if within retention**
3. What does OPTIMIZE do? **Compacts small files**
4. What is Z-ORDERING used for? **Co-locating related data**

### Structured Streaming
1. What is a checkpoint location? **Stores stream processing state**
2. What are the three output modes? **Append, Complete, Update**
3. What does watermarking do? **Handles late-arriving data**

### Performance
1. How do you broadcast a small table? **broadcast(df)**
2. What's the difference between repartition and coalesce? **Repartition shuffles, coalesce doesn't**
3. What is predicate pushdown? **Filtering at source**

### DLT
1. What are the three expectation actions? **expect, expect_or_drop, expect_or_fail**
2. What's the difference between live table and streaming live table? **Batch vs streaming**
3. Where are DLT quality metrics stored? **Event log**

---

## Final Checklist

Before the exam, ensure you understand:
- [ ] Creating and managing Delta tables
- [ ] Time Travel syntax (VERSION AS OF, TIMESTAMP AS OF)
- [ ] MERGE, OPTIMIZE, VACUUM commands
- [ ] Reading different file formats (CSV, JSON, Parquet, Delta)
- [ ] DataFrame transformations (filter, select, groupBy, join)
- [ ] Structured Streaming basics (readStream, writeStream, checkpoint)
- [ ] Auto Loader (cloudFiles) syntax
- [ ] Delta Live Tables decorators (@dlt.table, expectations)
- [ ] Cluster types and modes
- [ ] Unity Catalog three-level namespace
- [ ] GRANT/REVOKE privileges
- [ ] Partition pruning and performance optimization
- [ ] Window functions and aggregations
- [ ] Handling null values and data quality

---

**Good luck on your Databricks Data Engineer Associate Certification exam!**
