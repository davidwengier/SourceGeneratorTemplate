using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retreive the populated receiver 
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;

            var compilation = context.Compilation;

            // loop over the candidate fields, and keep the ones that are actually annotated
            var symbols = new List<ITypeSymbol>();
            foreach (var decl in receiver.ClassDeclarations)
            {
                var model = compilation.GetSemanticModel(decl.SyntaxTree);
                if (model.GetDeclaredSymbol(decl, context.CancellationToken) is ITypeSymbol symbol)
                {
                    symbols.Add(symbol);
                }
            }


            var sb = new StringBuilder();
            foreach (var symbol in symbols)
            {
                sb.AppendLine("// " + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            context.AddSource($"all_types.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> ClassDeclarations { get; } = new List<ClassDeclarationSyntax>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any field with at least one attribute is a candidate for property generation
            if (syntaxNode is ClassDeclarationSyntax decl)
            {
                ClassDeclarations.Add(decl);
            }
        }
    }
}
