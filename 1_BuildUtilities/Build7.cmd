set path_arg=%1
cd %path_arg%
IF EXIST *.yak DEL /F *.yak
"C:\Program Files\Rhino 7\System\Yak.exe" build
setlocal enabledelayedexpansion
for %%a in (*_*) do (
  set file=%%a
  ren "!file!" "!file:rh6_18-any.yak=rh7_0-any.yak!"
)