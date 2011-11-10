
Properties { # General props
}
	
	Properties { # Directories
		$base_dir = Resolve-Path .
		$lib_dir = "$base_dir\lib"
		$build_dir = "$base_dir\build" 
	}
	
	Properties { # Projects and solutions
		$sln_file = "$base_dir\src\AutoBot.sln"
	}

Task default -depends Compile

	### Shared tasks, used regardless of environment
	task Clean { 
		if (test-path $build_dir) { remove-item -force -recurse $build_dir -ErrorAction SilentlyContinue }
	} 

	Task Compile -depends Clean {
		exec { & msbuild $sln_file /t:Build  /p:Configuration=Release /v:q }
	}

	
