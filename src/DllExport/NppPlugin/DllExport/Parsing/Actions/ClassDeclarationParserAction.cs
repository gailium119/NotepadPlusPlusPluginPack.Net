using System.Text;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[ParserStateAction(ParserState.ClassDeclaration)]
	public sealed class ClassDeclarationParserAction : IlParser.ParserStateAction
	{
		public override void Execute(ParserStateValues state, string trimmedLine)
		{
			if (trimmedLine.StartsWith("{"))
			{
				state.State = ParserState.Class;
				string text = GetClassName(state);
				if (state.ClassNames.Count > 0)
				{
					text = state.ClassNames.Peek() + "/" + text;
				}
				state.ClassNames.Push(text);
			}
			else
			{
				state.ClassDeclaration = state.ClassDeclaration + " " + trimmedLine;
				state.AddLine = true;
			}
		}

		private static string GetClassName(ParserStateValues state)
		{
			bool hadClassName = false;
			StringBuilder classNameBuilder = new StringBuilder(state.ClassDeclaration.Length);
			IlParsingUtils.ParseIlSnippet(state.ClassDeclaration, ParsingDirection.Forward, delegate(IlParsingUtils.IlSnippetLocation s)
			{
				if (s.WithinString)
				{
					hadClassName = true;
					if (s.CurrentChar != '\'')
					{
						classNameBuilder.Append(s.CurrentChar);
					}
				}
				else if (hadClassName)
				{
					if (s.CurrentChar == '.' || s.CurrentChar == '/')
					{
						classNameBuilder.Append(s.CurrentChar);
					}
					else if (s.CurrentChar != '\'')
					{
						return false;
					}
				}
				return true;
			});
			return classNameBuilder.ToString();
		}
	}
}
