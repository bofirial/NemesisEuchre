$CsvPath = "reports/Gen5 Training/battle-modelResults.csv";

$candidateModel = "gen5bc";
$numberOfGames = 25000;

$callTrumpModels = @(
    "can5act.87",  # 0.54428 - LR=0.375, Iter=50, Leaves=255, MinEx=500
    "can5act.2",   # 0.54352 - LR=0.625, Iter=50, Leaves=255, MinEx=300
    "can5act.60",  # 0.54288 - LR=0.625, Iter=75, Leaves=511, MinEx=700
    "can5act.88",  # 0.54264 - LR=0.5, Iter=25, Leaves=255, MinEx=500
    "can5act.30"   # 0.54176 - LR=0.625, Iter=50, Leaves=511, MinEx=500
);

$discardModels = @(
    "can5ad.28",   # 0.50568 - LR=0.125, Iter=75, Leaves=31, MinEx=300
    "can5ad.10",   # 0.50480 - LR=0.25, Iter=75, Leaves=31, MinEx=200
    "can5ad.41",   # 0.50472 - LR=0.25, Iter=150, Leaves=63, MinEx=300
    "can5ad.48",   # 0.50380 - LR=0.375, Iter=75, Leaves=127, MinEx=300
    "can5ad.46"    # 0.50356 - LR=0.375, Iter=75, Leaves=31, MinEx=300
);

$simplePlayModels = @(
);

$playModels = @(
);

$advancedPlayModels = @(
    "can5ap.114",  # 0.53084 - LR=0.75, Iter=25, Leaves=127, MinEx=400, L2=0.05
    "can5ap.239",  # 0.52884 - LR=0.75, Iter=25, Leaves=63, MinEx=600, L2=0.07
    "can5ap.155",  # 0.52516 - LR=0.75, Iter=10, Leaves=127, MinEx=400, L2=0.03
    "can5ap.147",  # 0.52468 - LR=0.625, Iter=15, Leaves=127, MinEx=400, L2=0.03
    "can5ap.197"   # 0.52460 - LR=0.75, Iter=15, Leaves=127, MinEx=200, L2=0.05
);

foreach ($model in $callTrumpModels) {
    if (Test-Path -path "models/$($model)_calltrump.zip") {
        $outputFile = "output.json";

        $command = "dotnet run --project NemesisEuchre.Console -- -t1m $candidateModel -t2m $candidateModel -t2m-call $Model -c $numberOfGames -json $outputFile";

        Write-Host $command;

        Invoke-Expression $command

        $callTrumpBattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Model Name"         = $Model
            "DecisionType"       = "Call Trump";
            "Win Rate vs gen5bc" = $callTrumpBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}

foreach ($model in $discardModels) {
    if (Test-Path -path "models/$($model)_discardcard.zip") {
        $outputFile = "output.json";

        $command = "dotnet run --project NemesisEuchre.Console -- -t1m $candidateModel -t2m $candidateModel -t2m-discard $Model -c $numberOfGames -json $outputFile";

        Write-Host $command;

        Invoke-Expression $command

        $discardBattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Model Name"         = $Model
            "DecisionType"       = "Discard";
            "Win Rate vs gen5bc" = $discardBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}

foreach ($model in $simplePlayModels) {
    if (Test-Path -path "models/$($model)_simpleplaycard.zip") {
        $outputFile = "output.json";

        $command = "dotnet run --project NemesisEuchre.Console -- -t1m $candidateModel -t2m $candidateModel -t2m-simple-play $Model -c $numberOfGames -json $outputFile";

        Write-Host $command;

        Invoke-Expression $command

        $simplePlayBattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Model Name"         = $Model
            "DecisionType"       = "Simple Play";
            "Win Rate vs gen5bc" = $simplePlayBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}

foreach ($model in $playModels) {
    if (Test-Path -path "models/$($model)_playcard.zip") {
        $outputFile = "output.json";

        $command = "dotnet run --project NemesisEuchre.Console -- -t1m $candidateModel -t2m $candidateModel -t2m-play $Model -c $numberOfGames -json $outputFile";

        Write-Host $command;

        Invoke-Expression $command

        $playBattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Model Name"         = $Model
            "DecisionType"       = "Play";
            "Win Rate vs gen5bc" = $playBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}

foreach ($model in $advancedPlayModels) {
    if (Test-Path -path "models/$($model)_advancedplaycard.zip") {
        $outputFile = "output.json";

        $command = "dotnet run --project NemesisEuchre.Console -- -t1m $candidateModel -t2m $candidateModel -t2m-advanced-play $Model -c $numberOfGames -json $outputFile";

        Write-Host $command;

        Invoke-Expression $command

        $playBattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Model Name"         = $Model
            "DecisionType"       = "Advanced Play";
            "Win Rate vs gen5bc" = $playBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}
