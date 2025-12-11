cd "$(dirname "${BASH_SOURCE[0]}")"
(dotnet Luban/Luban.dll \
-t editor \
--conf luban.conf \
-x outputCodeDir=../Data/Editor \
-x pathValidator.rootDir=../ \
-c gdscript-editor \
-x tableImporter.name=extend \
-x tableImporter.tableMeta=TableMeta.ini \
--validationFailAsError) || {
  pause
  exit 1
}
pause