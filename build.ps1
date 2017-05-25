Framework '4.5.1x86'

properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$source_dir = "$base_dir\src"
	$result_dir = "$build_dir\results"
	$artifacts_dir = "$base_dir\artifacts"
	$global:config = "debug"
}


task default -depends local
task local -depends init, compile, test
task ci -depends clean, release, local

task clean {
	rd "$source_dir\artifacts" -recurse -force  -ErrorAction SilentlyContinue | out-null
	rd "$base_dir\build" -recurse -force  -ErrorAction SilentlyContinue | out-null
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
	
	echo "build: Tag is $tag"
	echo "build: Package version suffix is $version"

    exec { .\.nuget\NuGet.exe restore $base_dir\AutoMapper.Collection.sln }

    exec { dotnet restore $base_dir\AutoMapper.Collection.sln }

    exec { dotnet build $base_dir\AutoMapper.Collection.sln -c $config --version-suffix=$buildSuffix -v q /nologo }

	exec { dotnet pack $source_dir\AutoMapper.Collection -c $config --include-symbols --no-build --output $artifacts_dir --version-suffix $suffix}

	exec { dotnet pack $source_dir\AutoMapper.Collection.EntityFramework -c $config --include-symbols --no-build --output $artifacts_dir --version-suffix $suffix}

	exec { dotnet pack $source_dir\AutoMapper.Collection.LinqToSQL -c $config --include-symbols --no-build --output $artifacts_dir --version-suffix $suffix}
}

task test {
    $testRunners = @(gci $base_dir\packages -rec -filter Fixie.Console.exe)

    if ($testRunners.Length -ne 1)
    {
        throw "Expected to find 1 Fixie.Console.exe, but found $($testRunners.Length)."
    }

    $testRunner = $testRunners[0].FullName

    exec { & $testRunner $source_dir\AutoMapper.Collection.Tests\bin\$config\AutoMapper.Collection.Tests.dll }
    exec { & $testRunner $source_dir\AutoMapper.Collection.EntityFramework.Tests\bin\$config\AutoMapper.Collection.EntityFramework.Tests.dll }
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