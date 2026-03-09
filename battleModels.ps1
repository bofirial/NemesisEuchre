Param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Model,
    [Parameter(Position = 1)]
    [double]$Temperature = 0,
    [Parameter(Position = 2)]
    [double]$LearnRate = 0.7,
    [Parameter(Position = 3)]
    [int]$Iterations = 200,
    [Parameter(Position = 4)]
    [int]$NumberOfLeaves = 32,
    [Parameter(Position = 5)]
    [int]$MinimumExampleCountPerLeaf = 25,
    [Parameter(Position = 6)]
    [int]$ModelNumber = 0
)

$CsvPath = "calltrump-modelResults.csv";

$outputFile = "output.json";

$command = "dotnet run --project NemesisEuchre.Console -- -t1m gen3b -t2m gen3b -t2m-call $Model -c 25000 -json $outputFile";

Write-Host $command;

Invoke-Expression $command

$battleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

$command = "dotnet run --project NemesisEuchre.Console -- test -m $Model -json $outputFile";

Write-Host $command;

Invoke-Expression $command

$testOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

$newRow = [PSCustomObject]@{
    "Model Number"                   = $ModelNumber
    "Model Name"                     = $Model
    "Learn Rate"                     = $LearnRate
    "Iterations"                     = $Iterations
    "Number Of Leaves"               = $NumberOfLeaves
    "Minimum Example Count Per Leaf" = $MinimumExampleCountPerLeaf
    "Win Rate"                       = $battleOutput.Team2WinRate
    "CallTrump Passed Tests"         = $testOutput.TestsByDecisionType.CallTrump.Passed
    # "Discard Passed Tests"           = $testOutput.TestsByDecisionType.Discard.Passed
    # "Play Passed Tests"              = $testOutput.TestsByDecisionType.SimplePlay.Passed
}

$newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation