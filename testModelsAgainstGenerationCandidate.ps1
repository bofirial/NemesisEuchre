$CsvPath = "battle-modelResults.csv";

$candidateModel = "gen3bc";
$numberOfGames = 25000;

$callTrumpModels = @(
    # "can3.200",
    "can3.211",
    # "can3.245",
    # "can3.220",
    # "can3.227",
    # "can3.138",
    # "can3.237",
    # "can3.210",
    "can3.192",
    # "can3.172",
    # "can3.202",
    # "can3.219",
    "can3.176",
    # "can3.110",
    "can3.184"
    # "can3.168",
    # "can3.46",
    # "can3.182",
    # "can3.160",
    # "can3.180",
    # "can3.169",
    # "can3.170",
    # "can3.153",
    # "can3.74",
    # "can3.186",
    # "can3.188",
    # "can3.113",
    # "can3.263",
    # "can3.194",
    # "can3.59",
    # "can3.136",
    # "can3.56",
    # "can3.246",
    # "can3.181"
);

$discardModels = @(
    # "can3d.89",
    # "can3d.88",
    # "can3d.22",
    # "can3d.2",
    # "can3d.76",
    "can3d.46",
    "can3d.10",
    # "can3d.74",
    # "can3d.26",
    "can3d.47",
    # "can3d.32",
    # "can3d.91",
    # "can3d.30",
    # "can3d.49",
    "can3d.16",
    "can3d.81",
    # "can3d.77",
    # "can3d.55",
    # "can3d.6",
    # "can3d.5",
    # "can3d.40",
    # "can3d.25",
    # "can3d.27",
    # "can3d.15",
    "can3d.87"
    # "can3d.78",
    # "can3d.3",
    # "can3d.44",
    # "can3d.52",
    # "can3d.38",
    # "can3d.94"
);

$playModels = @(
    # "can3sp.23",
    # "can3sp.7",
    # "can3sp.6",
    "can3sp.17",
    "can3sp.18",
    "can3sp.14",
    # "can3sp.27",
    # "can3sp.24",
    # "can3sp.8",
    # "can3sp.28",
    "can3sp.13"
    # "can3sp.21",
    # "can3sp.29",
    # "can3sp.11",
    # "can3sp.4",
    # "can3sp.26",
    # "can3sp.12",
    # "can3sp.2",
    # "can3sp.22",
    # "can3sp.10",
    # "can3sp.9",
    # "can3sp.20",
    # "can3sp.1",
    # "can3sp.3",
    # "can3sp.30"
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
            "Win Rate vs gen3bc" = $callTrumpBattleOutput.Team2WinRate
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

        $discardTrumpBattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Model Name"         = $Model
            "DecisionType"       = "Discard";
            "Win Rate vs gen3bc" = $discardTrumpBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}

foreach ($model in $playModels) {
    if (Test-Path -path "models/$($model)_simpleplaycard.zip") {
        $outputFile = "output.json";

        $command = "dotnet run --project NemesisEuchre.Console -- -t1m $candidateModel -t2m $candidateModel -t2m-simple-play $Model -c $numberOfGames -json $outputFile";

        Write-Host $command;

        Invoke-Expression $command

        $playBattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

        $newRow = [PSCustomObject]@{
            "Model Name"         = $Model
            "DecisionType"       = "Simple Play";
            "Win Rate vs gen3bc" = $playBattleOutput.Team2WinRate
        }

        $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
    }
}