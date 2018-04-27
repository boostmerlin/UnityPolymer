@echo off
if "%1"=="init" (
  set SCRIPT=configall.py
) else if "%1"=="proto" (
  protoc.exe %2 %3 %4 %5 %6 %7 %8 %9
  goto :eof
) else (
  set SCRIPT=projector.py
)

set EXER=python %SCRIPT% %*
for /f %%i in ('where polymer.bat') do @set BIN=%%~dpi
pushd %BIN%..\pytool\
cmd /C %EXER%
popd

@echo on