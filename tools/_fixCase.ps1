[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true,Position=1)]
  [string]$inputFile,
  [Parameter(Mandatory=$true,Position=2)]
  [string]$outputFile,
  [switch]$DeleteInputFile
)

$reader = [System.IO.StreamReader] $inputFile
$writer = [System.IO.StreamWriter] $outputFile

while (($line = $reader.ReadLine()) -ne $null) {
  if ($line -match "^(\s+public\s+)(\w+)(\[]\s+)(\w+)(\s+.*$)") {
    $head = $matches[1] + $matches[2] + $matches[3]
    $tail = $matches[5]
    if ($matches[4] -eq "evar") {
      $middle = "EVar"
    } elseif ($matches[4] -eq "Items") {
      $middle = $matches[2]
    } else {
      $middle = $matches[4].substring(0,1).toupper() + $matches[4].substring(1)
    }
    if ($middle.substring($middle.length-1,1) -eq "x") {
      $middle = $middle + "es"
    } else {
      $middle = $middle + "s"
    }
    $rewrite = $head + $middle + $tail
    $writer.WriteLine($rewrite)
  } elseif ($line -match "^(\s+\[System\.Xml\.Serialization\.XmlAttributeAttribute\(.*)\)]\s*$") {
    $attrLine = $line
    $attr = $matches[1]
    if ($attr.substring($attr.length-1,1) -ne "(") {
      $attr = $attr + ", AttributeName="
    }
    $line = $reader.ReadLine()
    if ($line -match "^\s+\[System\.ComponentModel\.DefaultValueAttribute\(.*\)]\s*$") {
      $writer.WriteLine($line)
      $line = $reader.ReadLine()
    }
    if ($line -match "^(\s+public\s+)([\.\w]+)(\s+)(\w+)(\s+{\s*$)") {
      $attr = $attr + """" + $matches[4] + """)]"
      $head = $matches[1] + $matches[2] + $matches[3]
      $middle = $matches[4].substring(0,1).toupper() + $matches[4].substring(1)
      $tail = $matches[5]
      $rewrite = $head + $middle + $tail
      $writer.WriteLine($attr)
      $writer.WriteLine($rewrite)
    } else {
      $writer.WriteLine($attrLine)
      $writer.WriteLine($line)
    }
  } elseif ($line -match "^\s+\[System\.Diagnostics\.DebuggerStepThroughAttribute\(\)]\s*$") {
  } else {
    $writer.WriteLine($line)
  }
}

if ($DeleteInputFile -eq $true)
{
    Remove-Item $inputFile
}

# Make sure the file gets fully written and clean up handles
$writer.Flush();
$writer.Dispose();
$reader.Dispose();
