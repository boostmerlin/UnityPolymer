@if "%1"=="init" (
  @set SCRIPT=configall.py
) else (
  @set SCRIPT=projector.py
)

@set PROJ=python %SCRIPT% %*
@for /f %%i in ('where ploymer.bat') do @set BIN=%%~dpi
@set PRECD=%cd%
@cd /d %BIN%..\pytool\
@cmd /C %PROJ%
@cd /d %PRECD%