function Get-Help {
<#
.SYNOPSIS
    Details all available AutoBot commands.
.DESCRIPTION
    Returns a list of names of all *.psm1 PowerShell Modules in the AutoBot\Scripts folder and optionally gives
	detailed help on a given script
.NOTES
    Name: Get-Help
    Author: Lloyd Holman
    DateCreated: 2011/11/09
.EXAMPLE
    Get-Help
Description
------------
Returns a list of names of all *.psm1 PowerShell Modules in the AutoBot\Scripts folder and optionally gives
detailed help on a given script

#>
[cmdletbinding()]
    Param(
        [Parameter(
            Mandatory = $False )]
            [string]$modulename
        )
Begin {
		
    }
Process {
			
			Try 
			{
				If ($modulename -ne "")
				{
					#Write-Output "$modulename provided"
					Microsoft.PowerShell.Core\Import-Module ".\$modulename.psm1"
					$ghelp = Microsoft.PowerShell.Core\Get-Help $moduleName
				}
				else 
				{						
					$dir = Get-ChildItem -recurse
					$fileList = $dir | Where {$_.extension -eq ".psm1"}
					$ghelp = $fileList | Format-Table name
				}
			}
			catch [Exception] {
				Write-Host $_.Exception.ToString()
			}
    }
End {
		return $ghelp | Out-String
    }
}
