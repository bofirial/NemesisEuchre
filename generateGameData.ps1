$temperatures = @(0.01, 0.1, 0.5);
$generations = @("3b", "3t");

foreach ($temperature in $temperatures) {
    foreach ($generation in $generations) {
        $gameCount = 1000000;
        if ($temperature -eq 0.5) {
            $gameCount = 500000;
        }

        $generationModel = "gen$generation"
        $idvFileName = "can$($generation)t$temperature"

        $command = "dotnet run --project NemesisEuchre.Console -- -t1m $generationModel -t2m $generationModel -t2t $temperature --count $gameCount -idv $idvFileName"
        Write-Host $command;

        Invoke-Expression $command

        Write-Host "Completed $idvFileName $(Get-Date)"
    }
}

$command = "dotnet run --project NemesisEuchre.Console -- merge -o can3a -s can3bt0.01 -s can3bt0.1 -s can3bt0.5 -s can3tt0.01 -s can3tt0.1 -s can3tt0.5"
Write-Host $command;

Invoke-Expression $command