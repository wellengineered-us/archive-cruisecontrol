@echo off

REM
REM Copyright ©2020-2021 WellEngineered.us, all rights reserved.
REM Distributed under the MIT license: https://opensource.org/licenses/MIT
REM

CALL we_set_ps_env.bat

"%POWERSHELL_EXE_PATH%" -command .\we_clean_source_tree > ".\__we_cleanup.log"
