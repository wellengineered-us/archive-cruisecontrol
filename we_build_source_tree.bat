@echo off

REM
REM	Copyright ©2020-2021 WellEngineered.us, all rights reserved.
REM	Distributed under the MIT license: http://www.opensource.org/licenses/mit-license.php
REM

CALL we_set_ps_env.bat

"%POWERSHELL_EXE_PATH%" -command .\we_build_source_tree
