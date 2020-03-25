﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.CodeConverter.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using LangVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using System;
using System.Xml.Linq;
using System.IO;
using System.Linq;

namespace ICSharpCode.CodeConverter.CSharp
{
    /// <remarks>
    /// Can be stateful, need a new one for each project
    /// </remarks>
    internal class VBToCSProjectContentsConverter : IProjectContentsConverter
    {
        private readonly ConversionOptions _conversionOptions;
        private CSharpCompilation _csharpViewOfVbSymbols;
        private Project _convertedCsProject;

        private Project _csharpReferenceProject;
        private readonly IProgress<ConversionProgress> _progress;
        private readonly CancellationToken _cancellationToken;

        public VBToCSProjectContentsConverter(ConversionOptions conversionOptions, IProgress<ConversionProgress> progress, CancellationToken cancellationToken)
        {
            _conversionOptions = conversionOptions;
            _progress = progress;
            _cancellationToken = cancellationToken;
        }

        public string RootNamespace => _conversionOptions.RootNamespaceOverride ??
                                       ((VisualBasicCompilationOptions)Project.CompilationOptions).RootNamespace;

        public async Task InitializeSourceAsync(Project project)
        {
            var cSharpCompilationOptions = CSharpCompiler.CreateCompilationOptions();
            _convertedCsProject = project.ToProjectFromAnyOptions(cSharpCompilationOptions, CSharpCompiler.ParseOptions);
            _csharpReferenceProject = project.CreateReferenceOnlyProjectFromAnyOptions(cSharpCompilationOptions, CSharpCompiler.ParseOptions);
            _csharpViewOfVbSymbols = (CSharpCompilation) await _csharpReferenceProject.GetCompilationAsync(_cancellationToken);
            Project = await project.WithRenamedMergedMyNamespace(_cancellationToken);
        }

        public string LanguageVersion { get { return LangVersion.Latest.ToDisplayString(); } }

        public Project Project { get; private set; }

        public async Task<SyntaxNode> SingleFirstPass(Document document)
        {
            return await VisualBasicConverter.ConvertCompilationTree(document, _csharpViewOfVbSymbols, _csharpReferenceProject, _cancellationToken);
        }

        public async Task<(Project project, List<WipFileConversion<DocumentId>> firstPassDocIds)> GetConvertedProject(WipFileConversion<SyntaxNode>[] firstPassResults)
        {
            var (project, docIds) = _convertedCsProject.WithDocuments(firstPassResults);
            return (await project.RenameMergedNamespaces(_cancellationToken), docIds);
        }

        public IEnumerable<ConversionResult> GetConversionResults(ConversionResult result)
        {
            if (DesignerWithResx.TryCreate(Project.GetDirectoryPath(), result.SourcePathOrNull) is DesignerWithResx d) {
                result.TargetPathOrNull = d.TargetDesignerPath;
                if (d.SourceResxPath != d.TargetResxPath) {
                    yield return new ConversionResult(RebaseResxPaths(d.SourceResxPath)) { SourcePathOrNull = d.SourceResxPath, TargetPathOrNull = d.TargetResxPath };
                }
            }
            yield return result;
        }

        private static string RebaseResxPaths(string oldResxPath)
        {
            var original = File.ReadAllText(oldResxPath);
            return original.Replace(@"<value>..\", "<value>");
        }
    }
}