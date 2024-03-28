param (
	[Parameter()]
	[ValidateNotNullOrEmpty()]
	[string]
	$OutputPath = '.\bin\m3u-editor'
)

Write-Host 'Building'

dotnet publish `
	m3u_editor.csproj `
	-c Release `
	--self-contained false `
	-o $OutputPath

if ( -Not $? ) {
	exit $lastExitCode
	}

if ( Test-Path -Path .\bin\m3u-editor ) {
    rm -Force "$OutputPath\*.pdb"
    rm -Force "$OutputPath\*.xml"
}

Write-Host 'Build done'

ls $OutputPath

exit 0