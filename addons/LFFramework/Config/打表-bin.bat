@echo off
cd /d %~dp0
dotnet Luban\Luban.dll ^
-p extend ^
-t client ^
--conf luban.conf ^
--customTemplateDir Templates ^
-x outputCodeDir=..\Data\GenCode ^
-x pathValidator.rootDir=..\ ^
-c cs-bin ^
-d bin ^
-x outputDataDir=..\Data\Bin ^
-x cs-bin.const=csharp ^
-x tableImporter.name=extend ^
-x tableImporter.tableMeta=TableMeta.ini ^
--validationFailAsError
pause
if %errorlevel% neq 0 exit /b %errorlevel%