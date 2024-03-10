using System;
using System.Globalization;
using System.Text;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[ParserStateAction(ParserState.DeleteExportAttribute)]
	public sealed class DeleteExportAttributeParserAction : IlParser.ParserStateAction
	{
		public override void Execute(ParserStateValues state, string trimmedLine)
		{
			if (trimmedLine.StartsWith(".custom", StringComparison.Ordinal) || trimmedLine.StartsWith("// Code", StringComparison.Ordinal))
			{
				ExportedClass value;
				if (base.Exports.ClassesByName.TryGetValue(state.ClassNames.Peek(), out value))
				{
					ExportedMethod exportedMethod = value.MethodsByName[state.Method.Name][0];
					string declaration = state.Method.Declaration;
					StringBuilder stringBuilder = new StringBuilder(250);
					stringBuilder.Append(".method ").Append(state.Method.Attributes.NullSafeTrim()).Append(" ");
					stringBuilder.Append(state.Method.Result.NullSafeTrim());
					stringBuilder.Append(" modopt(['mscorlib']'").Append(AssemblyExports.ConventionTypeNames[exportedMethod.CallingConvention]).Append("') ");
					if (!string.IsNullOrEmpty(state.Method.ResultAttributes))
					{
						stringBuilder.Append(" ").Append(state.Method.ResultAttributes);
					}
					stringBuilder.Append(" '").Append(state.Method.Name).Append("'")
						.Append(state.Method.After.NullSafeTrim());
					bool flag = ValidateExportNameAndLogError(exportedMethod, state);
					if (flag)
					{
						state.Method.Declaration = stringBuilder.ToString();
					}
					if (state.MethodPos != 0)
					{
						state.Result.Insert(state.MethodPos, state.Method.Declaration);
					}
					if (flag)
					{
						base.Notifier.Notify(-2, DllExportLogginCodes.OldDeclaration, "\t" + Resources.OldDeclaration_0_, declaration);
						base.Notifier.Notify(-2, DllExportLogginCodes.NewDeclaration, "\t" + Resources.NewDeclaration_0_, state.Method.Declaration);
						state.Result.Add(string.Format(CultureInfo.InvariantCulture, "    .export [{0}] as '{1}'", exportedMethod.VTableOffset, exportedMethod.ExportName));
						base.Notifier.Notify(-1, DllExportLogginCodes.AddingVtEntry, "\t" + Resources.AddingVtEntry_0_export_1_, exportedMethod.VTableOffset, exportedMethod.ExportName);
					}
				}
				else
				{
					state.AddLine = false;
				}
				state.State = ParserState.Method;
			}
			else
			{
				state.AddLine = false;
			}
		}
	}
}
