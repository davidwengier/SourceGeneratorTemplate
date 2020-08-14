using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerator;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
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

            var (diagnostics, output) = GetGeneratedOutput(source);

            if (diagnostics.Length > 0)
            {
                Console.WriteLine("Diagnostics:");
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine("   " + diag.ToString());
                }
                Console.WriteLine();
                Console.WriteLine("Output:");
            }

            Console.WriteLine(output);
        }

        private static (ImmutableArray<Diagnostic>, string) GetGeneratedOutput(string source)
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

            ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

            var generator = new Generator();

            var output = generator.GetGeneratedSource(compilation);

            return (diagnostics, output);
        }
    }
}
