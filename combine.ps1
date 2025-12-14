# .\combine.ps1 -SourcePath "C:\Users\seanh\Downloads\test" -Title "Line One\nLine Two\nLine Three"
param (
    [Parameter(Mandatory = $true)]
    [string]$SourcePath,
    
    # use \n for new lines
    [Parameter(Mandatory = $false)]
    [string]$Title = "My Video Title"
)

# =========================
# Configuration
# =========================
$titleText = $Title
$titleSecs = 4
$imageSecs = 3
$width = 1920
$height = 1080

# Create safe filename from title (remove invalid characters and literal \n strings)
$safeTitle = $titleText -replace '\\n', ' ' -replace '[<>:"/\\|?*]', ''
$outputFile = Join-Path $SourcePath "$safeTitle.mp4"

$fontPath = "C:/Windows/Fonts/arial.ttf"

$tempDir = Join-Path $SourcePath "_temp"
$concatFile = Join-Path $tempDir "files.txt"
$titleVideo = Join-Path $tempDir "000_title.mp4"

$ErrorActionPreference = "Stop"

# =========================
# Prep temp directory
# =========================
Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $tempDir | Out-Null

# =========================
# Create title card (WINDOWS-SAFE)
# =========================
# Convert escaped newlines (\n in parameter) to actual newlines
$titleTextFormatted = $titleText -replace '\\n', "`n"
$titleTextEscaped = $titleTextFormatted -replace "'", "\\'"
$drawTextFilter = @"
drawtext=font='Arial':
text='${titleTextEscaped}':
fontcolor=white:
fontsize=64:
x=(w-text_w)/2:
y=(h-text_h)/2
"@ -replace "`r?`n", ""

ffmpeg -y `
    -f lavfi `
    -i "color=c=black:s=${width}x${height}:d=$titleSecs" `
    -f lavfi -i "anullsrc=r=44100:cl=stereo:d=$titleSecs" `
    -vf "$drawTextFilter" `
    -c:v libx264 -pix_fmt yuv420p `
    -c:a aac -b:a 128k `
    "$titleVideo"

if (-not (Test-Path $titleVideo)) {
    throw "Title video was not created. FFmpeg drawtext failed."
}

# =========================
# Build concat list
# =========================
$lines = @()
$lines += "file '$titleVideo'"

$imgIndex = 1

Get-ChildItem -Path $SourcePath -File |
Sort-Object Name |
ForEach-Object {

    if ($_.FullName -like "$tempDir*") { return }
    if ($_.FullName -eq $outputFile) { return }

    $ext = $_.Extension.ToLower()

    # -------------------------
    # Video files
    # -------------------------
    if (@('.mp4', '.mov', '.mkv', '.avi') -contains $ext) {
        Write-Host "Adding video: $($_.Name)"
        
        # Convert video to standardized MP4 format (video only, no audio)
        # Normalize to 30fps to avoid concat demuxer sync issues
        $vidIndex = (Get-ChildItem -Path $tempDir -Filter "*_vid.mp4" -ErrorAction SilentlyContinue | Measure-Object).Count + 1
        $vidVideo = Join-Path $tempDir ("{0:D3}_vid.mp4" -f $vidIndex)
        
        ffmpeg -y `
            -i "$($_.FullName)" `
            -vf "fps=30,scale=${width}:${height}:force_original_aspect_ratio=decrease,pad=${width}:${height}:(ow-iw)/2:(oh-ih)/2" `
            -c:v libx264 -pix_fmt yuv420p `
            -c:a aac -b:a 128k `
            "$vidVideo"
        
        if (-not (Test-Path $vidVideo)) {
            throw "Failed to convert video file $($_.Name)"
        }
        
        $lines += "file '$vidVideo'"
    }

    # -------------------------
    # Image files
    # -------------------------
    elseif (@('.jpg', '.jpeg', '.png') -contains $ext) {
        Write-Host "Adding image: $($_.Name)"

        $imgVideo = Join-Path $tempDir ("{0:D3}_img.mp4" -f $imgIndex)

        ffmpeg -y `
            -loop 1 -i "$($_.FullName)" -t $imageSecs `
            -f lavfi -i "anullsrc=r=44100:cl=stereo:d=$imageSecs" `
            -shortest `
            -vf "scale=${width}:${height}:force_original_aspect_ratio=decrease,pad=${width}:${height}:(ow-iw)/2:(oh-ih)/2" `
            -c:v libx264 -pix_fmt yuv420p `
            -c:a aac -b:a 128k `
            "$imgVideo"

        if (-not (Test-Path $imgVideo)) {
            throw "Failed to create image video for $($_.Name)"
        }

        $lines += "file '$imgVideo'"
        $imgIndex++
    }
}

# Write concat list WITHOUT BOM
[System.IO.File]::WriteAllLines($concatFile, $lines, [System.Text.Encoding]::ASCII)

# =========================
# Concatenate
# =========================
# Build ffmpeg command with all input files
$inputFiles = @("$titleVideo")

# Add all generated video and image files in order
@(Get-ChildItem -Path $tempDir -Filter "*_vid.mp4" -ErrorAction SilentlyContinue) + @(Get-ChildItem -Path $tempDir -Filter "*_img.mp4" -ErrorAction SilentlyContinue) | Sort-Object Name | ForEach-Object {
    $inputFiles += $_.FullName
}

# Build filter dynamically based on actual number of input files
$numInputs = $inputFiles.Count
$filterParts = @()
for ($i = 0; $i -lt $numInputs; $i++) {
    $filterParts += "[$i`:v][$i`:a]"
}
$filterArgs = ($filterParts -join "") + "concat=n=$numInputs`:v=1:a=1[v][a]"

# Build the ffmpeg arguments array
$ffmpegArgs = @("-y")
foreach ($inputFile in $inputFiles) {
    $ffmpegArgs += "-i"
    $ffmpegArgs += $inputFile
}
$ffmpegArgs += "-filter_complex"
$ffmpegArgs += $filterArgs
$ffmpegArgs += "-map"
$ffmpegArgs += "[v]"
$ffmpegArgs += "-map"
$ffmpegArgs += "[a]"
$ffmpegArgs += "-c:v"
$ffmpegArgs += "libx264"
$ffmpegArgs += "-crf"
$ffmpegArgs += "18"
$ffmpegArgs += "-pix_fmt"
$ffmpegArgs += "yuv420p"
$ffmpegArgs += "-c:a"
$ffmpegArgs += "aac"
$ffmpegArgs += "-b:a"
$ffmpegArgs += "192k"
$ffmpegArgs += $outputFile

& ffmpeg $ffmpegArgs

Write-Host "✅ Done → $outputFile"
