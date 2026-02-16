Param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Suffix
)

$sources = @("gen3t0.1", "gen3t0.25", "gen3t0.4", "gen3t0.55", "gen3t0.7", "gen3t0.85", "gen3t1", "gen3t1.15", "gen3t1.3", "gen3t1.45", "gen3t1.6", "gen3t1.75");

foreach ($source in $sources) {
    $model = $source + $Suffix;

    $command = "dotnet run --project NemesisEuchre.Console -- train -s $source -m $model";

    Write-Host $command;

    Invoke-Expression $command
}

./battleModels $Suffix