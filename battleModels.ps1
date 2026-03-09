Param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Model,
    [Parameter(Position = 1)]
    [double]$LearnRate = 0.7,
    [Parameter(Position = 2)]
    [int]$Iterations = 200,
    [Parameter(Position = 3)]
    [int]$NumberOfLeaves = 32,
    [Parameter(Position = 4)]
    [int]$MinimumExampleCountPerLeaf = 25,
    [Parameter(Position = 5)]
    [double]$L1Generalization = 0.0,
    [Parameter(Position = 6)]
    [double]$L2Generalization = 0.01,
    [Parameter(Position = 7)]
    [int]$ModelNumber = 0,
    [Parameter(Position = 8)]
    [string]$ModelParameterLabel = "-t2m-call",
    [Parameter(Position = 9)]
    [string]$CsvPath = "calltrump-modelResults.csv"
)

$outputFile = "output.json";

$battleCommand = "dotnet run --project NemesisEuchre.Console -- -t1m gen3b -t2m gen3b $ModelParameterLabel $Model -c 25000 -json $outputFile";

Write-Host $battleCommand;

Invoke-Expression $battleCommand

$battleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

$testCommand = "dotnet run --project NemesisEuchre.Console -- test -m $Model -json $outputFile";

Write-Host $testCommand;

Invoke-Expression $testCommand

$testOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

$newRow = [PSCustomObject]@{
    "Model Number"                   = $ModelNumber
    "Model Name"                     = $Model
    "Learn Rate"                     = $LearnRate
    "Iterations"                     = $Iterations
    "Number Of Leaves"               = $NumberOfLeaves
    "Minimum Example Count Per Leaf" = $MinimumExampleCountPerLeaf
    "L1 Generalization"              = $L1Generalization
    "L2 Generalization"              = $L2Generalization
    "Win Rate"                       = $battleOutput.Team2WinRate
    "CallTrump Passed Tests"         = $testOutput.TestsByDecisionType.CallTrump.Passed
    "Discard Passed Tests"           = $testOutput.TestsByDecisionType.Discard.Passed
    "Play Passed Tests"              = $testOutput.TestsByDecisionType.Play.Passed
    "Battle Command"                 = $battleCommand
    "Test Command"                   = $testCommand
}

$newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation