@echo off

REM
REM	Copyright ©2020-2021 WellEngineered.us, all rights reserved.
REM	Distributed under the MIT license: http://www.opensource.org/licenses/mit-license.php
REM

REM required for successful test coverage execution under .NET Core
SET COMPLUS_ReadyToRun=0
SET COREHOST_TRACE=0
SET DOTNET_CLI_CAPTURE_TIMING=0

CALL set_ps_env.bat

"%POWERSHELL_EXE_PATH%" -command .\we_run_unit_test_coverage
