# SQL Server DDL Extraction Script (SQL Server 2012 Compatible)
param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,
    
    [Parameter(Mandatory=$false)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [string]$Password,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseWindowsAuth = $true
)

# Create the base output directory if it doesn't exist
$baseDir = Join-Path $OutputPath $DatabaseName
New-Item -ItemType Directory -Force -Path $baseDir | Out-Null

# Create subdirectories for different object types
$directories = @('Tables', 'Views', 'StoredProcedures', 'Functions', 'Triggers', 'Schemas')
foreach ($dir in $directories) {
    New-Item -ItemType Directory -Force -Path (Join-Path $baseDir $dir) | Out-Null
}

# Function to write content to file
function Write-ToFile {
    param($Content, $ObjectName, $ObjectType)
    $fileName = [IO.Path]::Combine($baseDir, $ObjectType, "$ObjectName.sql")
    $Content | Out-File -FilePath $fileName -Encoding UTF8
}

# Build connection string based on authentication method
if ($UseWindowsAuth) {
    $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;"
} else {
    if ([string]::IsNullOrEmpty($Username) -or [string]::IsNullOrEmpty($Password)) {
        throw "Username and Password are required when not using Windows Authentication"
    }
    $connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;"
}

# Create SQL connection
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

try {
    # Get SQL Server version
    $versionQuery = "SELECT SERVERPROPERTY('ProductVersion') AS Version"
    $command = New-Object System.Data.SqlClient.SqlCommand($versionQuery, $connection)
    $serverVersion = $command.ExecuteScalar()
    Write-Host "SQL Server Version: $serverVersion"

      # 1. Extract Schemas
    $schemaQuery = @"
    SELECT 
        s.name AS schema_name,
        'CREATE SCHEMA ' + QUOTENAME(s.name) + ' AUTHORIZATION ' + QUOTENAME(dp.name) + ';' AS creation_script
    FROM sys.schemas s
    JOIN sys.database_principals dp ON s.principal_id = dp.principal_id
    WHERE s.name NOT IN ('dbo', 'guest', 'sys', 'INFORMATION_SCHEMA');
"@
    $command = New-Object System.Data.SqlClient.SqlCommand($schemaQuery, $connection)
    $reader = $command.ExecuteReader()
    while ($reader.Read()) {
        Write-ToFile -Content $reader['creation_script'] -ObjectName $reader['schema_name'] -ObjectType 'Schemas'
    }
    $reader.Close()

    # 2. Extract Tables with enhanced constraints and indexes
    $tableQuery = @"
    SELECT 
        s.name AS schema_name,
        t.name AS table_name,
        t.object_id
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.is_ms_shipped = 0
    ORDER BY s.name, t.name;
"@
    $command.CommandText = $tableQuery
    $reader = $command.ExecuteReader()
    $tables = @()
    while ($reader.Read()) {
        $tables += @{
            schema = $reader['schema_name']
            name = $reader['table_name']
            object_id = $reader['object_id']
        }
    }
    $reader.Close()

    foreach ($table in $tables) {
        $script = "USE [$DatabaseName]`nGO`n`n"
        $script += "CREATE TABLE [$($table.schema)].[$($table.name)]`n(`n"

        # Get columns (previous column query remains the same)
        $columnQuery = @"
        SELECT 
            c.name AS column_name,
            t.name AS data_type,
            c.max_length,
            c.precision,
            c.scale,
            c.is_nullable,
            c.is_identity,
            COLUMNPROPERTY(OBJECT_ID('[$($table.schema)].[$($table.name)]'), c.name, 'IsIdentity') as is_identity,
            IDENT_SEED('[$($table.schema)].[$($table.name)]') as identity_seed,
            IDENT_INCR('[$($table.schema)].[$($table.name)]') as identity_increment,
            dc.definition AS default_definition,
            c.collation_name
        FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
        WHERE c.object_id = OBJECT_ID('[$($table.schema)].[$($table.name)]')
        ORDER BY c.column_id;
"@
        $command.CommandText = $columnQuery
        $reader = $command.ExecuteReader()
        $columns = @()
        
        while ($reader.Read()) {
            # [Previous column definition code remains the same]
            $column = "    [$($reader['column_name'])] [$($reader['data_type'])]"
            
            # Add size/precision/scale
            switch ($reader['data_type']) {
                { $_ -in 'char','varchar','nchar','nvarchar' } {
                    if ($reader['max_length'] -eq -1) {
                        $column += "(MAX)"
                    } else {
                        if ($reader['data_type'].StartsWith('n')) {
                            $column += "($($reader['max_length']/2))"
                        } else {
                            $column += "($($reader['max_length']))"
                        }
                    }
                }
                { $_ -in 'decimal','numeric' } {
                    $column += "($($reader['precision']), $($reader['scale']))"
                }
            }
            
            if ($reader['collation_name']) {
                $column += " COLLATE $($reader['collation_name'])"
            }
            
            $column += if ($reader['is_nullable']) { " NULL" } else { " NOT NULL" }
            
            if ($reader['is_identity']) {
                $column += " IDENTITY($($reader['identity_seed']),$($reader['identity_increment']))"
            }
            
            if ($reader['default_definition']) {
                $column += " DEFAULT $($reader['default_definition'])"
            }
            
            $columns += $column
        }
        $reader.Close()
        
        $script += $columns -join ",`n"
        $script += "`n)"

        # Add Primary Key constraints
        $pkQuery = @"
        SELECT 
            i.name AS constraint_name,
            c.name AS column_name
        FROM sys.indexes i
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE i.object_id = $($table.object_id)
        AND i.is_primary_key = 1
        ORDER BY ic.key_ordinal;
"@
        $command.CommandText = $pkQuery
        $reader = $command.ExecuteReader()
        $pkColumns = @()
        $pkName = ""
        while ($reader.Read()) {
            $pkName = $reader['constraint_name']
            $pkColumns += "[$($reader['column_name'])]"
        }
        $reader.Close()

        if ($pkColumns.Count -gt 0) {
            $script += ",`n    CONSTRAINT [$pkName] PRIMARY KEY ($($pkColumns -join ', '))"
        }

        # Add Foreign Key constraints
        $fkQuery = @"
        SELECT 
            fk.name AS constraint_name,
            pc.name AS parent_column,
            rc.name AS referenced_column,
            rs.name AS referenced_schema,
            rt.name AS referenced_table
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        JOIN sys.columns pc ON fkc.parent_object_id = pc.object_id AND fkc.parent_column_id = pc.column_id
        JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
        JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
        JOIN sys.schemas rs ON rt.schema_id = rs.schema_id
        WHERE fk.parent_object_id = $($table.object_id)
        ORDER BY fk.name, fkc.constraint_column_id;
"@
        $command.CommandText = $fkQuery
        $reader = $command.ExecuteReader()
        $fkConstraints = @{}
        while ($reader.Read()) {
            $fkName = $reader['constraint_name']
            if (-not $fkConstraints.ContainsKey($fkName)) {
                $fkConstraints[$fkName] = @{
                    parent_columns = @()
                    referenced_columns = @()
                    referenced_schema = $reader['referenced_schema']
                    referenced_table = $reader['referenced_table']
                }
            }
            $fkConstraints[$fkName].parent_columns += "[$($reader['parent_column'])]"
            $fkConstraints[$fkName].referenced_columns += "[$($reader['referenced_column'])]"
        }
        $reader.Close()

        foreach ($fk in $fkConstraints.GetEnumerator()) {
            $script += ",`n    CONSTRAINT [$($fk.Key)] FOREIGN KEY ($($fk.Value.parent_columns -join ', '))`n"
            $script += "        REFERENCES [$($fk.Value.referenced_schema)].[$($fk.Value.referenced_table)] ($($fk.Value.referenced_columns -join ', '))"
        }

        # Add Unique constraints and indexes
        $uniqueQuery = @"
        SELECT 
            i.name AS constraint_name,
            i.type_desc,
            i.is_unique,
            i.is_primary_key,
            ic.key_ordinal,
            c.name AS column_name,
            ic.is_descending_key
        FROM sys.indexes i
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE i.object_id = $($table.object_id)
        AND i.is_primary_key = 0
        ORDER BY i.name, ic.key_ordinal;
"@
        $command.CommandText = $uniqueQuery
        $reader = $command.ExecuteReader()
        $indexes = @{}
        while ($reader.Read()) {
            $idxName = $reader['constraint_name']
            if (-not $indexes.ContainsKey($idxName)) {
                $indexes[$idxName] = @{
                    columns = @()
                    is_unique = $reader['is_unique']
                    type_desc = $reader['type_desc']
                }
            }
            $direction = if ($reader['is_descending_key']) { " DESC" } else { " ASC" }
            $indexes[$idxName].columns += "[$($reader['column_name'])]$direction"
        }
        $reader.Close()

        $script += "`n)`nGO`n"

        # Add non-clustered indexes after table creation
        foreach ($idx in $indexes.GetEnumerator()) {
            if ($idx.Value.type_desc -eq 'NONCLUSTERED') {
                $script += "`nCREATE"
                if ($idx.Value.is_unique) { $script += " UNIQUE" }
                $script += " NONCLUSTERED INDEX [$($idx.Key)]`n"
                $script += "ON [$($table.schema)].[$($table.name)] ($($idx.Value.columns -join ', '))`nGO`n"
            }
        }

        Write-ToFile -Content $script -ObjectName "$($table.schema).$($table.name)" -ObjectType 'Tables'
    }

    # 3. Extract Views
    $viewQuery = @"
    SELECT 
        s.name AS schema_name,
        v.name AS view_name,
        sm.definition AS view_definition
    FROM sys.views v
    JOIN sys.schemas s ON v.schema_id = s.schema_id
    LEFT JOIN sys.sql_modules sm ON v.object_id = sm.object_id
    WHERE v.is_ms_shipped = 0;
"@
    $command.CommandText = $viewQuery
    $reader = $command.ExecuteReader()
    while ($reader.Read()) {
        $viewDef = "USE [$DatabaseName]`nGO`n"
        if ($reader['view_definition']) {
            $viewDef += $reader['view_definition']
        } else {
            $viewDef += "-- Warning: Unable to retrieve view definition. The view might be encrypted.`n"
        }
        Write-ToFile -Content $viewDef -ObjectName "$($reader['schema_name']).$($reader['view_name'])" -ObjectType 'Views'
    }
    $reader.Close()

    # 4. Extract Stored Procedures
    $sprocQuery = @"
    SELECT 
        s.name AS schema_name,
        p.name AS proc_name,
        sm.definition AS proc_definition
    FROM sys.procedures p
    JOIN sys.schemas s ON p.schema_id = s.schema_id
    LEFT JOIN sys.sql_modules sm ON p.object_id = sm.object_id
    WHERE p.is_ms_shipped = 0;
"@
    $command.CommandText = $sprocQuery
    $reader = $command.ExecuteReader()
    while ($reader.Read()) {
        $sprocDef = "USE [$DatabaseName]`nGO`n"
        if ($reader['proc_definition']) {
            $sprocDef += $reader['proc_definition']
        } else {
            $sprocDef += "-- Warning: Unable to retrieve stored procedure definition. The procedure might be encrypted.`n"
        }
        Write-ToFile -Content $sprocDef -ObjectName "$($reader['schema_name']).$($reader['proc_name'])" -ObjectType 'StoredProcedures'
    }
    $reader.Close()

    # 5. Extract Functions
    $functionQuery = @"
    SELECT 
        s.name AS schema_name,
        o.name AS function_name,
        sm.definition AS function_definition
    FROM sys.objects o
    JOIN sys.schemas s ON o.schema_id = s.schema_id
    LEFT JOIN sys.sql_modules sm ON o.object_id = sm.object_id
    WHERE o.type IN ('FN', 'IF', 'TF')
    AND o.is_ms_shipped = 0;
"@
    $command.CommandText = $functionQuery
    $reader = $command.ExecuteReader()
    while ($reader.Read()) {
        $functionDef = "USE [$DatabaseName]`nGO`n"
        if ($reader['function_definition']) {
            $functionDef += $reader['function_definition']
        } else {
            $functionDef += "-- Warning: Unable to retrieve function definition. The function might be encrypted.`n"
        }
        Write-ToFile -Content $functionDef -ObjectName "$($reader['schema_name']).$($reader['function_name'])" -ObjectType 'Functions'
    }
    $reader.Close()

    # 6. Extract Triggers
    $triggerQuery = @"
    SELECT 
        s.name AS schema_name,
        t.name AS trigger_name,
        sm.definition AS trigger_definition
    FROM sys.triggers t
    JOIN sys.objects o ON t.parent_id = o.object_id
    JOIN sys.schemas s ON o.schema_id = s.schema_id
    LEFT JOIN sys.sql_modules sm ON t.object_id = sm.object_id
    WHERE t.is_ms_shipped = 0;
"@
    $command.CommandText = $triggerQuery
    $reader = $command.ExecuteReader()
    while ($reader.Read()) {
        $triggerDef = "USE [$DatabaseName]`nGO`n"
        if ($reader['trigger_definition']) {
            $triggerDef += $reader['trigger_definition']
        } else {
            $triggerDef += "-- Warning: Unable to retrieve trigger definition. The trigger might be encrypted.`n"
        }
        Write-ToFile -Content $triggerDef -ObjectName "$($reader['schema_name']).$($reader['trigger_name'])" -ObjectType 'Triggers'
    }
    $reader.Close()

} finally {
    $connection.Close()
}

Write-Host "Database DDL extraction completed. Files have been saved to: $baseDir"
