$CsvPath = "final-modelResults.csv";
$outputFile = "output.json";

$models = @(
    "Chaos",
    "gen1b2",
    "gen1c",
    "gen2b",
    "gen2t",
    "gen3b",
    "gen3t",
    "gen4b"
);

foreach ($model in $models) {
    foreach ($model2 in $models) {
        if ($model -eq $model2) {
            Write-Host "Skipping $model vs $model2 (Same Model)";
            continue;
        }

        $csvData = Import-Csv -Path $CsvPath

        if ($csvData | Where-Object { ($_."Team 1 Name" -eq $model2 -and $_."Team 2 Name" -eq $model) -or ($_."Team 1 Name" -eq $model -and $_."Team 2 Name" -eq $model2) }) {
            Write-Host "Skipping $model vs $model2 (Already Played)";
            continue;
        }

        $t1Model = "-t1m $model";
        $t2Model = "-t2m $model2";

        if ($model -eq "Chaos") {
            $t1Model = "-t1 $model";
        }
        if ($model2 -eq "Chaos") {
            $t2Model = "-t2 $model2";
        }

        $battleCommand = "dotnet run --project NemesisEuchre.Console -- $t1Model $t2Model -c 25000 -json $outputFile";

        Write-Host $battleCommand;

        Invoke-Expression $battleCommand

        $battleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Team 1 Name"     = $model
            "Team 2 Name"     = $model2
            "Battle Command"  = $battleCommand
            "Team 1 Win Rate" = $battleOutput.Team1WinRate
            "Team 2 Win Rate" = $battleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}