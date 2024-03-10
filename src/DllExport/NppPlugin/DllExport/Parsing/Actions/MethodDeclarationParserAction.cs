using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport.Parsing.Actions
{
	[ParserStateAction(ParserState.MethodDeclaration)]
	public sealed class MethodDeclarationParserAction : IlParser.ParserStateAction
	{
		private static readonly Regex _CilManagedRegex = new Regex("\\b(?:cil|managed)\\b", RegexOptions.Compiled);

		public override void Execute(ParserStateValues state, string trimmedLine)
		{
			if (trimmedLine.StartsWith("{", StringComparison.Ordinal))
			{
				ExportedClass exportedClass;
				if (GetIsExport(state, out exportedClass))
				{
					Notify(-1, DllExportLogginCodes.MethodFound, string.Format(Resources.Found_method_0_1_, exportedClass.FullTypeName, state.Method.Declaration));
					state.MethodPos = state.Result.Count;
					state.State = ParserState.MethodProperties;
				}
				else
				{
					state.Result.Add(state.Method.Declaration);
					state.State = ParserState.Method;
					state.MethodPos = 0;
				}
			}
			else
			{
				state.Method.Declaration = state.Method.Declaration + " " + trimmedLine;
				state.AddLine = false;
			}
		}

		private bool GetPartBeforeParameters(string line, out string methodName, out string afterMethodName, out string foundResult, out string foundResultModifier, out string foundMethodAttributes)
		{
			methodName = null;
			foundResult = null;
			foundResultModifier = null;
			afterMethodName = null;
			foundMethodAttributes = null;
			line = line.TrimStart();
			if (!line.StartsWith(".method"))
			{
				return false;
			}
			line = line.Substring(".method".Length).TrimStart();
			StringBuilder afterMethodNameBuilder = new StringBuilder(line.Length);
			string result = null;
			IlParsingUtils.ParseIlSnippet(line, ParsingDirection.Backward, delegate(IlParsingUtils.IlSnippetLocation s)
			{
				if (!s.WithinString && s.AtOuterBracket)
				{
					if (s.CurrentChar != ')')
					{
						result = line.Substring(0, s.Index);
						afterMethodNameBuilder.Insert(0, s.CurrentChar);
						return false;
					}
					RemoveCilManagedFromMethodSuffix(ref afterMethodNameBuilder);
				}
				afterMethodNameBuilder.Insert(0, s.CurrentChar);
				return true;
			});
			if (afterMethodNameBuilder.Length > 0)
			{
				afterMethodName = afterMethodNameBuilder.ToString();
			}
			if (result != null)
			{
				string attributesWithResult;
				methodName = ExtractMethodName(result, out attributesWithResult);
				if (SplitAttributesAndResult(attributesWithResult, out foundResult, out foundMethodAttributes))
				{
					if (foundResult != null && foundResult.Contains("("))
					{
						ExtractResultModifier(ref foundResult, out foundResultModifier);
					}
					return true;
				}
			}
			return false;
		}

		private static void ExtractResultModifier(ref string foundResult, out string foundResultModifier)
		{
			int bracketEnd = -1;
			string localFoundResult = foundResult;
			string localfoundResultModifier = null;
			IlParsingUtils.ParseIlSnippet(foundResult, ParsingDirection.Backward, delegate(IlParsingUtils.IlSnippetLocation s)
			{
				if (s.WithinString || !s.AtOuterBracket)
				{
					return true;
				}
				if (s.CurrentChar == ')')
				{
					bracketEnd = s.Index;
					return true;
				}
				string text = s.InputText.Substring(0, s.Index);
				int num = text.LastIndexOf(' ');
				if (num > -1 && text.Substring(num + 1) == "marshal")
				{
					localfoundResultModifier = s.InputText.Substring(num + 1, bracketEnd - num);
					localFoundResult = s.InputText.Remove(num + 1, bracketEnd - num);
				}
				return true;
			});
			foundResult = localFoundResult;
			foundResultModifier = localfoundResultModifier;
		}

		private static bool RemoveCilManagedFromMethodSuffix(ref StringBuilder afterMethodNameBuilder)
		{
			bool cilManagedFound = false;
			string value = _CilManagedRegex.Replace(afterMethodNameBuilder.ToString(), delegate
			{
				cilManagedFound = true;
				return "";
			});
			if (cilManagedFound)
			{
				afterMethodNameBuilder = new StringBuilder(value, afterMethodNameBuilder.Capacity);
			}
			return cilManagedFound;
		}

		private bool SplitAttributesAndResult(string attributesWithResult, out string foundResult, out string foundMethodAttributes)
		{
			if (string.IsNullOrEmpty(attributesWithResult))
			{
				foundResult = null;
				foundMethodAttributes = null;
				return false;
			}
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < attributesWithResult.Length; i++)
			{
				char c = attributesWithResult[i];
				if (c == '\'')
				{
					num2 = i;
					break;
				}
				if (char.IsWhiteSpace(c) || i == attributesWithResult.Length - 1)
				{
					if (num > -1)
					{
						string item = attributesWithResult.Substring(num, i - num).Trim();
						if (!base.Parser.MethodAttributes.Contains(item))
						{
							num2 = num;
							break;
						}
						num = i + 1;
					}
				}
				else if (num < 0)
				{
					num = i;
				}
			}
			if (num2 > -1)
			{
				foundMethodAttributes = attributesWithResult.Substring(0, num2).Trim();
				foundResult = attributesWithResult.Substring(num2).Trim();
				return true;
			}
			foundResult = null;
			foundMethodAttributes = null;
			return false;
		}

		private static string ExtractMethodName(string result, out string attributesWithResult)
		{
			string text = null;
			string localAttributesWithResult = null;
			StringBuilder methodNameBuilder = new StringBuilder(result.Length);
			IlParsingUtils.ParseIlSnippet(result, ParsingDirection.Backward, delegate(IlParsingUtils.IlSnippetLocation s)
			{
				if (s.CurrentChar == '\'')
				{
					return true;
				}
				if (!s.WithinString && s.CurrentChar != '.' && s.CurrentChar != ',' && s.CurrentChar != '/' && s.CurrentChar != '<' && s.CurrentChar != '>' && s.CurrentChar != '!')
				{
					return false;
				}
				methodNameBuilder.Insert(0, s.CurrentChar);
				return true;
			}, delegate(IlParsingUtils.IlSnippetFinalizaton f)
			{
				localAttributesWithResult = ((f.LastPosition > -1) ? result.Substring(0, f.LastPosition) : null);
			});
			text = methodNameBuilder.ToString();
			attributesWithResult = localAttributesWithResult;
			return text;
		}

		private static StringBuilder OldExtractMethodName(string result, out string attributesWithResult)
		{
			StringBuilder stringBuilder = new StringBuilder(result.Length);
			bool flag = false;
			int num = -1;
			for (int num2 = result.Length - 1; num2 > -1; num2--)
			{
				char c = result[num2];
				if (c == '\'')
				{
					flag = !flag;
				}
				else
				{
					if (!flag && c != '.' && c != ',' && c != '/' && c != '<' && c != '>' && c != '!')
					{
						break;
					}
					stringBuilder.Insert(0, c);
				}
				num = num2;
			}
			attributesWithResult = ((num > -1) ? result.Substring(0, num) : null);
			return stringBuilder;
		}

		private bool GetIsExport(ParserStateValues state, out ExportedClass exportedClass)
		{
			if (!ExtractMethodParts(state))
			{
				exportedClass = null;
				return false;
			}
			bool flag = base.Exports.ClassesByName.TryGetValue(state.ClassNames.Peek(), out exportedClass) && exportedClass != null;
			List<ExportedMethod> value = null;
			if (flag && exportedClass.HasGenericContext)
			{
				if (exportedClass.MethodsByName.TryGetValue(state.Method.Name, out value))
				{
					value.ForEach(delegate(ExportedMethod method)
					{
						Notify(2, DllExportLogginCodes.ExportInGenericType, Resources.The_type_1_cannot_export_the_method_2_as_0_because_it_is_generic_or_is_nested_within_a_generic_type, method.ExportName, method.ExportedClass.FullTypeName, method.MemberName);
					});
				}
				return false;
			}
			bool flag2 = flag && exportedClass.MethodsByName.TryGetValue(state.Method.Name, out value);
			if (flag && !flag2)
			{
				ExportedMethod duplicateExport = base.Exports.GetDuplicateExport(exportedClass.FullTypeName, state.Method.Name);
				ValidateExportNameAndLogError(duplicateExport, state);
				if (duplicateExport != null)
				{
					Notify(state, 1, DllExportLogginCodes.AmbigiguousExportName, Resources.Ambiguous_export_name_0_on_1_2_, duplicateExport.ExportName, duplicateExport.ExportedClass.FullTypeName, duplicateExport.MemberName);
				}
			}
			else
			{
				List<ExportedMethod> obj = value ?? exportedClass.NullSafeCall((ExportedClass i) => i.Methods);
				value = obj;
				if (obj != null)
				{
					foreach (ExportedMethod item in value)
					{
						if (!item.IsStatic)
						{
							flag2 = false;
							Notify(state, 2, DllExportLogginCodes.MethodIsNotStatic, Resources.The_method_1_2_is_not_static_export_name_0_, item.ExportName, item.ExportedClass.FullTypeName, item.MemberName);
						}
						if (item.IsGeneric)
						{
							flag2 = false;
							Notify(state, 2, DllExportLogginCodes.ExportOnGenericMethod, Resources.The_method_1_2_is_generic_export_name_0_Generic_methods_cannot_be_exported_, item.ExportName, item.ExportedClass.FullTypeName, item.MemberName);
						}
					}
					return flag2;
				}
			}
			return flag2;
		}

		private bool ExtractMethodParts(ParserStateValues state)
		{
			string methodName;
			string afterMethodName;
			string foundResult;
			string foundResultModifier;
			string foundMethodAttributes;
			if (GetPartBeforeParameters(state.Method.Declaration, out methodName, out afterMethodName, out foundResult, out foundResultModifier, out foundMethodAttributes))
			{
				state.Method.After = afterMethodName;
				state.Method.Name = methodName;
				state.Method.Attributes = foundMethodAttributes;
				state.Method.Result = foundResult;
				state.Method.ResultAttributes = foundResultModifier;
				return true;
			}
			return false;
		}
	}
}
