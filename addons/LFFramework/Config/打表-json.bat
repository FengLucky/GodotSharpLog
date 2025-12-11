@echo off
cd /d %~dp0
dotnet Luban\Luban.dll ^
-p extend ^
-t client ^
--conf luban.conf ^
--customTemplateDir Templates ^
-x outputCodeDir=..\Data\GenCode ^
-x pathValidator.rootDir=..\ ^
-c cs-dotnet-json ^
-d json ^
-x outputDataDir=..\Data\Json ^
-x cs-dotnet-json.const=csharp ^
-x tableImporter.name=extend ^
-x tableImporter.tableMeta=TableMeta.ini ^
--validationFailAsError
pause
if %errorlevel% neq 0 exit /b %errorlevel%