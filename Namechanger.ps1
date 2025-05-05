# Set your old and new service names
$oldServiceName = "OldServiceName"
$newServiceName = "NewServiceName"

# Set the path where your services are located
$servicesPath = "D:\Fontys\SoftwareSemester6\dms"

# Define the folder names for your services
$oldServicePath = Join-Path -Path $servicesPath -ChildPath $oldServiceName
$newServicePath = Join-Path -Path $servicesPath -ChildPath $newServiceName

# Rename the folder
Rename-Item -Path $oldServicePath -NewName $newServiceName

# Go to the new service folder
Set-Location -Path $newServicePath

# Rename all .csproj files inside the service folder (for example, rename "OldServiceName.Api.csproj" to "NewServiceName.Api.csproj")
$csprojFiles = Get-ChildItem -Recurse -Filter "*.csproj"

foreach ($csprojFile in $csprojFiles) {
    $newCsprojFileName = $csprojFile.Name.Replace($oldServiceName, $newServiceName)
    Rename-Item -Path $csprojFile.FullName -NewName $newCsprojFileName
}

# Now update the references in the .csproj files
foreach ($csprojFile in $csprojFiles) {
    $csprojContent = Get-Content -Path $csprojFile.FullName
    $updatedContent = $csprojContent -replace $oldServiceName, $newServiceName

    # Save the updated content back to the .csproj file
    Set-Content -Path $csprojFile.FullName -Value $updatedContent
}

# Now update namespaces in the C# files (optional)
$csFiles = Get-ChildItem -Recurse -Filter "*.cs"

foreach ($csFile in $csFiles) {
    $fileContent = Get-Content -Path $csFile.FullName
    $updatedFileContent = $fileContent -replace $oldServiceName, $newServiceName

    # Save the updated content back to the C# file
    Set-Content -Path $csFile.FullName -Value $updatedFileContent
}

# Optionally, update namespaces in any other relevant files (e.g., interfaces, configuration, etc.)
Write-Host "Service rename completed!"