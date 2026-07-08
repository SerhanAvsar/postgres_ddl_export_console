# Load SQL Server SMO (SQL Server Management Objects)
Add-Type -AssemblyName "Microsoft.SqlServer.Smo"

# Define the connection to the SQL Server
$serverName = "sunucuAdresi"
$databaseName = "Trevoo"
$username = "kullaniciAdi"
$password = "parola"

# Set up the SQL Server connection
$securePassword = ConvertTo-SecureString $password -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($username, $securePassword)
$serverConnection = New-Object Microsoft.SqlServer.Management.Common.ServerConnection($serverName, $username, $password)
$server = New-Object Microsoft.SqlServer.Management.Smo.Server($serverConnection)
$database = $server.Databases[$databaseName]

# Define the output directory
$outputDir = "C:\Works\SQL_ddl_export\exported"

# Create the output directory if it doesn't exist
if (-Not (Test-Path -Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir
}

# Function to save the DDL to a file
function Save-ObjectDDL ($object, $type) {
    $scripter = New-Object Microsoft.SqlServer.Management.Smo.Scripter($server)
    $scripter.Options.ScriptSchema = $true
    $scripter.Options.ScriptData = $false
    $scripter.Options.IncludeHeaders = $true
	$scripter.Options.DriAll = $true
	$scripter.Options.Triggers = $true

    $script = $scripter.Script($object)
    $scriptContent = $script -join "`n"

    $objectDir = Join-Path $outputDir $type
    if (-Not (Test-Path -Path $objectDir)) {
        New-Item -ItemType Directory -Path $objectDir
    }

    $objectFile = Join-Path $objectDir ($object.Name + ".sql")
    $scriptContent | Out-File -FilePath $objectFile -Force
}

# Save the DDL for schemas
foreach ($table in $database.Schemas) {
    if ($table.IsSystemObject -eq $false) {
        Save-ObjectDDL $table "Schemas"
		break
    }
}

# Save the DDL for tables
foreach ($table in $database.Tables) {
    if ($table.IsSystemObject -eq $false) {
        Save-ObjectDDL $table "Tables"
		break
    }
}
# Save the DDL for views
foreach ($view in $database.Views) {
    if ($view.IsSystemObject -eq $false) {
        Save-ObjectDDL $view "Views"
		break
    }
}

# Save the DDL for stored procedures
foreach ($sproc in $database.StoredProcedures) {
    if ($sproc.IsSystemObject -eq $false) {
        Save-ObjectDDL $sproc "StoredProcedures"
		break
    }
}

Write-Output "DDL backup completed successfully."
