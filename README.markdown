# AutoBot

<img src="http://teamcity.codebetter.com/app/rest/builds/buildType:(id:bt972)/statusIcon" alt="teamcity.codebetter AutoBot build status"/>


# Introduction
AutoBot is inspired by GitHub's splendid [HUBOT](http://hubot.github.com/) and a childhood love of [Transformers](http://en.wikipedia.org/wiki/Autobot), AutoBot however is a chat bot for the Windows crew. 

+ The core bot engine is written in C# (.NET 4), his functionality and extensibility is provided by the addition of PowerShell 2.0 script modules which are dynamically loaded and executed at runtime.
+ AutoBot currently chats only with the [XMPP](http://xmpp.org/about-xmpp/) powered awesomeness that is [HipChat](http://www.hipchat.com) using the [jabber-net](http://code.google.com/p/jabber-net/) library

_WARNING_: AutoBot is an infant and has some obvious (and other not so obvious) restrictions/issues/flaws, please take a look at [AutoBot's current issues](https://github.com/lholman/AutoBot/issues?labels=AutoBot.Engine&sort=created&direction=desc&state=open&page=1) before continuing.

## Building Your AutoBot 
1. [Setup Git](http://help.github.com/win-set-up-git/)

1. Get AutoBot source by cloning the github repo

		$ git clone git@github.com:lholman/AutoBot.git

1. Build AutoBot

		C:\AutoBot>rename "src\AutoBot\App.config.example"  "src\AutoBot\App.config"
		
		C:\AutoBot>build.bat

## Running Your AutoBot
+ Set PowerShell Execution policy (be sure to use the 32 Bit Powershell Command Prompt as an Administrator)
		PS C:\>Set-ExecutionPolicy RemoteSigned

+ As a CommandLine App

		C:\AutoBot\build\>AutoBot.exe

+ As a Windows Service

		C:\AutoBot\src\>Install AutoBot service.bat
		
		Then simply Start\Stop the service in services.msc until your hearts content
		
## Scripts
AutoBot's scripts are written in [PowerShell](http://en.wikipedia.org/wiki/Windows_PowerShell).  
AutoBot comes with a [couple of simple scripts](https://github.com/lholman/AutoBot/tree/master/src/AutoBot/Scripts) to get you started.  If you'd like to contribute to his library of scripts please head over to the community scripts repository at [AutoBot-Scripts](https://github.com/lholman/AutoBot-Scripts) and get scripting.

## Contributing To AutoBot Scripts
1. Head over to [AutoBot-Scripts](https://github.com/lholman/AutoBot-Scripts)

## Contributing To AutoBot's Core Engine
1. Check out [AutoBot's current issues](https://github.com/lholman/AutoBot/issues?labels=AutoBot.Engine&sort=created&direction=desc&state=open&page=1)
1. [Fork it](http://help.github.com/fork-a-repo/).
1. Create a branch (git checkout -b my_autobot)
1. Commit your changes (git commit -am "Added cool feature")
1. Push to the branch (git push origin my_autobot)
1. Create an [Issue](http://github.com/lholman/AutoBot/issues) with a link to your branch for everyone to discuss
1. If your contribution is needed/wanted we will ask you to send us a [Pull Request](http://help.github.com/send-pull-requests/) and merge your changes in

## License
AutoBot is released under the [MIT license](http://opensource.org/licenses/MIT)
