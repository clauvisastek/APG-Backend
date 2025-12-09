#!/usr/bin/env python3
import subprocess

# SQL Server connection details
DATABASE = "APGDb"
USERNAME = "sa"
PASSWORD = "YourStrong@Passw0rd"

queries = [
    ("GlobalSalarySettings structure", "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GlobalSalarySettings' ORDER BY ORDINAL_POSITION"),
    ("ClientMarginSettings structure", "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMarginSettings' ORDER BY ORDINAL_POSITION"),
    ("GlobalSalarySettings count", "SELECT COUNT(*) as RowCount FROM GlobalSalarySettings"),
    ("ClientMarginSettings count", "SELECT COUNT(*) as RowCount FROM ClientMarginSettings")
]

for name, query in queries:
    print(f"\n{'='*60}")
    print(f"{name}")
    print('='*60)
    
    # Write query to temp file
    with open('/tmp/query.sql', 'w') as f:
        f.write(query)
    
    # Copy to container
    subprocess.run(["docker", "cp", "/tmp/query.sql", "apg-sqlserver:/tmp/query.sql"], 
                   check=True, capture_output=True)
    
    # Execute query
    result = subprocess.run([
        "docker", "exec", "apg-sqlserver",
        "/opt/mssql-tools18/bin/sqlcmd",
        "-S", "localhost",
        "-U", USERNAME,
        "-P", PASSWORD,
        "-d", DATABASE,
        "-i", "/tmp/query.sql",
        "-C",
        "-s", ",",
        "-W"
    ], capture_output=True, text=True)
    
    print(result.stdout)
    if result.stderr and 'Sqlcmd:' not in result.stderr:
        print(f"Warnings: {result.stderr}")
