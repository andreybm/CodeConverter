# Code Converter [![Build Status](https://icsharpcode.visualstudio.com/icsharpcode-pipelines/_apis/build/status/icsharpcode.CodeConverter?branchName=master)](https://icsharpcode.visualstudio.com/icsharpcode-pipelines/_build?definitionId=2&statusFilter=succeeded&repositoryFilter=2&branchFilter=32)

Esta es una guía para instalar el proyecto CodeConverter en el IDE Visual Studio.


Para agregar la extensión de Visual Studio a nuestro IDE, se debe de descargar desde el Visual Studio Market Place en el siguiente sitio web https://marketplace.visualstudio.com/items?itemName=SharpDevelopTeam.CodeConverter.


<p>
<img title="Selected text can be converted" alt="Selected text conversion context menu" src="https://github.com/andreybm/CodeConverter/blob/master/.github/img/vsmarketplace.PNG" />
</p>


Cabe destacar que es necesario tener instalado como mínimo versión 15.7 de Visual Studio 2017.

Posteriormente se abre la solución en Visual Studio 2017 o superior y se realizan las conversiones especificadas en el wiki.


## Ejecutar desde el codigo fuente
1. Asegurarse tener instalado[.NET Core SDK 3.1+](https://dotnet.microsoft.com/download/dotnet-core/3.1)
2. Abrir la solucion en Visual Studio 2017+
3. Para ejecutar el sitio web, elegir CodeConverter.Web como el archivo de proyecto de inicio.
4. Para ejecutar la extension de Visual Studio, elegir Vsix como el archivo de proyecto de inicio.
   * Una nueva instancia de Visual Studio abrira con la extension instalada. 

## Contributing
If you want to get involved in writing the code yourself, even better! We've already had code contributions from several first time GitHub contributors, so don't be shy! See [Contributing.md](https://github.com/icsharpcode/CodeConverter/blob/master/.github/CONTRIBUTING.md) for more info.

Currently, the VB -> C# conversion quality is higher than the C# -> VB conversion quality. This is due to demand of people raising issues and supply of developers willing to fix them. But we're very happy to support developers who want to contribute to either conversion direction. Visual Basic will have support for some project types on initial versions of .NET 5, but won't be getting new features according to the [.NET Team Blog](https://devblogs.microsoft.com/vbteam/visual-basic-support-planned-for-net-5-0/).

## Other ways to use the converter
* Latest CI build (potentially less stable):
  * [See latest build](https://icsharpcode.visualstudio.com/icsharpcode-pipelines/_build?definitionId=2&statusFilter=succeeded&repositoryFilter=2&branchFilter=32)
  * Uninstall current version, then install VSIX file inside "1 published" artifact
* Integrating the NuGet library
  * Check out the [CodeConversion class](https://github.com/icsharpcode/CodeConverter/blob/8226313a8d46d5dd73bd35f07af2212e6155d0fd/Vsix/CodeConversion.cs#L226) in the VSIX project.
  * Or check out the [ConverterController](https://github.com/icsharpcode/CodeConverter/blob/master/Web/ConverterController.cs) for a more web-focused API.

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
