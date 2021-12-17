$current = Get-Location
$buildPath = "${current}\Build\Win32\2_GoldenPath.exe"

& $buildPath -mlapi client
