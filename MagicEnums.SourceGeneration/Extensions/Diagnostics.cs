using Microsoft.CodeAnalysis;

namespace CodeChops.MagicEnums.SourceGeneration.Extensions;

internal static class Diagnostics
{
	public static void ReportDiagnostic(this GeneratorExecutionContext context, string id, string title, string description, DiagnosticSeverity severity, ISymbol? symbol = null)
	{
		context.ReportDiagnostic(
			Diagnostic.Create(
				new DiagnosticDescriptor(id, title, description, "CodeChops", severity, isEnabledByDefault: true), symbol?.Locations.FirstOrDefault()));
	}
}