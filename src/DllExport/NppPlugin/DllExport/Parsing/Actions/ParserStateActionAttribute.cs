using System;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal sealed class ParserStateActionAttribute : Attribute
	{
		public readonly ParserState ParserState;

		public ParserStateActionAttribute(ParserState parserState)
		{
			ParserState = parserState;
		}
	}
}
