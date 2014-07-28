@echo off

%~dp0\tools\GitVersion /proj %~dp0\src\ReactGraph.sln /updateAssemblyInfo /projargs "/property:configuration=Debug"

%~dp0\tools\xunit.console.clr4.x86.exe %~dp0\src\ReactGraph.Tests\bin\Debug\ReactGraph.Tests.dll