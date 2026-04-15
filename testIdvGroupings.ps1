$ErrorActionPreference = 'Stop'

$modelNumber = 1;
$csvPath = "reports/Gen5 Training/phase2-composition-results.csv";

$groupings = @("idv5c1", "idv5c2", "idv5c3", "idv5c4", "idv5c5", "idv5c6", "idv5c7", "idv5c8", "idv5c9")

$decisionTypes = @(
    @{
        DecisionType  = "AdvancedCallTrump"
        ModelPrefix   = "can5act"
        BattleParam   = "-t2m-advanced-call"
        LearnRate     = 0.375
        Iterations    = 50
        NumberOfLeaves = 255
        MinimumExampleCountPerLeaf = 500
        L1            = 0.0
        L2            = 0.01
    },
    @{
        DecisionType  = "AdvancedDiscard"
        ModelPrefix   = "can5ad"
        BattleParam   = "-t2m-advanced-discard"
        LearnRate     = 0.125
        Iterations    = 75
        NumberOfLeaves = 31
        MinimumExampleCountPerLeaf = 300
        L1            = 0.0
        L2            = 0.01
    },
    @{
        DecisionType  = "AdvancedPlay"
        ModelPrefix   = "can5ap"
        BattleParam   = "-t2m-advanced-play"
        LearnRate     = 0.75
        Iterations    = 25
        NumberOfLeaves = 127
        MinimumExampleCountPerLeaf = 400
        L1            = 0.0
        L2            = 0.05
    }
)

$groupingIndex = 1;

foreach ($grouping in $groupings) {
    foreach ($dt in $decisionTypes) {
        $model = "$($dt.ModelPrefix).g$groupingIndex"

        if (Test-Path $csvPath) {
            $csvData = Import-Csv -Path $csvPath

            if ($csvData | Where-Object { $_."Model Name" -eq $model }) {
                Write-Host "Skipping $model (Already Trained)";
                $modelNumber = $modelNumber + 1;
                continue;
            }
        }

        $trainCommand = "dotnet run --project NemesisEuchre.Console -- train -s $grouping -m $model -d $($dt.DecisionType) -lr $($dt.LearnRate) -i $($dt.Iterations) -l $($dt.NumberOfLeaves) -msl $($dt.MinimumExampleCountPerLeaf) -l1 $($dt.L1) -l2 $($dt.L2)";

        Write-Host $trainCommand;

        Invoke-Expression $trainCommand

        $battleCommand = "./battleModels -Model $model -ModelNumber $modelNumber -LearnRate $($dt.LearnRate) -Iterations $($dt.Iterations) -NumberOfLeaves $($dt.NumberOfLeaves) -MinimumExampleCountPerLeaf $($dt.MinimumExampleCountPerLeaf) -L1Regularization $($dt.L1) -L2Regularization $($dt.L2) -ModelParameterLabel ""$($dt.BattleParam)"" -CsvPath ""$csvPath"" -DecisionType ""$($dt.DecisionType)"" -IdvSource ""$grouping""";

        Write-Host $battleCommand;

        Invoke-Expression $battleCommand

        $modelNumber = $modelNumber + 1;
        Write-Host "Completed $model $(Get-Date)"
    }

    $groupingIndex = $groupingIndex + 1;
}
