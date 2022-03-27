using System.Collections.Generic;
using System.Collections.Immutable;
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
               transform: GetTypeSymbols).Collect();
            
            context.RegisterSourceOutput(classDeclarations, GenerateSource);
        }

        private void GenerateSource(SourceProductionContext context, ImmutableArray<ITypeSymbol> typeSymbols)
        {
            var sb = new StringBuilder();
            foreach (var symbol in typeSymbols)
            {
                if (symbol is null)
                    continue;

                sb.AppendLine("// " + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            context.AddSource($"all_types.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private ITypeSymbol GetTypeSymbols(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var decl = (ClassDeclarationSyntax)context.Node;

            if (context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is ITypeSymbol typeSymbol)
            {
                return typeSymbol;
            }

            return null;
        }
    }
}

