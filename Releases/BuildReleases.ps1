$version = "1.0.0"

$platforms = @("linux-arm", "linux-x64", "osx-x64", "win-arm", "win-x64", "win-x86" )

foreach ($platform in $platforms) {

    $oldPath = "$platform/latest"

    if (Test-Path $oldPath) {
        $archivePath = "$platform/$platform-$version.zip"
        "Compressing $oldPath to $archivePath"
        Compress-Archive -Path $oldPath -DestinationPath $archivePath -CompressionLevel "Optimal"
        "Cleaning up $oldPath"
        Remove-Item $oldPath -Recurse
    }
}