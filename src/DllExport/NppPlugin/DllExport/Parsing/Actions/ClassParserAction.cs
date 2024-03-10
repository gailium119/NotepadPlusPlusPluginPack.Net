using System;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[ParserStateAction(ParserState.Class)]
	public sealed class ClassParserAction : IlParser.ParserStateAction
	{
		public override void Execute(ParserStateValues state, string trimmedLine)
		{
			if (trimmedLine.StartsWith(".class", StringComparison.Ordinal))
			{
				state.State = ParserState.ClassDeclaration;
				state.AddLine = true;
				state.ClassDeclaration = trimmedLine;
			}
			else if (trimmedLine.StartsWith(".method", StringComparison.Ordinal))
			{
				ExportedClass value;
				if (state.ClassNames.Count != 0 && base.Parser.Exports.ClassesByName.TryGetValue(state.ClassNames.Peek(), out value))
				{
					state.Method.Reset();
					state.Method.Declaration = trimmedLine;
					state.AddLine = false;
					state.State = ParserState.MethodDeclaration;
				}
			}
			else if (trimmedLine.StartsWith("} // end of class", StringComparison.Ordinal))
			{
				state.ClassNames.Pop();
				state.State = ((state.ClassNames.Count > 0) ? ParserState.Class : ParserState.Normal);
			}
		}
	}
}
