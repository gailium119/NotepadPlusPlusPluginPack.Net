using System;

namespace NppPlugin.DllExport.Parsing.Actions
{
	internal static class IlParsingUtils
	{
		public class IlSnippetLocationBase
		{
			public string InputText { get; private set; }

			public string LastIdentifier { get; set; }

			public bool WithinString { get; private set; }

			public bool WithinScope { get; set; }

			public int NestedBrackets { get; private set; }

			public bool AtOuterBracket { get; private set; }

			protected IlSnippetLocationBase(string inputText, string lastIdentifier, bool withinString, bool withinScope, int nestedBrackets, bool atOuterBracket)
			{
				if (inputText == null)
				{
					throw new ArgumentNullException("inputText");
				}
				InputText = inputText;
				LastIdentifier = lastIdentifier;
				WithinString = withinString;
				WithinScope = withinScope;
				NestedBrackets = nestedBrackets;
				AtOuterBracket = atOuterBracket;
			}
		}

		public sealed class IlSnippetFinalizaton : IlSnippetLocationBase
		{
			public int LastPosition { get; private set; }

			public bool WasInterupted { get; private set; }

			public IlSnippetFinalizaton(string inputText, int lastPosition, bool wasInterupted, string lastIdentifier, bool withinString, bool withinScope, int nestedBrackets, bool atOuterBracket)
				: base(inputText, lastIdentifier, withinString, withinScope, nestedBrackets, atOuterBracket)
			{
				LastPosition = lastPosition;
				WasInterupted = wasInterupted;
			}
		}

		public class IlSnippetLocation : IlSnippetLocationBase
		{
			public int Index { get; private set; }

			public char CurrentChar { get; private set; }

			public IlSnippetLocation(string inputText, int index, char currentChar, string lastIdentifier, bool withinString, bool withinScope, int nestedBrackets, bool atOuterBracket)
				: base(inputText, lastIdentifier, withinString, withinScope, nestedBrackets, atOuterBracket)
			{
				Index = index;
				CurrentChar = currentChar;
			}
		}

		public static void ParseIlSnippet(string inputText, ParsingDirection direction, Func<IlSnippetLocation, bool> predicate, Action<IlSnippetFinalizaton> finalization = null)
		{
			bool flag = false;
			bool flag2 = false;
			bool atOuterBracket = false;
			int num = 0;
			int endIndex = inputText.Length - 1;
			bool wasInterupted = false;
			int num2 = -1;
			int num3 = ((direction == ParsingDirection.Forward) ? 1 : (-1));
			int num4 = ((direction != ParsingDirection.Forward) ? endIndex : 0);
			Func<int, bool> func = ((direction == ParsingDirection.Forward) ? ((Func<int, bool>)((int i) => i <= endIndex)) : ((Func<int, bool>)((int i) => i > -1)));
			int lastPosition = -1;
			char c = '\0';
			string lastIdentifier = null;
			for (int j = num4; func(j); j += num3)
			{
				char c2 = inputText[j];
				atOuterBracket = false;
				if (c2 == '\'')
				{
					flag = !flag;
					if (!flag && num2 > -1)
					{
						int num5 = j - num3;
						int startIndex = Math.Min(num5, num2);
						int num6 = Math.Max(num5, num2);
						lastIdentifier = ((num2 != num5) ? inputText.Substring(0, num6 + 1).Substring(startIndex) : "");
					}
					else
					{
						lastIdentifier = null;
					}
					if (flag && c == '[')
					{
						flag2 = true;
					}
				}
				else
				{
					if (flag && c == '\'')
					{
						num2 = j;
					}
					if (!flag)
					{
						num2 = -1;
						if (flag2 && c2 == ']')
						{
							flag2 = false;
						}
						switch (c2)
						{
						case ')':
							atOuterBracket = num == 0;
							num++;
							break;
						case '(':
							num--;
							atOuterBracket = num == 0;
							break;
						}
					}
				}
				if (!predicate(new IlSnippetLocation(inputText, j, c2, lastIdentifier, flag, flag2, num, atOuterBracket)))
				{
					wasInterupted = true;
					break;
				}
				lastPosition = j;
				c = c2;
			}
			if (finalization != null)
			{
				finalization(new IlSnippetFinalizaton(inputText, lastPosition, wasInterupted, lastIdentifier, flag, flag2, num, atOuterBracket));
			}
		}
	}
}
