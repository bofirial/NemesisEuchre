$actSources = @("idv5c8", "idv5c1", "idv5c3")
$adSources = @("idv5c1", "idv5c2", "idv5c3", "idv5c4", "idv5c5", "idv5c6", "idv5c7", "idv5c8", "idv5c9")

$actOutput = "idv5act3"
$adOutput = "idv5ad9"

# Merge top 3 ACT IDV files
$actSourceArgs = ($actSources | ForEach-Object { "-s $_" }) -join " "
$actMergeCommand = "dotnet run --project NemesisEuchre.Console -- merge $actSourceArgs -o $actOutput -d CallTrump --overwrite"
Write-Host $actMergeCommand
Invoke-Expression $actMergeCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "ACT merge failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

Write-Host "ACT merge complete $(Get-Date)" -ForegroundColor Green

# Merge all 9 AD IDV files
$adSourceArgs = ($adSources | ForEach-Object { "-s $_" }) -join " "
$adMergeCommand = "dotnet run --project NemesisEuchre.Console -- merge $adSourceArgs -o $adOutput -d Discard --overwrite"
Write-Host $adMergeCommand
Invoke-Expression $adMergeCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "AD merge failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

Write-Host "AD merge complete $(Get-Date)" -ForegroundColor Green

# Run hyperparameter tuning
Write-Host "Starting trainModels.ps1 $(Get-Date)" -ForegroundColor Cyan
./trainModels.ps1
