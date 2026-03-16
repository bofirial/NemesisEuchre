$CsvPath = "reports/Gen 4 Training/battle-modelResults.csv";

$candidateModel = "gen4bc";
$numberOfGames = 25000;

$callTrumpModels = @(
    "can3ct.89",  # 0.50904 - LR=0.75, Iter=75, Leaves=255, MinEx=500
    "can3ct.85",  # 0.50584 - LR=0.625, Iter=75, Leaves=255, MinEx=500
    "can3ct.69",  # 0.50620 - LR=0.5, Iter=75, Leaves=255, MinEx=300
    "can3ct.91",  # 0.50404 - LR=0.75, Iter=125, Leaves=255, MinEx=500
    "can3ct.70"   # 0.50572 - LR=0.5, Iter=100, Leaves=255, MinEx=300
);

$discardModels = @(
    "can3d.31",   # 0.50640 - LR=0.25, Iter=150, Leaves=63, MinEx=300
    "can3d.37",   # 0.50588 - LR=0.5, Iter=75, Leaves=63, MinEx=300
    "can3d.16",   # 0.50524 - LR=0.5, Iter=200, Leaves=63, MinEx=200
    "can3d.9",    # 0.50464 - LR=0.25, Iter=200, Leaves=255, MinEx=200
    "can3d.67"    # 0.50384 - LR=0.25, Iter=200, Leaves=31, MinEx=200
);

$simplePlayModels = @(
    "can3sp.47",  # 0.51112 - LR=0.625, Iter=100, Leaves=63, MinEx=600
    "can3sp.12",  # 0.50896 - LR=0.625, Iter=100, Leaves=127, MinEx=400
    "can3sp.30",  # 0.50568 - LR=0.75, Iter=100, Leaves=127, MinEx=400
    "can3sp.20",  # 0.50392 - LR=0.675, Iter=100, Leaves=63, MinEx=400
    "can3sp.2"    # 0.50372 - LR=0.5, Iter=100, Leaves=63, MinEx=400
);

$playModels = @(
    "can3p.20",   # 0.50604 - LR=0.5, Iter=75, Leaves=255, MinEx=400
    "can3p.25",   # 0.50308 - LR=0.5, Iter=200, Leaves=127, MinEx=400
    "can3p.32",   # 0.50256 - LR=0.75, Iter=150, Leaves=255, MinEx=400
    "can3p.30"    # 0.50232 - LR=0.75, Iter=75, Leaves=511, MinEx=400
);

$advancedPlayModels = @(
    "can3p.20",   # 0.50604 - LR=0.5, Iter=75, Leaves=255, MinEx=400
    "can3p.25",   # 0.50308 - LR=0.5, Iter=200, Leaves=127, MinEx=400
    "can3p.32",   # 0.50256 - LR=0.75, Iter=150, Leaves=255, MinEx=400
    "can3p.30"    # 0.50232 - LR=0.75, Iter=75, Leaves=511, MinEx=400
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
            "Win Rate vs gen4bc" = $callTrumpBattleOutput.Team2WinRate
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
            "Win Rate vs gen4bc" = $discardBattleOutput.Team2WinRate
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
            "Win Rate vs gen4bc" = $simplePlayBattleOutput.Team2WinRate
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
            "Win Rate vs gen4bc" = $playBattleOutput.Team2WinRate
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
            "Win Rate vs gen4bc" = $playBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}
