$version = "1.0.0"

$platforms = @("linux-arm", "linux-x64", "osx-x64", "win-arm", "win-x64", "win-x86" )

foreach ($platform in $platforms) {

    $oldPath = "$platform/latest"
    $newPath = "$platform/$version-latest"

    if (Test-Path $oldPath) {
        "Renaming '$oldPath' to '$newPath'"
        Move-Item $oldPath $newPath
        $archivePath = "$newPath.zip"
        "Compressing $newPath to $archivePath"
        Compress-Archive -Path $newPath -DestinationPath $archivePath -CompressionLevel "Optimal"
        "Cleaning up $newPath"
        Remove-Item $newPath -Recurse
    }
}