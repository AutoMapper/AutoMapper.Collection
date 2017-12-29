Framework '4.5.1x86'

properties {
	$base_dir = resolve-path .
	$source_dir = "$base_dir\src"
	$result_dir = "$base_dir\results"
	$artifacts_dir = "$base_dir\artifacts"
	$global:config = "debug"
}


task default -depends local
task local -depends init, compile, test
task ci -depends clean, release, local

task clean {
	rd "$artifacts_dir" -recurse -force  -ErrorAction SilentlyContinue | out-null
	rd "$result_dir" -recurse -force  -ErrorAction SilentlyContinue | out-null
}

task init {
	# Make sure per-user dotnet is installed
	Install-Dotnet
}

task release {
    $global:config = "release"
}

task compile -depends clean {

	$tag = $(git tag -l --points-at HEAD)
	$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
	$suffix = @{ $true = ""; $false = "ci-$revision"}[$tag -ne $NULL -and $revision -ne "local"]
	$commitHash = $(git rev-parse --short HEAD)
	$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]
	
	$buildParam = @{ $true = ""; $false = "--version-suffix=$buildSuffix"}[$tag -ne $NULL -and $revision -ne "local"]
	$packageParam = @{ $true = ""; $false = "--version-suffix=$suffix"}[$tag -ne $NULL -and $revision -ne "local"]
	
	echo "build: Tag is $tag"
	echo "build: Package version suffix is $suffix"
	echo "build: Build version suffix is $buildSuffix" 

	# restore all project references (creating project.assets.json for each project)
	exec { dotnet restore $base_dir\AutoMapper.Collection.sln /nologo }

	exec { dotnet build $base_dir\AutoMapper.Collection.sln -c $config $buildParam /nologo --no-restore }

	exec { dotnet pack $base_dir\AutoMapper.Collection.sln -c $config --include-symbols --no-build --no-restore --output $artifacts_dir $packageParam /nologo}
}

task test {
	$logger = @{ $true = "Appveyor"; $false = "trx" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];

	exec { dotnet test $source_dir\AutoMapper.Collection.Tests -c $config --no-build --no-restore --results-directory $result_dir --logger $logger /nologo }

	exec { dotnet test $source_dir\AutoMapper.Collection.EntityFramework.Tests -c $config --no-build --no-restore --results-directory $result_dir --logger $logger /nologo }
}

function Install-Dotnet
{
    $dotnetcli = where-is('dotnet')
	
    if($dotnetcli -eq $null)
    {
		$dotnetPath = "$pwd\.dotnet"
		$dotnetCliVersion = if ($env:DOTNET_CLI_VERSION -eq $null) { 'Latest' } else { $env:DOTNET_CLI_VERSION }
		$dotnetInstallScriptUrl = 'https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/install.ps1'
		$dotnetInstallScriptPath = '.\scripts\obtain\install.ps1'

		md -Force ".\scripts\obtain\" | Out-Null
		curl $dotnetInstallScriptUrl -OutFile $dotnetInstallScriptPath
		& .\scripts\obtain\install.ps1 -Channel "preview" -version $dotnetCliVersion -InstallDir $dotnetPath -NoPath
		$env:Path = "$dotnetPath;$env:Path"
	}
}

function where-is($command) {
    (ls env:\path).Value.split(';') | `
        where { $_ } | `
        %{ [System.Environment]::ExpandEnvironmentVariables($_) } | `
        where { test-path $_ } |`
        %{ ls "$_\*" -include *.bat,*.exe,*cmd } | `
        %{  $file = $_.Name; `
            if($file -and ($file -eq $command -or `
			   $file -eq ($command + '.exe') -or  `
			   $file -eq ($command + '.bat') -or  `
			   $file -eq ($command + '.cmd'))) `
            { `
                $_.FullName `
            } `
        } | `
        select -unique
}