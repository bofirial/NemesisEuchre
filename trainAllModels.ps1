$ErrorActionPreference = 'Stop'

$modelNumber = 1;
$source = "idv5c1";
$csvPath = "reports/Gen5 Training/phase1-modeltype-results.csv";

$modelConfigs = @(
    @{
        Name          = "can5ct.p1"
        DecisionType  = "CallTrump"
        BattleParam   = "-t2m-call"
        LearnRate     = 0.75
        Iterations    = 75
        NumberOfLeaves = 255
        MinimumExampleCountPerLeaf = 500
        L1            = 0.0
        L2            = 0.01
    },
    @{
        Name          = "can5act.p1"
        DecisionType  = "AdvancedCallTrump"
        BattleParam   = "-t2m-advanced-call"
        LearnRate     = 0.75
        Iterations    = 75
        NumberOfLeaves = 255
        MinimumExampleCountPerLeaf = 500
        L1            = 0.0
        L2            = 0.01
    },
    @{
        Name          = "can5d.p1"
        DecisionType  = "Discard"
        BattleParam   = "-t2m-discard"
        LearnRate     = 0.25
        Iterations    = 150
        NumberOfLeaves = 63
        MinimumExampleCountPerLeaf = 300
        L1            = 0.0
        L2            = 0.01
    },
    @{
        Name          = "can5ad.p1"
        DecisionType  = "AdvancedDiscard"
        BattleParam   = "-t2m-advanced-discard"
        LearnRate     = 0.25
        Iterations    = 150
        NumberOfLeaves = 63
        MinimumExampleCountPerLeaf = 300
        L1            = 0.0
        L2            = 0.01
    },
    @{
        Name          = "can5p.p1"
        DecisionType  = "Play"
        BattleParam   = "-t2m-play"
        LearnRate     = 0.75
        Iterations    = 100
        NumberOfLeaves = 127
        MinimumExampleCountPerLeaf = 400
        L1            = 0.0
        L2            = 0.01
    },
    @{
        Name          = "can5sp.p1"
        DecisionType  = "SimplePlay"
        BattleParam   = "-t2m-simple-play"
        LearnRate     = 0.75
        Iterations    = 100
        NumberOfLeaves = 127
        MinimumExampleCountPerLeaf = 400
        L1            = 0.0
        L2            = 0.01
    },
    @{
        Name          = "can5ap.p1"
        DecisionType  = "AdvancedPlay"
        BattleParam   = "-t2m-advanced-play"
        LearnRate     = 0.75
        Iterations    = 100
        NumberOfLeaves = 127
        MinimumExampleCountPerLeaf = 400
        L1            = 0.0
        L2            = 0.01
    }
)

foreach ($config in $modelConfigs) {
    $model = $config.Name

    if (Test-Path $csvPath) {
        $csvData = Import-Csv -Path $csvPath

        if ($csvData | Where-Object { $_."Model Name" -eq $model }) {
            Write-Host "Skipping $model (Already Trained)";
            $modelNumber = $modelNumber + 1;
            continue;
        }
    }

    $trainCommand = "dotnet run --project NemesisEuchre.Console -- train -s $source -m $model -d $($config.DecisionType) -lr $($config.LearnRate) -i $($config.Iterations) -l $($config.NumberOfLeaves) -msl $($config.MinimumExampleCountPerLeaf) -l1 $($config.L1) -l2 $($config.L2)";

    Write-Host $trainCommand;

    Invoke-Expression $trainCommand

    $battleCommand = "./battleModels -Model $model -ModelNumber $modelNumber -LearnRate $($config.LearnRate) -Iterations $($config.Iterations) -NumberOfLeaves $($config.NumberOfLeaves) -MinimumExampleCountPerLeaf $($config.MinimumExampleCountPerLeaf) -L1Regularization $($config.L1) -L2Regularization $($config.L2) -ModelParameterLabel ""$($config.BattleParam)"" -CsvPath ""$csvPath"" -DecisionType ""$($config.DecisionType)""";

    Write-Host $battleCommand;

    Invoke-Expression $battleCommand

    $modelNumber = $modelNumber + 1;
    Write-Host "Completed $model $(Get-Date)"
}
