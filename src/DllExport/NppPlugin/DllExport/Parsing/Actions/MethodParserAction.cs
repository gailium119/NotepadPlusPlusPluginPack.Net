using System;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[ParserStateAction(ParserState.Method)]
	public sealed class MethodParserAction : IlParser.ParserStateAction
	{
		public override void Execute(ParserStateValues state, string trimmedLine)
		{
			if (trimmedLine.StartsWith("} // end of method", StringComparison.Ordinal))
			{
				state.State = ParserState.Class;
			}
		}
	}
}
