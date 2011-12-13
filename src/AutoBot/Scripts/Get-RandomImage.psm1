$ErrorActionPreference = "Stop";

function Get-RandomImage() { 
<#
.SYNOPSIS
    Returns a random image from a Google image search for a given search term.
.DESCRIPTION
    Returns a random image from a Google image search for a given search term.
.NOTES
    Name: Get-RandomImage
    Author: Steve Garrett, Lloyd Holman
    DateCreated: 21/11/2011
.EXAMPLE
    Get-RandomImage coolio
Description
------------
Returns a picture of the rather awesome Coolio, hopefully!


#>
	param($term)
	
	Begin 
	{
			
	}
	Process
	{
		Try	
		{
			$html = (New-Object System.Net.WebClient).DownloadString("https://ajax.googleapis.com/ajax/services/search/images?v=1.0&q=$term")
			#Write-Output $html
			$regex = [regex] """url"":""([^""]+)"""
			$result = $regex.Matches($html) | get-random |% { $_.Groups[1].Value } 
		}
		Catch [Exception] {
			Write-Host $_.Exception.ToString()
		}
	}			
	End {
		return $result | Out-String
	}
}
