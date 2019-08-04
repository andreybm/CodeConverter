﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.CodeConverter.CSharp;
using ICSharpCode.CodeConverter.Util;
using ICSharpCode.CodeConverter.VB;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;

namespace ICSharpCode.CodeConverter.Shared
{
    public class ProjectConversion
    {
        private readonly Compilation _sourceCompilation;
        private readonly IEnumerable<SyntaxTree> _syntaxTreesToConvert;
        private readonly ConcurrentDictionary<string, string> _errors = new ConcurrentDictionary<string, string>();
        private readonly Dictionary<string, Document> _firstPassResults = new Dictionary<string, Document>();
        private readonly ILanguageConversion _languageConversion;
        private readonly Project _project;
        private readonly bool _showCompilationErrors =
#if DEBUG && ShowCompilationErrors
            true;
#else
            false;

        private readonly bool _returnSelectedNode;
#endif

        private ProjectConversion(Compilation sourceCompilation, IEnumerable<SyntaxTree> syntaxTreesToConvert,
            ILanguageConversion languageConversion, Compilation convertedCompilation, Project project = null, bool returnSelectedNode = false)
        {
            _languageConversion = languageConversion;
            _project = project;
            _sourceCompilation = sourceCompilation;
            _syntaxTreesToConvert = syntaxTreesToConvert.ToList();
            _returnSelectedNode = returnSelectedNode;
            languageConversion.Initialize(convertedCompilation.RemoveAllSyntaxTrees(), project).GetAwaiter().GetResult();
        }

        public static Task<ConversionResult> ConvertText<TLanguageConversion>(string text, IReadOnlyCollection<PortableExecutableReference> references, string rootNamespace = null) where TLanguageConversion : ILanguageConversion, new()
        {
            var languageConversion = new TLanguageConversion {
                RootNamespace = rootNamespace
            };
            var syntaxTree = languageConversion.MakeFullCompilationUnit(text);
            var compilation = languageConversion.CreateCompilationFromTree(syntaxTree, references);
            return ConvertSingle(compilation, syntaxTree, new TextSpan(0, 0), new TLanguageConversion());
        }

        /// <summary>
        /// If the compilation comes from a Project/Workspace, you must specify the <paramref name="containingProject"/>.
        /// Otherwise an error will occur when one or more project references to another project of the same language exist.
        /// </summary>
        public static async Task<ConversionResult> ConvertSingle(Compilation compilation, SyntaxTree syntaxTree, TextSpan selected,
            ILanguageConversion languageConversion, Project containingProject = null)
        {
            var convertedCompilation = containingProject == null
                ? GetConvertedCompilation(compilation, languageConversion)
                : await GetConvertedCompilationWithProjectReferences(containingProject, languageConversion);

            if (selected.Length > 0) {
                var annotatedSyntaxTree = await GetSyntaxTreeWithAnnotatedSelection(syntaxTree, selected);
                compilation = compilation.ReplaceSyntaxTree(syntaxTree, annotatedSyntaxTree);
                syntaxTree = annotatedSyntaxTree;
            }

            var conversion = new ProjectConversion(compilation, new[] {syntaxTree}, languageConversion, convertedCompilation, returnSelectedNode: true);
            var conversionResults = (await ConvertProjectContents(conversion)).ToList();
            var codeResult = conversionResults.SingleOrDefault(x => !string.IsNullOrWhiteSpace(x.ConvertedCode))
                             ?? conversionResults.First();
            codeResult.Exceptions = conversionResults.SelectMany(x => x.Exceptions).ToArray();
            return codeResult;
        }

        public static async Task<IEnumerable<ConversionResult>> ConvertProject(Project project, ILanguageConversion languageConversion,
            params (string, string)[] replacements)
        {
            return (await ConvertProjectContents(project, languageConversion)).Concat(new[]
                {ConvertProjectFile(project, languageConversion, replacements)}
            );
        }

        public static async Task<IEnumerable<ConversionResult>> ConvertProjectContents(Project project,
            ILanguageConversion languageConversion)
        {
            var solutionFilePath = project.Solution.FilePath ?? project.FilePath;
            var solutionDir = Path.GetDirectoryName(solutionFilePath);
            var compilation = await project.GetCompilationAsync();
            var syntaxTreesToConvert = compilation.SyntaxTrees.Where(t => t.FilePath.StartsWith(solutionDir));
            var projectConversion = new ProjectConversion(compilation, syntaxTreesToConvert,
                languageConversion, await GetConvertedCompilationWithProjectReferences(project, languageConversion), project);
            return await ConvertProjectContents(projectConversion);
        }

        public static ConversionResult ConvertProjectFile(Project project, ILanguageConversion languageConversion, params (string, string)[] textReplacements)
        {
            return new FileInfo(project.FilePath).ConversionResultFromReplacements(textReplacements, languageConversion.PostTransformProjectFile);
        }

        /// <summary>
        /// If the source compilation has project references to a compilation of the same language, this will fail with an argument exception.
        /// Use <see cref="GetConvertedCompilationWithProjectReferences"/> wherever this is possible.
        /// </summary>
        private static Compilation GetConvertedCompilation(Compilation compilation, ILanguageConversion languageConversion)
        {
            var convertedCompilation = languageConversion is VBToCSConversion ? CSharpCompiler.CreateCSharpCompilation(compilation.References) : (Compilation) VisualBasicCompiler.CreateVisualBasicCompilation(compilation.References);
            return convertedCompilation;
        }

        private static Task<Compilation> GetConvertedCompilationWithProjectReferences(Project project, ILanguageConversion languageConversion)
        {
            return project.Solution.RemoveProject(project.Id)
                .AddProject(project.Id, project.Name, project.AssemblyName, languageConversion.TargetLanguage)
                .GetProject(project.Id)
                .WithProjectReferences(project.AllProjectReferences).WithMetadataReferences(project.MetadataReferences)
                .GetCompilationAsync();
        }

        private static async Task<IEnumerable<ConversionResult>> ConvertProjectContents(ProjectConversion projectConversion)
        {
            using (var adhocWorkspace = new AdhocWorkspace()) {
                var pathNodePairs = await Task.WhenAll(projectConversion.Convert(adhocWorkspace));
                var results = pathNodePairs.Select(pathNodePair => {
                    var errors = projectConversion._errors.TryRemove(pathNodePair.Path, out var nonFatalException)
                        ? new[] {nonFatalException}
                        : new string[0];
                    return new ConversionResult(pathNodePair.Node.ToFullString())
                        {SourcePathOrNull = pathNodePair.Path, Exceptions = errors};
                });

                projectConversion.AddProjectWarnings();

                return results.Concat(projectConversion._errors
                    .Select(error => new ConversionResult {SourcePathOrNull = error.Key, Exceptions = new[] {error.Value}})
                );
            }
        }

        private IEnumerable<Task<(string Path, SyntaxNode Node)>> Convert(AdhocWorkspace adhocWorkspace)
        {
            FirstPass();
            return SecondPass(adhocWorkspace);
        }

        private IEnumerable<Task<(string Path, SyntaxNode Node)>> SecondPass(AdhocWorkspace workspace)
        {
            foreach (var firstPassResult in _firstPassResults) {
                yield return SingleSecondPassHandled(workspace, firstPassResult);
            }
        }

        private async Task<(string Key, SyntaxNode singleSecondPass)> SingleSecondPassHandled(AdhocWorkspace workspace, KeyValuePair<string, Document> firstPassResult)
        {
            try {
                var singleSecondPass = await SingleSecondPass(firstPassResult, workspace);
                return (firstPassResult.Key, singleSecondPass);
            }
            catch (Exception e)
            {
                var formatted = await Format(await firstPassResult.Value.GetSyntaxRootAsync(), workspace);
                _errors.TryAdd(firstPassResult.Key, e.ToString());
                return (firstPassResult.Key, formatted);
            }
        }

        private void AddProjectWarnings()
        {
            if (!_showCompilationErrors) return;

            var nonFatalWarningsOrNull = _languageConversion.GetWarningsOrNull();
            if (!string.IsNullOrWhiteSpace(nonFatalWarningsOrNull))
            {
                var warningsDescription = Path.Combine(_sourceCompilation.AssemblyName, "ConversionWarnings.txt");
                _errors.TryAdd(warningsDescription, nonFatalWarningsOrNull);
            }
        }

        private async Task<SyntaxNode> SingleSecondPass(KeyValuePair<string, Document> cs, AdhocWorkspace workspace)
        {
            var secondPassNode = await _languageConversion.SingleSecondPass(cs);
            return await Format(secondPassNode, workspace);
        }

        private void FirstPass()
        {
            foreach (var tree in _syntaxTreesToConvert)
            {
                var treeFilePath = tree.FilePath ?? "";
                try {
                    SingleFirstPass(tree, treeFilePath);
                    var errorAnnotations = tree.GetRoot().GetAnnotations(AnnotationConstants.ConversionErrorAnnotationKind).ToList();
                    if (errorAnnotations.Any()) {
                        _errors.TryAdd(treeFilePath,
                            string.Join(Environment.NewLine, errorAnnotations.Select(a => a.Data))
                        );
                    }
                }
                catch (Exception e)
                {
                    _errors.TryAdd(treeFilePath, e.ToString());
                }
            }
        }

        private void SingleFirstPass(SyntaxTree tree, string treeFilePath)
        {
            var currentSourceCompilation = this._sourceCompilation;
            var convertedTree = _languageConversion.SingleFirstPass(currentSourceCompilation, tree);
            _firstPassResults.Add(treeFilePath, convertedTree);
        }

        private static async Task<SyntaxTree> GetSyntaxTreeWithAnnotatedSelection(SyntaxTree syntaxTree, TextSpan selected)
        {
            var root = await syntaxTree.GetRootAsync();
            var selectedNode = root.FindNode(selected);
            return root.WithAnnotatedNode(selectedNode, AnnotationConstants.SelectedNodeAnnotationKind);
        }

        private async Task<SyntaxNode> Format(SyntaxNode resultNode, Workspace workspace)
        {
            SyntaxNode selectedNode = _returnSelectedNode ? GetSelectedNode(resultNode) : resultNode;
            SyntaxNode nodeToFormat = selectedNode ?? resultNode;
            return Formatter.Format(nodeToFormat, workspace);
        }

        private SyntaxNode GetSelectedNode(SyntaxNode resultNode)
        {
            var selectedNode = resultNode.GetAnnotatedNodes(AnnotationConstants.SelectedNodeAnnotationKind)
                .FirstOrDefault();
            if (selectedNode != null)
            {
                var children = _languageConversion.FindSingleImportantChild(selectedNode);
                if (selectedNode.GetAnnotations(AnnotationConstants.SelectedNodeAnnotationKind)
                        .Any(n => n.Data == AnnotationConstants.AnnotatedNodeIsParentData)
                    && children.Count == 1)
                {
                    selectedNode = children.Single();
                }
            }

            return selectedNode ?? resultNode;
        }
    }
}