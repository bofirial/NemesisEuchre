$CsvPath = "final-test-modelResults.csv";
$outputFile = "output.json";

$models = @(
    "gen1b2",
    "gen1c",
    "gen2b",
    "gen2t",
    "gen3b",
    "gen3t",
    "gen4b"
);

foreach ($model in $models) {
    
    $testCommand = "dotnet run --project NemesisEuchre.Console -- test -m $model -json $outputFile";

    Write-Host $testCommand;

    Invoke-Expression $testCommand

    $testOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

    $callTrumpSidecar = Get-Content -Path "models/$($model)_calltrump.json" -Raw | ConvertFrom-Json
    $discardSidecar = Get-Content -Path "models/$($model)_discardcard.json" -Raw | ConvertFrom-Json
    $playSidecar = $null

    if (Test-Path -Path "models/$($model)_playcard.json") {
        $playSidecar = Get-Content -Path "models/$($model)_playcard.json" -Raw | ConvertFrom-Json
    }
    elseif (Test-Path -Path "models/$($model)_simpleplaycard.json") {
        $playSidecar = Get-Content -Path "models/$($model)_simpleplaycard.json" -Raw | ConvertFrom-Json
    }

    $newRow = [PSCustomObject]@{
        "Model Name"                               = $Model
        "CallTrump Passed Tests"                   = $testOutput.TestsByDecisionType.CallTrump.Passed
        "Discard Passed Tests"                     = $testOutput.TestsByDecisionType.Discard.Passed
        "Play Passed Tests"                        = $testOutput.TestsByDecisionType.Play.Passed
        "Test Command"                             = $testCommand
        "CallTrump Model"                          = $callTrumpSidecar.ModelName
        "CallTrump Learn Rate"                     = $callTrumpSidecar.Hyperparameters.LearningRate
        "CallTrump Iterations"                     = $callTrumpSidecar.Hyperparameters.NumberOfIterations
        "CallTrump Number Of Leaves"               = $callTrumpSidecar.Hyperparameters.NumberOfLeaves
        "CallTrump Minimum Example Count Per Leaf" = $callTrumpSidecar.Hyperparameters.MinimumExampleCountPerLeaf
        "CallTrump TrainingSamples"                = $callTrumpSidecar.TrainingSamples
        "Discard Model"                            = $discardSidecar.ModelName
        "Discard Learn Rate"                       = $discardSidecar.Hyperparameters.LearningRate
        "Discard Iterations"                       = $discardSidecar.Hyperparameters.NumberOfIterations
        "Discard Number Of Leaves"                 = $discardSidecar.Hyperparameters.NumberOfLeaves
        "Discard Minimum Example Count Per Leaf"   = $discardSidecar.Hyperparameters.MinimumExampleCountPerLeaf
        "Discard TrainingSamples"                  = $discardSidecar.TrainingSamples
        "Play Model"                               = $playSidecar.ModelName
        "Play Learn Rate"                          = $playSidecar.Hyperparameters.LearningRate
        "Play Iterations"                          = $playSidecar.Hyperparameters.NumberOfIterations
        "Play Number Of Leaves"                    = $playSidecar.Hyperparameters.NumberOfLeaves
        "Play Minimum Example Count Per Leaf"      = $playSidecar.Hyperparameters.MinimumExampleCountPerLeaf
        "Play TrainingSamples"                     = $playSidecar.TrainingSamples
    }

    $newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation
}