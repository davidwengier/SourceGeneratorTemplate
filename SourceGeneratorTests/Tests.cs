using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerator;
using Xunit;

namespace SourceGeneratorTests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            string source = @"
namespace Foo
{
    class C
    {
        void M()
        {
        }
    }
}";
            string output = GetGeneratedOutput(source);

            Assert.NotNull(output);

            Assert.Equal("class Foo { }", output);
        }

        private static string GetGeneratedOutput(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic)
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
            Assert.False(diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + diagnostics.FirstOrDefault()?.GetMessage());

            var generator = new Generator();

            var output = generator.GetGeneratedSource(compilation);
            return output;
        }
    }
}
