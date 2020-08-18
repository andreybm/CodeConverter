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
