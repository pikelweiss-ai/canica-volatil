param(
    [ValidateSet('Android','Windows','Linux')][string]$Target='Windows',
    [string]$Unity='C:\Program Files\Unity\Hub\Editor\6000.0.53f1\Editor\Unity.exe',
    [string]$BuildRoot
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if ($BuildRoot) {
    if (![IO.Path]::IsPathRooted($BuildRoot)) { $BuildRoot = Join-Path $project $BuildRoot }
    $BuildRoot = [IO.Path]::GetFullPath($BuildRoot)
    $logs = Join-Path $BuildRoot 'Logs'
} else {
    $logs = Join-Path $project 'Builds\BuildLogs'
}
New-Item -ItemType Directory -Force -Path $logs | Out-Null
$platform=if($Target -eq 'Android'){'Android'}elseif($Target -eq 'Linux'){'Linux64'}else{'Win64'}
function Invoke-Unity([string]$method,[string]$log) {
    $arguments=@('-batchmode','-nographics','-quit','-projectPath',('"'+$project+'"'),'-buildTarget',$platform,'-executeMethod',$method,'-logFile',('"'+$log+'"'))
    $originalRoot = $env:CANICA_ALPHA_BUILD_ROOT
    try {
        $env:CANICA_ALPHA_BUILD_ROOT = $BuildRoot
        $process=Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
    } finally {
        $env:CANICA_ALPHA_BUILD_ROOT = $originalRoot
    }
    $process.WaitForExit()
    if($process.ExitCode -ne 0){throw "Unity falla ($($process.ExitCode)): $log"}
}
$prepare=if($Target -eq 'Android'){'AlphaMobileBuild.PrepareAndroid'}else{'AlphaMobileBuild.PreparePC'}
Invoke-Unity $prepare (Join-Path $logs "$Target-prepare.log")
$build=if($Target -eq 'Android'){'AlphaMobileBuild.BuildAndroid'}elseif($Target -eq 'Linux'){'AlphaReviewBuild.BuildLinux'}else{'AlphaReviewBuild.BuildWindows'}
Invoke-Unity $build (Join-Path $logs "$Target-build.log")
Write-Output "ALPHA_BUILD_OK: $Target; logs: $logs"
