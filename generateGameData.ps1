$ErrorActionPreference = 'Stop'

$Model = "gen4c"
$TestModel = "gen3t"
$ProjectPath = "NemesisEuchre.Console"

# Game counts per grouping
$BaselineCount = 3125000
$MirrorLowCount = 4375000
$MirrorHighCount = 1875000
$ChaosFullHalfCount = 6250000
$MixedMirrorCount = 4375000
$MixedOpponentCount = 3750000
$ThreeWayCount = 3125000
$Gen4FormulaLow = 2500000
$Gen4FormulaHigh = 1250000
$Gen3tOpponentCount = 1875000

function Write-StepLog {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
}

function Invoke-GameGeneration {
    param(
        [string]$IdvName,
        [int]$Count,
        [string]$Team1Model,
        [float]$Team1Temp,
        [string]$Team2Model,
        [float]$Team2Temp,
        [string]$Team2ActorType
    )

    $dotnetArgs = @(
        "run", "--project", $ProjectPath, "--",
        "--count", $Count,
        "-idv", $IdvName,
        "-t1m", $Team1Model,
        "-t1t", $Team1Temp
    )

    if ($Team2ActorType) {
        $dotnetArgs += @("-t2", $Team2ActorType)
    }
    else {
        $dotnetArgs += @("-t2m", $Team2Model, "-t2t", $Team2Temp)
    }

    $command = "dotnet $($dotnetArgs -join ' ')"
    Write-StepLog "Running: $command"

    & dotnet @dotnetArgs

    if ($LASTEXITCODE -ne 0) {
        throw "Game generation failed for $IdvName (exit code $LASTEXITCODE)"
    }

    Write-StepLog "Completed $IdvName"
}

function Invoke-IdvMerge {
    param(
        [string[]]$Sources,
        [string]$Output
    )

    $dotnetArgs = @("run", "--project", $ProjectPath, "--", "merge", "-o", $Output)
    foreach ($source in $Sources) {
        $dotnetArgs += @("-s", $source)
    }

    $command = "dotnet $($dotnetArgs -join ' ')"
    Write-StepLog "Running: $command"

    & dotnet @dotnetArgs

    if ($LASTEXITCODE -ne 0) {
        throw "Merge failed for $Output (exit code $LASTEXITCODE)"
    }

    Write-StepLog "Completed merge -> $Output"
}

# ── Grouping 1 (g5c1): Baseline — 100% gen4c-vs-gen4c, t=0.01 ──
Write-StepLog "=== Grouping 1 (g5c1): Baseline t=0.01 ==="
Invoke-GameGeneration -IdvName "g5c1a" -Count $BaselineCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-GameGeneration -IdvName "g5c1b" -Count $BaselineCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-IdvMerge -Sources @("g5c1a_gen4c_0.01t", "g5c1b_gen4c_0.01t") -Output "g5c1"

# ── Grouping 2 (g5c2): 100% gen4c-vs-gen4c, t=0.1 ──
Write-StepLog "=== Grouping 2 (g5c2): Baseline t=0.1 ==="
Invoke-GameGeneration -IdvName "g5c2a" -Count $BaselineCount -Team1Model $Model -Team1Temp 0.1 -Team2Model $Model -Team2Temp 0.1
Invoke-GameGeneration -IdvName "g5c2b" -Count $BaselineCount -Team1Model $Model -Team1Temp 0.1 -Team2Model $Model -Team2Temp 0.1
Invoke-IdvMerge -Sources @("g5c2a_gen4c_0.1t", "g5c2b_gen4c_0.1t") -Output "g5c2"

# ── Grouping 3 (g5c3): 70% t=0.01 + 30% t=0.5 ──
Write-StepLog "=== Grouping 3 (g5c3): 70% t=0.01 + 30% t=0.5 ==="
Invoke-GameGeneration -IdvName "g5c3_low" -Count $MirrorLowCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-GameGeneration -IdvName "g5c3_high" -Count $MirrorHighCount -Team1Model $Model -Team1Temp 0.5 -Team2Model $Model -Team2Temp 0.5
Invoke-IdvMerge -Sources @("g5c3_low_gen4c_0.01t", "g5c3_high_gen4c_0.5t") -Output "g5c3"

# ── Grouping 4 (g5c4): 100% gen4c-vs-ChaosBot, t=0.01 (strong team only) ──
Write-StepLog "=== Grouping 4 (g5c4): gen4c vs ChaosBot ==="
Invoke-GameGeneration -IdvName "g5c4a" -Count $ChaosFullHalfCount -Team1Model $Model -Team1Temp 0.01 -Team2ActorType "Chaos"
Invoke-GameGeneration -IdvName "g5c4b" -Count $ChaosFullHalfCount -Team1Model $Model -Team1Temp 0.01 -Team2ActorType "Chaos"
Invoke-IdvMerge -Sources @("g5c4a_gen4c_0.01t", "g5c4b_gen4c_0.01t") -Output "g5c4"

# ── Grouping 5 (g5c5): 70% mirror + 30% ChaosBot, t=0.01 ──
Write-StepLog "=== Grouping 5 (g5c5): 70% mirror + 30% ChaosBot ==="
Invoke-GameGeneration -IdvName "g5c5_mirror" -Count $MixedMirrorCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-GameGeneration -IdvName "g5c5_chaos" -Count $MixedOpponentCount -Team1Model $Model -Team1Temp 0.01 -Team2ActorType "Chaos"
Invoke-IdvMerge -Sources @("g5c5_mirror_gen4c_0.01t", "g5c5_chaos_gen4c_0.01t") -Output "g5c5"

# ── Grouping 6 (g5c6): 70% mirror + 30% ChadBot, t=0.01 ──
Write-StepLog "=== Grouping 6 (g5c6): 70% mirror + 30% ChadBot ==="
Invoke-GameGeneration -IdvName "g5c6_mirror" -Count $MixedMirrorCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-GameGeneration -IdvName "g5c6_chad" -Count $MixedOpponentCount -Team1Model $Model -Team1Temp 0.01 -Team2ActorType "Chad"
Invoke-IdvMerge -Sources @("g5c6_mirror_gen4c_0.01t", "g5c6_chad_gen4c_0.01t") -Output "g5c6"

# ── Grouping 7 (g5c7): 50% mirror + 25% Chaos + 25% Chad, t=0.01 ──
Write-StepLog "=== Grouping 7 (g5c7): 50% mirror + 25% Chaos + 25% Chad ==="
Invoke-GameGeneration -IdvName "g5c7_mirror" -Count $ThreeWayCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-GameGeneration -IdvName "g5c7_chaos" -Count $ThreeWayCount -Team1Model $Model -Team1Temp 0.01 -Team2ActorType "Chaos"
Invoke-GameGeneration -IdvName "g5c7_chad" -Count $ThreeWayCount -Team1Model $Model -Team1Temp 0.01 -Team2ActorType "Chad"
Invoke-IdvMerge -Sources @("g5c7_mirror_gen4c_0.01t", "g5c7_chaos_gen4c_0.01t", "g5c7_chad_gen4c_0.01t") -Output "g5c7"

# ── Grouping 8 (g5c8): Gen4 formula with gen4c (3 temps, 2:2:1 ratio) ──
Write-StepLog "=== Grouping 8 (g5c8): Gen4 formula (2:2:1 temp ratio) ==="
Invoke-GameGeneration -IdvName "g5c8_t001" -Count $Gen4FormulaLow -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-GameGeneration -IdvName "g5c8_t01" -Count $Gen4FormulaLow -Team1Model $Model -Team1Temp 0.1 -Team2Model $Model -Team2Temp 0.1
Invoke-GameGeneration -IdvName "g5c8_t05" -Count $Gen4FormulaHigh -Team1Model $Model -Team1Temp 0.5 -Team2Model $Model -Team2Temp 0.5
Invoke-IdvMerge -Sources @("g5c8_t001_gen4c_0.01t", "g5c8_t01_gen4c_0.1t", "g5c8_t05_gen4c_0.5t") -Output "g5c8"

# ── Grouping 9 (g5c9): 70% mirror + 30% gen3t, t=0.01 (both teams' data) ──
Write-StepLog "=== Grouping 9 (g5c9): 70% mirror + 30% gen3t ==="
Invoke-GameGeneration -IdvName "g5c9_mirror" -Count $MixedMirrorCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $Model -Team2Temp 0.01
Invoke-GameGeneration -IdvName "g5c9_gen3t" -Count $Gen3tOpponentCount -Team1Model $Model -Team1Temp 0.01 -Team2Model $TestModel -Team2Temp 0.01
Invoke-IdvMerge -Sources @("g5c9_mirror_gen4c_0.01t", "g5c9_gen3t_gen4c_0.01t", "g5c9_gen3t_gen3t_0.01t") -Output "g5c9"

Write-StepLog "=== All 9 groupings complete ==="
