@if "%1"=="init" (
  @set SCRIPT=configall.py
) else if "%1"=="proto" (
  @protoc.exe %2 %3 %4 %5 %6 %7 %8 %9
  @goto :eof
) else (
  @set SCRIPT=projector.py
)

@set PROJ=python %SCRIPT% %*
@for /f %%i in ('where ploymer.bat') do @set BIN=%%~dpi
@set PRECD=%cd%
@cd /d %BIN%..\pytool\
@cmd /C %PROJ%
@cd /d %PRECD%