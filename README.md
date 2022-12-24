# Play.Inventory
Play Economy Inventory microservice

## Create and publish package
```powershell

$version="1.0.2"
$owner="DotNetMicroservicesBasics"
$local_packages_path="D:\Dev\NugetPackages"
$gh_pat="PAT HERE"

dotnet pack src\Play.Inventory.Contracts --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Inventory -o $local_packages_path

dotnet nuget push $local_packages_path\Play.Inventory.Contracts.$version.nupkg --api-key $gh_pat --source github
```