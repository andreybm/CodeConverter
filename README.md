# Code Converter [![Build Status](https://icsharpcode.visualstudio.com/icsharpcode-pipelines/_apis/build/status/icsharpcode.CodeConverter?branchName=master)](https://icsharpcode.visualstudio.com/icsharpcode-pipelines/_build?definitionId=2&statusFilter=succeeded&repositoryFilter=2&branchFilter=32)


Convert code from VB.NET to C# and vice versa using Roslyn - all free and open source:
* [Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=SharpDevelopTeam.CodeConverter)
* Command line `dotnet tool install ICSharpCode.CodeConverter.codeconv --global`
* [Online snippet converter](https://codeconverter.icsharpcode.net/)

Esta es una guía para instalar el proyecto \textit{CodeConverter} en el IDE Visual Studio.


Para agregar la extensión de Visual Studio a nuestro IDE, se debe de descargar desde el Visual Studio Market Place en el siguiente sitio web \{https://marketplace.visualstudio.com/items?itemName=SharpDevelopTeam.CodeConverter} como se muestra en la siguiente imagen:

<p>
<img title="Selected text can be converted" alt="Selected text conversion context menu" src="https://github.com/icsharpcode/CodeConverter/raw/master/.github/img/vbToCsSelection.png" />
</p>


Cabe destacar que es necesario tener instalado como mínimo versión 15.7} de Visual Studio 2017.


Posteriormente se abre la solución en Visual Studio 2017 o superior y se realizan las conversiones especificadas.


## Building/running from source
1. Ensure you have [.NET Core SDK 3.1+](https://dotnet.microsoft.com/download/dotnet-core/3.1)
2. Open the solution in Visual Studio 2017+
3. To run the website, set CodeConverter.Web as the startup project
4. To run the Visual Studio extension, set Vsix as the startup project
   * A new instance of Visual Studio will open with the extension installed

##  History
A spiritual successor of the code conversion within [SharpDevelop](https://github.com/icsharpcode/SharpDevelop) and later part of [Refactoring Essentials](https://github.com/icsharpcode/RefactoringEssentials), the code converter was separated out to avoid difficulties with different Visual Studio and Roslyn versions.

## More screenshots
<p float="left">
  <img src="https://github.com/icsharpcode/CodeConverter/raw/master/.github/img/solution.png" width="49%" />
  <img src="https://github.com/icsharpcode/CodeConverter/raw/master/.github/img/vbToCsFile.png" width="49%" /> 
  <img src="https://github.com/icsharpcode/CodeConverter/raw/master/.github/img/vbToCsProject.png" width="49%" /> 
  <img src="https://github.com/icsharpcode/CodeConverter/raw/master/.github/img/csToVbProject.png" width="49%" /> 
</p>
