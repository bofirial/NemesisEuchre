$command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen2 -t2m Gen2 -t2t 1.3 --count 1000000 -idv gen3t1.3"
Write-Host $command;
Invoke-Expression $command

$command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen2 -t2m Gen2 -t2t 1.45 --count 1000000 -idv gen3t1.45"
Write-Host $command;
Invoke-Expression $command

$command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen2 -t2m Gen2 -t2t 1.6 --count 1000000 -idv gen3t1.6"
Write-Host $command;
Invoke-Expression $command

$command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen2 -t2m Gen2 -t2t 1.75 --count 1000000 -idv gen3t1.75"
Write-Host $command;
Invoke-Expression $command