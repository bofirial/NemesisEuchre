Param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Model,
    [Parameter(Position = 1)]
    [double]$Temperature = 1,
    [Parameter(Position = 2)]
    [double]$LearnRate = 0.7,
    [Parameter(Position = 3)]
    [int]$Iterations = 200,
    [Parameter(Position = 4)]
    [int]$NumberOfLeaves = 16,
    [Parameter(Position = 5)]
    [int]$MinimumExampleCountPerLeaf = 25
)

$CsvPath = "callTrumpModelResults.csv";

$outputFile = "output.json";

$command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen2 -t1m-call Gen1 -t2m Gen2 -t2m-call $Model -c 25000 -json $outputFile";

Write-Host $command;

Invoke-Expression $command

$gen1BattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

$command = "dotnet run --project NemesisEuchre.Console -- -t1m Gen2                -t2m Gen2 -t2m-call $Model -c 25000 -json $outputFile";

Write-Host $command;

Invoke-Expression $command

$gen2BattleOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

$command = "dotnet run --project NemesisEuchre.Console -- test -m $Model -json $outputFile";

Write-Host $command;

Invoke-Expression $command

$testOutput = Get-Content -Path $outputFile -Raw | ConvertFrom-Json

Write-Host $testOutput

$newRow = [PSCustomObject]@{
    "Model Name"                     = "$Model CallTrump"
    "Temperature"                    = $Temperature
    "Learn Rate"                     = $LearnRate
    "Iterations"                     = $Iterations
    "Number Of Leaves"               = $NumberOfLeaves
    "Minimum Example Count Per Leaf" = $MinimumExampleCountPerLeaf
    "Win Rate vs Gen1 CallTrump"     = $gen1BattleOutput.Team2WinRate
    "Win Rate vs Gen2 CallTrump"     = $gen2BattleOutput.Team2WinRate
    "Call Trump Passed Tests"        = $testOutput.TestsByDecisionType.CallTrump.Passed
}

$newRow | Export-Csv -Path $CsvPath -Append -NoTypeInformation