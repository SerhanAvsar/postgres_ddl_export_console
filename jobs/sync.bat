IF %1.==. GOTO Missing_Args
IF %2.==. GOTO Missing_Args

set BRANCH=%1
set SETTINGS_FILE=%2

rmdir /S /Q export


git clone --branch "%BRANCH%" git@dev.bitbucket.org:biletbank2020/trevoo.git export

cd export
for /d %i in (*.*) do @rmdir /s /q "%i"
cd ..

cd app_deployed
ddl_export_console.exe -db:MSSQL -od:"C:\Robots\ddl_export\export" -s:%SETTINGS_FILE%
cd ..

cd export

git add -A

git commit -a -m "AutoUpdated"

git push

git tag -a "%date:~10,4%%date:~7,2%%date:~4,2%_%BRANCH%" -m "DatetimeAdded"

git push --tags 

cd ..

GOTO End_Of_Script

:Missing_Args
echo "usage: sync.bat <branch name> <settings json file>

:End_Of_Script