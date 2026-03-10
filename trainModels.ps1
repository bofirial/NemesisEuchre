$modelNumber = 1;

$source = "can3a";
$decisionType = "CallTrump";

if ($decisionType -eq "CallTrump") {
    $modelParameterLabel = "-t2m-call";
    $csvPath = "reports/Gen4 Training/calltrump-modelResults.csv";
    
    $modelPrefix = "can3ct";

    $l1Generalization = 0.0;

    $learnRates = @(0.375, 0.5, 0.625)
    $iterations = @(200, 300)
    $numbersOfLeaves = @(511, 640, 768)
    $minimumExampleCountsPerLeaf = @(200, 300)
}
elseif ($decisionType -eq "Discard") {
    $modelParameterLabel = "-t2m-discard";
    $csvPath = "reports/Gen4 Training/discard-modelResults.csv";
    
    $modelPrefix = "can3d";

    $l1Generalization = 0.0;

    $learnRates = @(0.25, 0.5, 0.75)
    $iterations = @(200, 300)
    $numbersOfLeaves = @(63, 127, 255)
    $minimumExampleCountsPerLeaf = @(200, 300)
}
elseif ($decisionType -eq "Play") {
    $modelParameterLabel = "-t2m-play";
    $csvPath = "reports/Gen4 Training/play-modelResults.csv";
    
    $modelPrefix = "can3p";

    $l1Generalization = 0.0;

    $learnRates = @(0.5, 0.675, 0.75)
    $iterations = @(200, 300)
    $numbersOfLeaves = @(255, 511, 640)
    $minimumExampleCountsPerLeaf = @(200, 300)
}
elseif ($decisionType -eq "SimplePlay") {
    $modelParameterLabel = "-t2m-simple-play";
    $csvPath = "reports/Gen4 Training/simpleplay-modelResults.csv";
    
    $modelPrefix = "can3sp";

    $l1Generalization = 0.0;

    $learnRates = @(0.5, 0.675, 0.75)
    $iterations = @(200, 300)
    $numbersOfLeaves = @(31, 63, 127)
    $minimumExampleCountsPerLeaf = @(200, 300)
}

foreach ($minimumExampleCountPerLeaf in $minimumExampleCountsPerLeaf) {
    foreach ($learnRate in $learnRates) {
        foreach ($iteration in $iterations) {
            foreach ($numberOfLeaves in $numbersOfLeaves) {
                $model = "$modelPrefix.$modelNumber";

                $trainCommand = "dotnet run --project NemesisEuchre.Console -- train -s $source -m $model -d $decisionType -lr $learnRate -i $iteration -l $numberOfLeaves -msl $minimumExampleCountPerLeaf -l1 $l1Generalization";

                Write-Host $trainCommand;

                # Invoke-Expression $trainCommand
                
                $battleCommand = "./battleModels -Model $model -ModelNumber $modelNumber -LearnRate $learnRate -Iterations $iteration -NumberOfLeaves $numberOfLeaves -MinimumExampleCountPerLeaf $minimumExampleCountPerLeaf -L1Generalization $l1Generalization -ModelParameterLabel $modelParameterLabel -CsvPath ""$csvPath""";

                Write-Host $battleCommand;

                Invoke-Expression $battleCommand

                $modelNumber = $modelNumber + 1;
                Write-Host "Completed $model $(Get-Date)"
            }
        }
    }
}
