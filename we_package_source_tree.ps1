#
#	Copyright ©2020-2021 WellEngineered.us, all rights reserved.
#	Distributed under the MIT license: http://www.opensource.org/licenses/mit-license.php
#

cls

$this_dir_path = [System.Environment]::CurrentDirectory


$src_dir_name = "src"
$src_dir_path = "$this_dir_path\$src_dir_name"

echo "SRC_DIR_PATH: $src_dir_path"


$nupkg_files = $null # can be explicitly set here or $null for auto-discovery

if ($nupkg_files -eq $null)
{
	$nupkg_files = Get-ChildItem -Recurse "$src_dir_path\*.nupkg" | Select-Object -Property Name, Directory, FullName

	if ($nupkg_files -eq $null)
	{ echo "An error occurred during the operation (NuGet Package file discovery)."; return; }

	foreach ($nupkg_file in $nupkg_files)
	{
		echo ("NUPKG_FILE[]: " + $nupkg_file)	
	}
}


$pkg_dir_name = "__we_nuget"
$pkg_dir_path = "$this_dir_path\$pkg_dir_name"

echo "PKG_DIR_PATH: $pkg_dir_path"


echo "The operation is starting..."

if ((Test-Path -Path $pkg_dir_path))
{
	Remove-Item $pkg_dir_path -recurse -force
}

New-Item -ItemType directory -Path $pkg_dir_path

foreach ($nupkg_file in $nupkg_files)
{
	$nupkg_file_path = $nupkg_file.FullName

	echo "NUPKG_FILE_PATH: $nupkg_file_path"

	Copy-Item $nupkg_file_path $pkg_dir_path
}

echo "The operation completed successfully."
