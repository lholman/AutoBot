# AutoBot
AutoBot is inspired by GitHub's splendid [HUBOT](http://hubot.github.com/), AutoBot however, is one for the Windows crew. 
The core bot engine is written in C# (.NET 4), his functionality and extensibility is provided by the addition of PowerShell modules which are dynamically loaded and executed at runtime.

WARNING: AutoBot is an infant and has some obvious (and other not so obvious) restrictions/issues/flaws, please take a look at [AutoBot's current issues](https://github.com/lholman/AutoBot/issues?labels=AutoBot.Engine&sort=created&direction=desc&state=open&page=1) before continuing.

## Building Your AutoBot 
+ Setup Git

	http://help.github.com/win-set-up-git/

+ Get AutoBot source by cloning the github repo

	$ git clone git@github.com:lholman/AutoBot.git

+ Build AutoBot

	C:\AutoBot>rename "src\AutoBot.Cmd\App.config.example"  "src\AutoBot.Cmd\App.config"
	C:\AutoBot>build.bat

## Running Your AutoBot
+ As a CommandLine App
    C:\AutoBot>AutoBot.Cmd.exe

## Scripts
AutoBot's scripts are written in [PowerShell](http://en.wikipedia.org/wiki/Windows_PowerShell).  
AutoBot comes with a [couple of simple scripts](https://github.com/lholman/AutoBot/tree/master/src/AutoBot.Cmd/Scripts) to get you started.  If you'd like to contribute to his library of scripts please head over to the community scripts repository at [AutoBot-Scripts](https://github.com/lholman/AutoBot-Scripts) and get scripting.

## Contributing To AutoBot Scripts
1. Head over to [AutoBot-Scripts](https://github.com/lholman/AutoBot-Scripts)

## Contributing To AutoBot Core
1. Fork it.
1. Create a branch (git checkout -b my_autobot)
1. Commit your changes (git commit -am "Added cool feature")
1. Push to the branch (git push origin my_autobot)
1. Create an [Issue](http://github.com/lholman/AutoBot/issues) with a link to your branch
1. Wait for feedback

## Disclaimer
NO warranty, expressed or written