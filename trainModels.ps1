$modelNumber = 1;

$learnRates = @(0.375, 0.5, 0.675, 0.75, 0.875);
$iterations = @(200);
$numbersOfLeaves = @(63, 127);
$minimumExampleCountsPerLeaf = @(600);

foreach ($minimumExampleCountPerLeaf in $minimumExampleCountsPerLeaf) {
    foreach ($learnRate in $learnRates) {
        foreach ($iteration in $iterations) {
            foreach ($numberOfLeaves in $numbersOfLeaves) {
                $source = "can3a"
                $model = "can3ct.$modelNumber";

                $command = "dotnet run --project NemesisEuchre.Console -- train -s $source -m $model -d CallTrump -lr $learnRate -i $iteration -l $numberOfLeaves -msl $minimumExampleCountPerLeaf -l1 0.0";

                Write-Host $command;

                Invoke-Expression $command
                
                Write-Host "./battleModels -Model $model -ModelNumber $modelNumber -LearnRate $learnRate -Iterations $iteration -NumberOfLeaves $numberOfLeaves -MinimumExampleCountPerLeaf $minimumExampleCountPerLeaf";

                ./battleModels -Model $model -ModelNumber $modelNumber -LearnRate $learnRate -Iterations $iteration -NumberOfLeaves $numberOfLeaves -MinimumExampleCountPerLeaf $minimumExampleCountPerLeaf
                    
                $modelNumber = $modelNumber + 1;
                Write-Host "Completed $model $(Get-Date)"
            }
        }
    }
}
