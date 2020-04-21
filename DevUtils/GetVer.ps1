[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true,Position=1)]
  [string]$inputFile
)
try {
  $asm = [System.Reflection.Assembly]::LoadFrom($inputFile)
  $ver = $asm.GetName().Version.ToString()
  [System.Console]::WriteLine($ver)
} catch {
  [System.Console]::WriteLine("VER Error")
}
