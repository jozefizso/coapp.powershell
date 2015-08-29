@echo off
setlocal
cd %~dp0

set TOOLSDIR=%~dp0\tools

:: Scrub the output directories clea
msbuild /t:Clean ClrPlus.sln /p:Configuration=Release /p:TargetFrameworkVersion=v4.5 || goto failed

rmdir /s /q output\v45
rmdir /s /q intermediate\v45

:: Build .NET 4.0 and 4.5 versions
msbuild /t:Rebuild ClrPlus.sln /p:Configuration=Release /p:TargetFrameworkVersion=v4.5 || goto failed

:: Do merges for monolithic packages
echo Merging Monolithic builds for .NET 4.5
pushd output\v45\AnyCPU\Release\bin

echo Merging Monolithic build for .NET 4.5 [ClrPlus.Powershell.dll]
%TOOLSDIR%\il-repack.exe /log:merge.log /t:library /xmldocs /targetplatform:v4 /lib:"C:\Program Files (x86)\Reference Assemblies\Microsoft\WindowsPowerShell\3.0" /out:ClrPlus.Powershell.dll ClrPlus.Powershell.Azure.dll  ClrPlus.Core.dll ClrPlus.Scripting.dll ClrPlus.Scripting.MsBuild.dll ClrPlus.Platform.dll ClrPlus.Windows.Api.dll ClrPlus.Powershell.Core.dll || goto failed

:: rem skip ClrPlus.Powershell.Azure.dll for now.
echo Merging Monolithic build for .NET 4.5 [ClrPlus.dll]
%TOOLSDIR%\il-repack.exe /log:merge.log /t:library /xmldocs /targetplatform:v4 /lib:"C:\Program Files (x86)\Reference Assemblies\Microsoft\WindowsPowerShell\3.0" /out:ClrPlus.dll ClrPlus.CommandLine.dll ClrPlus.Console.dll ClrPlus.Core.dll  ClrPlus.Crypto.dll ClrPlus.Networking.dll ClrPlus.Platform.dll ClrPlus.Powershell.Core.dll ClrPlus.Powershell.Provider.dll  ClrPlus.Remoting.dll ClrPlus.Scripting.dll ClrPlus.Scripting.MsBuild.dll ClrPlus.Windows.Api.dll ClrPlus.Powershell.Azure.dll ClrPlus.Windows.Debugging.dll ClrPlus.Windows.PeBinary.dll Microsoft.Cci.MetadataHelper.dll Microsoft.Cci.MetadataModel.dll Microsoft.Cci.MutableMetadataModel.dll Microsoft.Cci.PeReader.dll Microsoft.Cci.PeWriter.dll Microsoft.Cci.SourceModel.dll Microsoft.WindowsAzure.Storage.dll System.Spatial.dll  || goto failed

popd

goto FIN

:failed
echo ===== ERROR =======

:FIN
