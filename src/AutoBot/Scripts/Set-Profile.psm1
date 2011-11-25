function Set-Profile {
<#
.SYNOPSIS
    Provides AutoBot with a means to update his HipChat profile.
.DESCRIPTION
    Provides AutoBot with a means to update his HipChat profile.
.NOTES
    Name: Set-Profile
    Author: Lloyd Holman
    DateCreated: 2011/11/11
.EXAMPLE
    Set-Profile
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

		Add-Type -path "..\HipChat.Net.dll"
		
    }
Process {
			
			Try 
			{
				
			}
			catch [Exception] {
				Write-Host $_.Exception.ToString()
			}
    }
End {
		return $output | Out-String
    }
}
