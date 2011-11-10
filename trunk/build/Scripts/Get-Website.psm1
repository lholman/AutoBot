function Get-WebSite {
<#
.SYNOPSIS
    Retrieves information about a website.
.DESCRIPTION
    Retrieves information about a website.
.PARAMETER Url
    URL of the website to test access to.
.PARAMETER UseDefaultCredentials
    Use the currently authenticated user's credentials
.PARAMETER Proxy
    Used to connect via a proxy
.PARAMETER TimeOut
    Timeout to connect to site, in milliseconds
.PARAMETER Credential
    Provide alternate credentials
.NOTES
    Name: Get-WebSite
    Author: Boe Prox
    DateCreated: 08Feb2011
.EXAMPLE
    Get-WebSite -url "http://www.bing.com"
Description
------------
Returns information about Bing.Com to include StatusCode and type of web server being used to host the site.

#>
[cmdletbinding(
	DefaultParameterSetName = 'url',
	ConfirmImpact = 'low'
)]
    Param(
        [Parameter(
            Mandatory = $True,
            Position = 0,
            ParameterSetName = '',
            ValueFromPipeline = $True)]
            [string][ValidatePattern("^(http|https)\://*")]$Url,
        [Parameter(
            Position = 1,
            Mandatory = $False,
            ParameterSetName = 'defaultcred')]
            [switch]$UseDefaultCredentials,
        [Parameter(
            Mandatory = $False,
            ParameterSetName = '')]
            [string]$Proxy,
        [Parameter(
            Mandatory = $False,
            ParameterSetName = '')]
            [Int]$Timeout,
        [Parameter(
            Mandatory = $False,
            ParameterSetName = 'altcred')]
            [switch]$Credential            

        )
Begin {
    $psBoundParameters.GetEnumerator() | % {
        Write-Verbose "Parameter: $_"
        }

    #Create the initial WebRequest object using the given url
    Write-Verbose "Creating the web request object"
    $webRequest = [net.WebRequest]::Create($url)

    #Use Proxy address if specified
    If ($PSBoundParameters.ContainsKey('Proxy')) {
        #Create Proxy Address for Web Request
        Write-Verbose "Creating proxy address and adding into Web Request"
        $webRequest.Proxy = New-Object -TypeName Net.WebProxy($proxy,$True)
        }

    #Set timeout
    If ($PSBoundParameters.ContainsKey('TimeOut')) {
        #Setting the timeout on web request
        Write-Verbose "Setting the timeout on web request"
        $webRequest.Timeout = $timeout
        }        

    #Determine if using Default Credentials
    If ($PSBoundParameters.ContainsKey('UseDefaultCredentials')) {
        #Set to True, otherwise remains False
        Write-Verbose "Using Default Credentials"
        $webrequest.UseDefaultCredentials = $True
        }
    #Determine if using Alternate Credentials
    If ($PSBoundParameters.ContainsKey('Credentials')) {
        #Prompt for alternate credentals
        Write-Verbose "Prompt for alternate credentials"
        $wc.Credential = (Get-Credential).GetNetworkCredential()
        }            

    #Set TimeStamp prior to attempting connection
    $then = get-date
    }
Process {
    Try {
        #Make connection to gather response from site
        $response = $webRequest.GetResponse()
        #If successful, get the date for comparison
        $now = get-date 

        #Generate report
        Write-Verbose "Generating report from website connection and response"
        $report = @{
            URL = $url
            StatusCode = $response.Statuscode -as [int]
            StatusDescription = $response.StatusDescription
            ResponseTime = "$(($now - $then).totalseconds)"
            WebServer = $response.Server
            Size = $response.contentlength
            }
        }
    Catch {
        #Get timestamp of failed attempt
        $now = get-date
        #Put the current error into a variable for later use
        $errorstring = "$($error[0])"

        #Generate report
        $report = @{
            URL = $url
            StatusCode = ([regex]::Match($errorstring,"\b\d{3}\b")).value
            StatusDescription = (($errorstring.split('\)')[2]).split('.\')[0]).Trim()
            ResponseTime = "$(($now - $then).totalseconds)"
            WebServer = $response.Server
            Size = $response.contentlength
            }
        }
    }
End {
    #Display Report
    return $report
    }
}
