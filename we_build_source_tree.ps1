#
#	Copyright ©2020-2021 WellEngineered.us, all rights reserved.
#	Distributed under the MIT license: http://www.opensource.org/licenses/mit-license.php
#

cls

$this_dir_path = [System.Environment]::CurrentDirectory


$src_dir_name = "src"
$src_dir_path = "$this_dir_path\$src_dir_name"

echo "SRC_DIR_PATH: $src_dir_path"


$sln_file_path = $null # can be explicitly set here or $null for auto-discovery

if ($sln_file_path -eq $null)
{
	$sln_files = Get-ChildItem "$src_dir_path\*.sln" | Select-Object -First 1 -Property Name

	if ($sln_files -eq $null)
	{ echo "An error occurred during the operation (solution file discovery)."; return; }

	$sln_file_name = $sln_files[0].Name
	$sln_file_path = "$src_dir_path\$sln_file_name"
}

echo "SLN_FILE_NAME: $sln_file_name"
echo "SLN_FILE_PATH: $sln_file_path"


$msbuild_flavor = "debug"
$msbuild_verbosity = "quiet"
$msbuild_dir_path = "C:\Program Files\dotnet"
$msbuild_file_name = "dotnet.exe"
$msbuild_command = "msbuild"
$msbuild_exe = "$msbuild_dir_path\$msbuild_file_name"

echo "MSBUILD_FLAVOR: $msbuild_flavor"
echo "MSBUILD_VERBOSITY: $msbuild_verbosity"
echo "MSBUILD_DIR_PATH: $msbuild_dir_path"
echo "MSBUILD_FILE_NAME: $msbuild_file_name"
echo "MSBUILD_COMMAND: $msbuild_command"
echo "MSBUILD_EXE: $msbuild_exe"


echo "The operation is starting..."

&"$msbuild_exe" $msbuild_command /verbosity:$msbuild_verbosity /consoleloggerparameters:ErrorsOnly "$sln_file_path" /t:clean /p:Configuration="$msbuild_flavor"

if (!($LastExitCode -eq $null -or $LastExitCode -eq 0))
{ echo "An error occurred during the operation."; return; }

&"$msbuild_exe" $msbuild_command /verbosity:$msbuild_verbosity /consoleloggerparameters:ErrorsOnly "$sln_file_path" /t:restore /p:Configuration="$msbuild_flavor"

if (!($LastExitCode -eq $null -or $LastExitCode -eq 0))
{ echo "An error occurred during the operation."; return; }

&"$msbuild_exe" $msbuild_command /verbosity:$msbuild_verbosity /consoleloggerparameters:ErrorsOnly "$sln_file_path" /t:build /p:Configuration="$msbuild_flavor"

if (!($LastExitCode -eq $null -or $LastExitCode -eq 0))
{ echo "An error occurred during the operation."; return; }

echo "The operation completed successfully."
