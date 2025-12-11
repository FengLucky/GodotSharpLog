cd "$(dirname "${BASH_SOURCE[0]}")"
(dotnet Luban/Luban.dll \
-p extend \
-t client \
--conf luban.conf \
--customTemplateDir Templates \
-x outputCodeDir=../Data/GenCode \
-x pathValidator.rootDir=../ \
-c cs-dotnet-json \
-d json \
-x outputDataDir=../Data/Json \
-x cs-dotnet-json.const=csharp \
-x tableImporter.name=extend \
-x tableImporter.tableMeta=TableMeta.ini \
--validationFailAsError) || {
  pause
  exit 1
}
pause