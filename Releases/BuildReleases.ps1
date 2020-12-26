$version = "1.1.0"

$platforms = @("linux-arm", "linux-x64", "osx-x64", "win-arm", "win-x64", "win-x86" )

foreach ($platform in $platforms) {

    $archivePath = "$platform/$platform-$version.zip"
    $projectPath = "$platform/latest"
    if (Test-Path $archivePath) {
        "$archivePath already exists. Not bothering to archive"
    } else {
        if (-not (Test-Path $projectPath)) {
            $publishProfile = "$platform Latest"
            "Starting publish for profile: '$publishProfile'"
            dotnet publish "../Source/TaskBud.Website/TaskBud.Website.csproj" -p:PublishProfile="$publishProfile" --output "$projectPath" -p:DebugType=None
        } else {
            "Published project already exists: '$projectPath'"
        }
    
        if (-not (Test-Path $projectPath)) {
            "Something broke, project folder cannot be found: '$projectPath'"
            exit 1
        }

        "Handling any broken LastWriteTime in: '$projectPath'"

        Get-ChildItem -path "$projectPath" -rec -file *.dll | Where-Object {$_.LastWriteTime -lt (Get-Date).AddYears(-20)} | %  { try { $_.LastWriteTime = '01/01/2020 00:00:00' } catch {} }

        "Compressing $projectPath to $archivePath"
        Compress-Archive -Path $projectPath -DestinationPath $archivePath -CompressionLevel "Optimal"

        if (-not (Test-Path $archivePath)) {
            "Archiving process failed for '$archivePath'"
            exit 1
        }
    }

    if (Test-Path $projectPath) {
        "Cleaning up $projectPath"
        Remove-Item $projectPath -Recurse
    }
}