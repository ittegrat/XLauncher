[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true,Position=1)]
  [int]$days
)
try {
  $today = [System.DateTime]::Today
  $fdate = $today.AddDays($days).ToString("yyyyMMdd")
  [System.Console]::WriteLine($fdate)
} catch {
  [System.Console]::WriteLine("DATE Error")
}
