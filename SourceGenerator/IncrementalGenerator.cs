using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
	public class IncrementalGenerator : IIncrementalGenerator
	{
		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
            var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
               predicate: (s, t) => s is ClassDeclarationSyntax,
               transform: GetTypeSymbols);
            
            context.RegisterSourceOutput(classDeclarations, GenerateSource);
        }

        private void GenerateSource(SourceProductionContext context, IEnumerable<ITypeSymbol> typeSymbols)
        {
            var sb = new StringBuilder();
            foreach (var symbol in typeSymbols)
            {
                sb.AppendLine("// " + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            context.AddSource($"all_types.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private IEnumerable<ITypeSymbol> GetTypeSymbols(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var decl = (ClassDeclarationSyntax)context.Node;

            if (context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is ITypeSymbol typeSymbol)
            {
                yield return typeSymbol;
            }
        }
    }
}

