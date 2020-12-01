#
#	Copyright ©2020-2021 WellEngineered.us, all rights reserved.
#	Distributed under the MIT license: https://opensource.org/licenses/MIT
#

cls

$this_dir_path = [System.Environment]::CurrentDirectory

$cleanup_file_name = ".we_cleanup"
$cleanup_file_path = "$this_dir_path\$cleanup_file_name"

$lines = Get-Content $cleanup_file_path

foreach ($line in $lines)
{
	if ([System.String]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#"))
	{ continue; }

	$remove_items = Get-ChildItem $this_dir_path -Recurse -Include $line -Force

	foreach ($remove_item in $remove_items)
	{
		#echo ("Cleanup item ($line): " + $remove_item.FullName)

		if ((Test-Path -Path $remove_item.FullName))
		{
			echo ("Deleting cleanup item ($line): " + $remove_item.FullName)

			try { Remove-Item $remove_item.FullName -Recurse -Force }
			catch { echo ("FAIL: cleanup item ($line): " + $remove_item.FullName) }
		}
		else
		{
			echo ("Ignoring cleanup item ($line): " + $remove_item.FullName)
		}
	}
}
