function Get-Help {
<#
.SYNOPSIS
    Displays all available AutoBot commands.
.DESCRIPTION
    Returns a list of names of all *.psm1 PowerShell Modules in the AutoBot\Scripts folder
.NOTES
    Name: Get-Help
    Author: Lloyd Holman
    DateCreated: 2011/11/09
.EXAMPLE
    Get-Help
Description
------------
Returns a list of names of all *.psm1 PowerShell Modules in the AutoBot\Scripts folder

#>

Begin {
    #Set TimeStamp prior to attempting connection
    $then = get-date
    }
Process {
    Try {
        #Make connection to gather response from site
        $dir = Get-ChildItem -recurse
		$fileList = $dir | Where {$_.extension -eq ".psm1"}
		$fileList | Format-Table name | Out-String
        }
    Catch {
        }
    }
End {
    #Display Report
    #New-Object PSObject -property $fileList
    }
}
