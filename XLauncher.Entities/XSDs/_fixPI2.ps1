[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true,Position=1)]
  [string]$inputFile,
  [Parameter(Mandatory=$true,Position=2)]
  [string]$outputFile
)

$reader = [System.IO.StreamReader] $inputFile
$writer = [System.IO.StreamWriter] $outputFile

$omit = $false
$lines = @()
while (($line = $reader.ReadLine()) -ne $null) {

  if ($line -match "System.CodeDom.Compiler.GeneratedCodeAttribute") {

    if (!($omit)) {
      foreach ($li in $lines) {
        $writer.WriteLine($li)
      }
    }

    $omit = $false
    $lines = @()

  }

  if ($line -match "urn:schemas-bimi-com:XLauncher:Common") {
    $omit = $true
  } elseif ($line -match "urn:schemas-bimi-com:XLauncher:Environments") {
    $omit = $true
  }

  if ($line -match "System.Xml.Serialization.XmlIncludeAttribute\(typeof\(Import\)\)") {
  } else {
    $lines += $line
  }

  if ($line -match "^(\s*)using System.Xml.Serialization;") {
    $lines += $matches[1] + "using XLauncher.Entities.Common;"
    $lines += $matches[1] + "using XLauncher.Entities.Environments;"
  }

}

foreach ($li in $lines) {
  $writer.WriteLine($li)
}

# Make sure the file gets fully written and clean up handles
$writer.Flush();
$writer.Dispose();
$reader.Dispose();
