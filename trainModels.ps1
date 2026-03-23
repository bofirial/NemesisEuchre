$modelNumber = 1;
$generationDirectory = "Gen5 Training";

$source = "idv5act3";
$decisionType = "AdvancedCallTrump";

if ($decisionType -eq "CallTrump") {
    $modelParameterLabel = "-t2m-call";
    $csvPath = "reports/$generationDirectory/calltrump-modelResults.csv";

    $modelPrefix = "can5ct";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.75, 0.875, 1.0)
    $iterations = @(25, 50, 75)
    $numbersOfLeaves = @(255)
    $minimumExampleCountsPerLeaf = @(500, 700)
}
elseif ($decisionType -eq "Discard") {
    $modelParameterLabel = "-t2m-discard";
    $csvPath = "reports/$generationDirectory/discard-modelResults.csv";
    
    $modelPrefix = "can5d";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.125, 0.25, 0.5)
    $iterations = @(75, 150, 200, 300)
    $numbersOfLeaves = @(31, 63)
    $minimumExampleCountsPerLeaf = @(200, 300)
}
elseif ($decisionType -eq "Play") {
    $modelParameterLabel = "-t2m-play";
    $csvPath = "reports/$generationDirectory/play-modelResults.csv";
    
    $modelPrefix = "can5p";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.5, 0.75)
    $iterations = @(75, 150, 200)
    $numbersOfLeaves = @(127, 255, 511)
    $minimumExampleCountsPerLeaf = @(200, 400)
}
elseif ($decisionType -eq "SimplePlay") {
    $modelParameterLabel = "-t2m-simple-play";
    $csvPath = "reports/$generationDirectory/simpleplay-modelResults.csv";
    
    $modelPrefix = "can5sp";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.5, 0.625, 0.675, 0.75)
    $iterations = @(100, 150, 200)
    $numbersOfLeaves = @(31, 63, 127)
    $minimumExampleCountsPerLeaf = @(400, 600, 800)
}
elseif ($decisionType -eq "AdvancedCallTrump") {
    $modelParameterLabel = "-t2m-advanced-call";
    $csvPath = "reports/$generationDirectory/advancedcalltrump-modelResults.csv";

    $modelPrefix = "can5act";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.625, 0.75, 0.875)
    $iterations = @(50, 75, 100)
    $numbersOfLeaves = @(127, 255, 511)
    $minimumExampleCountsPerLeaf = @(300, 500, 700)
}
elseif ($decisionType -eq "AdvancedPlay") {
    $modelParameterLabel = "-t2m-advanced-play";
    $csvPath = "reports/$generationDirectory/advancedplay-modelResults.csv";
    
    $modelPrefix = "can5ap";

    $l1Regularizations = @(0.0);
    $l2Regularizations = @(0.01);

    $learnRates = @(0.5, 0.625, 0.675, 0.75)
    $iterations = @(100, 150, 200)
    $numbersOfLeaves = @(31, 63, 127)
    $minimumExampleCountsPerLeaf = @(400, 600, 800)
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
