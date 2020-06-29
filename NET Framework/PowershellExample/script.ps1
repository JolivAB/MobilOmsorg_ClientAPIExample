Param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    [Parameter(Mandatory=$true)]
    [string]$ApiSecret,
    [Parameter(Mandatory=$true)]
    [string]$CompanyCode,
    [Parameter(Mandatory=$true)]    
    [string]$BaseUri #Example: https://api.mobilomsorg.se/api
)

# Load Libraries
$path = Resolve-Path -Path "..\MOApiClient\bin\Debug\MOApiClient.dll"
Write-Host -Object $path
[Reflection.Assembly]::LoadFile($path)
$path = Resolve-Path -Path "...\MOApiClient\bin\Debug\RestSharp.dll"
[Reflection.Assembly]::LoadFile($path)
$path = Resolve-Path -Path "..\MOApiClient\bin\Debug\Newtonsoft.Json.dll"
[Reflection.Assembly]::LoadFile($path)

# Create Client instance
$client = New-Object MOApiClient.ApiClient $BaseUri
$client.CompanyCode = $CompanyCode
$client.ApiKey = $ApiKey
$client.ApiSecret = $ApiSecret

$Date = Get-Date -Format "yyyy-MM-dd"

# Get BI Data
$data = $client.RequestSync("GET", "BusinessIntelligence/GetTaskMetrics?from=$($Date)&to=$($Date)", $null, $null, "application/json")

# Output response BI Data
Write-Host -Object $data