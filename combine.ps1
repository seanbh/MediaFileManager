param (
    [Parameter(Mandatory = $true)]
    [string]$SourcePath
)

# =========================
# Configuration
# =========================
$titleText = "My Video Title"
$titleSecs = 4
$imageSecs = 3
$width = 1920
$height = 1080
$outputFile = Join-Path $SourcePath "final.mp4"

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
$titleTextEscaped = $titleText -replace "'", "\\'"
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
    -vf "$drawTextFilter" `
    -c:v libx264 -pix_fmt yuv420p `
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

    $ext = $_.Extension.ToLower()

    # -------------------------
    # Video files
    # -------------------------
    if ($ext -match '\.(mp4|mov|mkv|avi)$') {
        $lines += "file '$($_.FullName)'"
    }

    # -------------------------
    # Image files
    # -------------------------
    elseif ($ext -match '\.(jpg|jpeg|png)$') {

        $imgVideo = Join-Path $tempDir ("{0:D3}_img.mp4" -f $imgIndex)

        ffmpeg -y `
            -loop 1 -i "$($_.FullName)" -t $imageSecs `
            -vf "scale=${width}:${height}:force_original_aspect_ratio=decrease,pad=${width}:${height}:(ow-iw)/2:(oh-ih)/2" `
            -c:v libx264 -pix_fmt yuv420p `
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
ffmpeg -y `
    -f concat -safe 0 `
    -i "$concatFile" `
    -c:v libx264 -crf 18 `
    -c:a aac -b:a 192k `
    -pix_fmt yuv420p `
    "$outputFile"

Write-Host "✅ Done → $outputFile"
