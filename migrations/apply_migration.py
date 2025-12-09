#!/usr/bin/env python3
import subprocess
import sys

# SQL Server connection details
SERVER = "localhost,1433"
DATABASE = "APGDb"
USERNAME = "sa"
PASSWORD = "YourStrong@Passw0rd"

# Read SQL file from argument or default
if len(sys.argv) > 1:
    sql_file = sys.argv[1]
else:
    sql_file = "005_AddCalculatorSettings.sql"

print(f"Applying migration: {sql_file}")
print(f"Target: {SERVER}/{DATABASE}")
print()

# Read the SQL content
with open(sql_file, 'r') as f:
    sql_content = f.read()

# Split by GO statements
sql_batches = [batch.strip() for batch in sql_content.split('GO') if batch.strip()]

print(f"Found {len(sql_batches)} SQL batches to execute\n")

# Execute using docker exec to sqlserver container
for i, batch in enumerate(sql_batches, 1):
    print(f"Executing batch {i}/{len(sql_batches)}...")
    
    # Create a temp file with the batch
    temp_sql = f"/tmp/batch_{i}.sql"
    with open(temp_sql, 'w') as f:
        f.write(batch)
    
    # Copy to container and execute
    subprocess.run([
        "docker", "cp", temp_sql, "apg-sqlserver:/tmp/batch.sql"
    ], check=True, capture_output=True)
    
    result = subprocess.run([
        "docker", "exec", "apg-sqlserver",
        "/opt/mssql-tools18/bin/sqlcmd",
        "-S", "localhost",
        "-U", USERNAME,
        "-P", PASSWORD,
        "-d", DATABASE,
        "-i", "/tmp/batch.sql",
        "-C"  # Trust server certificate
    ], capture_output=True, text=True)
    
    if result.returncode == 0:
        print(f"✓ Batch {i} executed successfully")
        if result.stdout:
            print(f"  Output: {result.stdout.strip()}")
    else:
        print(f"✗ Batch {i} failed!")
        print(f"  Error: {result.stderr}")
        sys.exit(1)

print("\n✅ All batches executed successfully!")
print("Migration applied.")
