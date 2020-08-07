using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
        }

        public void Execute(SourceGeneratorContext context)
        {
            var source = GetGeneratedSource(context.Compilation);
            if (source != null)
            {
                context.AddSource("generated.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        public string GetGeneratedSource(Compilation compilation)
        {
            return "class Foo { }";
        }
    }
}
