$ErrorActionPreference = "Stop";


function Get-PokedexEntry()
{

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

	param( $id )

	# read the tab-delimited pokedex. the first line contains a title row
	$pokedex = [System.IO.File]::ReadAllLines("Scripts\pokedex.txt");

	$index = -1
	if( [System.Int32]::TryParse($id, [ref] $index) )
	{
		# use the $index-th pokemon
	}
	else
	{
		# find the pokemon by name
		$index = -1;
		$name = $id;
		for( $i=1; $i -lt $pokedex.Length; $i++ )
		{
			$parts = $pokedex[$i].Split("`t");
			if( ($parts.Count -gt 0) -and ($parts[1] -eq $name) )
			{
				$index = $i;
				break;
			}
		}
		if( $index -eq -1 )
		{
			throw new-object System.InvalidOperationException("Not found.");
		}
	}


	$pokemon = $pokedex[$index].Split("`t");
	$id   = [System.String]::Format("{0:D3}", $index);
	$name = $pokemon[1];
	$link = [System.String]::Format("http://bulbapedia.bulbagarden.net/wiki/{0}", $name);

	# the image is hosted on a cdn, so the url varies depending on which server it's hosted on.

	# the first step is to get the html that contains the image link
	$url = [System.String]::Format("http://bulbapedia.bulbagarden.net/wiki/File:{0}{1}.png", $id, $name);
	$client = new-object System.Net.WebClient;
	$html = $client.DownloadString($url);
	$client.Dispose();

	# the second part is to extract the image url
	$image = $html;
	$search = "<div class=`"fullImageLink`" id=`"file`"><a href=`"";
	$image = $html.Substring($image.IndexOf($search) + $search.Length);
	$search = "`">";
	$image = $image.Substring(0, $image.IndexOf($search));

	$name + " " + $link + " " + $image | Out-String
}