Param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Suffix
)

$CsvPath = "modelResults.csv";

$sources = @("gen3t0.1", "gen3t0.25", "gen3t0.4", "gen3t0.55", "gen3t0.7", "gen3t0.85", "gen3t1", "gen3t1.15", "gen3t1.3", "gen3t1.45", "gen3t1.6", "gen3t1.75");

$outputFile = "output.json";

foreach ($source in $sources) {
    $model = $source + $Suffix;

    $command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen1 -t2m $model -c 10000 -json $outputFile";

    Write-Host $command;

    Invoke-Expression $command

    $gen1BattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

    $command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen2 -t2m $model -c 10000 -json $outputFile";

    Write-Host $command;

    Invoke-Expression $command

    $gen2BattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

    $command = "dotnet run --project NemesisEuchre.Console -- test -m $model -json $outputFile";

    Write-Host $command;

    Invoke-Expression $command

    $testOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

    $newRow = [PSCustomObject]@{
        "Model Name" = $model
        "Win Rate vs Gen1" = $gen1BattleOutput.Team2WinRate
        "Win Rate vs Gen2" = $gen2BattleOutput.Team2WinRate
        "Call Trump Passed Tests" = $testOutput.TestsByDecisionType[0].Passed
        "Discard Card Passed Tests" = $testOutput.TestsByDecisionType[1].Passed
        "Play Card Passed Tests" = $testOutput.TestsByDecisionType[2].Passed
    }

    $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
}