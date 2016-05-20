$version_suffix="$env:version_suffix"

if ((get-command dotnet.exe -ErrorAction SilentlyContinue) -eq $null) {
    Write-Error "You need to have dotnet installed and added to your path, download from http://dot.net"
    exit 255
}


Get-ChildItem src\*\project.json | % { 

    if ($version_suffix -ne ""){
        &dotnet pack --output "artifacts" --configuration Release --version-suffix $version_suffix "$_"
    } else {
        &dotnet pack --output "artifacts" --configuration Release "$_"
    }
}
