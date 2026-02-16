
# $sources = @("gen3t0.1", "gen3t0.25", "gen3t0.4", "gen3t0.55", "gen3t0.7", "gen3t0.85", "gen3t1", "gen3t1.15", "gen3t1.3", "gen3t1.45", "gen3t1.6", "gen3t1.75");

# $learnRates = @{};

# $learnRates.Add("gen3msl05", 5);
# $learnRates.Add("gen3msl10", 10);
# $learnRates.Add("gen3msl15", 15);
# $learnRates.Add("gen3msl20", 20);
# $learnRates.Add("gen3msl25", 25);
# $learnRates.Add("gen3msl30", 30);
# $learnRates.Add("gen3msl35", 35);
# $learnRates.Add("gen3msl40", 40);

# foreach ($kvp in $learnRates.GetEnumerator()) {

#     $command = "dotnet run --project NemesisEuchre.Console -- train -s gen3t1.45 -d CallTrump -m $($kvp.Key) -msl $($kvp.Value)";

#     Write-Host $command;

#     Invoke-Expression $command

#     ./battleModels $kvp.Key -Temperature 1.45 -MinimumExampleCountPerLeaf $kvp.Value
# }

$command = "dotnet run --project NemesisEuchre.Console -- train -s gen2 -d CallTrump -m gen2x -lr 0.7 -i 200 -l 16 -msl 25";

Write-Host $command;

Invoke-Expression $command

./battleModels "gen2x" -Temperature 0.25