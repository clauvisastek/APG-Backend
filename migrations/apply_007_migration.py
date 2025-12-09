#!/usr/bin/env python3
"""
Apply migration 007: Add IsDeleted column to GlobalSalarySettings table
"""

import pyodbc
import sys

# Connection string for SQL Server (Docker instance)
connection_string = (
    "DRIVER={ODBC Driver 18 for SQL Server};"
    "SERVER=localhost,1433;"
    "DATABASE=APGDb;"
    "UID=sa;"
    "PWD=YourStrong@Passw0rd;"
    "TrustServerCertificate=yes;"
)

def apply_migration():
    """Apply the IsDeleted migration"""
    print("Connecting to database...")
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("Reading migration file...")
        with open('007_AddIsDeletedToGlobalSalarySettings.sql', 'r') as f:
            migration_sql = f.read()
        
        print("Executing migration 007_AddIsDeletedToGlobalSalarySettings...")
        
        # Split by GO statements and execute each batch
        batches = [batch.strip() for batch in migration_sql.split('GO') if batch.strip()]
        
        for i, batch in enumerate(batches, 1):
            print(f"Executing batch {i}/{len(batches)}...")
            cursor.execute(batch)
            
            # Fetch any messages
            while cursor.nextset():
                pass
        
        conn.commit()
        print("✅ Migration completed successfully!")
        
        # Verify the column was added
        cursor.execute("""
            SELECT COUNT(*) 
            FROM sys.columns 
            WHERE object_id = OBJECT_ID(N'[dbo].[GlobalSalarySettings]') 
            AND name = 'IsDeleted'
        """)
        
        result = cursor.fetchone()
        if result[0] > 0:
            print("✅ Verified: IsDeleted column exists")
        else:
            print("⚠️  Warning: IsDeleted column not found after migration")
        
        cursor.close()
        conn.close()
        
    except pyodbc.Error as e:
        print(f"❌ Database error: {e}")
        sys.exit(1)
    except FileNotFoundError:
        print("❌ Migration file not found: 007_AddIsDeletedToGlobalSalarySettings.sql")
        sys.exit(1)
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    apply_migration()
