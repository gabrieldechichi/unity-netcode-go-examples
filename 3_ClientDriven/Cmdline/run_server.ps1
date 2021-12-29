$current = Get-Location
$buildPath = "${current}\Build\Win32\3_ClientDriven.exe"

& $buildPath -mlapi server $args