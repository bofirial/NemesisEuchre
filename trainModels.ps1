$modelNumber = 53;

$source = "can3a";
$decisionType = "CallTrump";

if ($decisionType -eq "CallTrump") {
    $modelParameterLabel = "-t2m-call";
    $csvPath = "reports/Gen4 Training/calltrump-modelResults.csv";

    $modelPrefix = "can3ct";

    $l1Regularizations = @(0.0, 0.3);
    $l2Regularizations = @(0.01, 0.1);

    $learnRates = @(0.5, 0.625)
    $iterations = @(200)
    $numbersOfLeaves = @(511)
    $minimumExampleCountsPerLeaf = @(300, 500)
}
elseif ($decisionType -eq "Discard") {
    $modelParameterLabel = "-t2m-discard";
    $csvPath = "reports/Gen4 Training/discard-modelResults.csv";
    
    $modelPrefix = "can3d";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.25, 0.5, 0.75)
    $iterations = @(200, 300)
    $numbersOfLeaves = @(63, 127, 255)
    $minimumExampleCountsPerLeaf = @(200, 300)
}
elseif ($decisionType -eq "Play") {
    $modelParameterLabel = "-t2m-play";
    $csvPath = "reports/Gen4 Training/play-modelResults.csv";
    
    $modelPrefix = "can3p";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.5, 0.675, 0.75)
    $iterations = @(200, 300)
    $numbersOfLeaves = @(255, 511, 640)
    $minimumExampleCountsPerLeaf = @(200, 300)
}
elseif ($decisionType -eq "SimplePlay") {
    $modelParameterLabel = "-t2m-simple-play";
    $csvPath = "reports/Gen4 Training/simpleplay-modelResults.csv";
    
    $modelPrefix = "can3sp";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.5, 0.675, 0.75)
    $iterations = @(200, 300)
    $numbersOfLeaves = @(31, 63, 127)
    $minimumExampleCountsPerLeaf = @(200, 300)
}

foreach ($l2Regularization in $l2Regularizations) {
    foreach ($l1Regularization in $l1Regularizations) {
        foreach ($minimumExampleCountPerLeaf in $minimumExampleCountsPerLeaf) {
            foreach ($learnRate in $learnRates) {
                foreach ($iteration in $iterations) {
                    foreach ($numberOfLeaves in $numbersOfLeaves) {
                        $model = "$modelPrefix.$modelNumber";

                        $trainCommand = "dotnet run --project NemesisEuchre.Console -- train -s $source -m $model -d $decisionType -lr $learnRate -i $iteration -l $numberOfLeaves -msl $minimumExampleCountPerLeaf -l1 $l1Regularization -l2 $l2Regularization";

                        Write-Host $trainCommand;

                        Invoke-Expression $trainCommand
                        
                        $battleCommand = "./battleModels -Model $model -ModelNumber $modelNumber -LearnRate $learnRate -Iterations $iteration -NumberOfLeaves $numberOfLeaves -MinimumExampleCountPerLeaf $minimumExampleCountPerLeaf -L1Regularization $l1Regularization -L2Regularization $l2Regularization -ModelParameterLabel $modelParameterLabel -CsvPath ""$csvPath""";

                        Write-Host $battleCommand;

                        Invoke-Expression $battleCommand

                        $modelNumber = $modelNumber + 1;
                        Write-Host "Completed $model $(Get-Date)"
                    }
                }
            }
        }
    }
}
