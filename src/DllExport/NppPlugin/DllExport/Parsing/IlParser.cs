using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NppPlugin.DllExport.Parsing.Actions;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport.Parsing
{
	public sealed class IlParser : HasServiceProvider
	{
		public abstract class ParserStateAction : IParserStateAction
		{
			protected string DllExportAttributeAssemblyName
			{
				get
				{
					return Parser.DllExportAttributeAssemblyName;
				}
			}

			protected string DllExportAttributeFullName
			{
				get
				{
					return Parser.DllExportAttributeFullName;
				}
			}

			protected IDllExportNotifier Notifier
			{
				get
				{
					return Parser.GetNotifier();
				}
			}

			protected AssemblyExports Exports
			{
				get
				{
					return Parser.Exports;
				}
			}

			public long Milliseconds { get; set; }

			public IlParser Parser { get; set; }

			public abstract void Execute(ParserStateValues state, string trimmedLine);

			protected void Notify(int severity, string code, string message, params object[] values)
			{
				Notify(null, severity, code, message, values);
			}

			protected void Notify(ParserStateValues stateValues, int severity, string code, string message, params object[] values)
			{
				SourceCodeRange range;
				if (stateValues != null && (range = stateValues.GetRange()) != null)
				{
					Notifier.Notify(severity, code, range.FileName, range.StartPosition, range.EndPosition, message, values);
				}
				else
				{
					Notifier.Notify(severity, code, message, values);
				}
			}

			protected bool ValidateExportNameAndLogError(ExportedMethod exportMethod, ParserStateValues stateValues)
			{
				if (exportMethod == null)
				{
					return false;
				}
				if (exportMethod.ExportName != null && (exportMethod.ExportName.Contains("'") || Regex.IsMatch(exportMethod.ExportName, "\\P{IsBasicLatin}")))
				{
					Notify(stateValues, 3, DllExportLogginCodes.ExportNamesHaveToBeBasicLatin, Resources.Export_name_0_on_1__2_is_Unicode_windows_export_names_have_to_be_basic_latin, exportMethod.ExportName, exportMethod.ExportedClass.FullTypeName, exportMethod.MemberName);
					return false;
				}
				return true;
			}

			public static Dictionary<ParserState, IParserStateAction> GetActionsByState(IlParser parser)
			{
				Dictionary<ParserState, IParserStateAction> dictionary = new Dictionary<ParserState, IParserStateAction>();
				dictionary.Add(ParserState.ClassDeclaration, new ClassDeclarationParserAction());
				dictionary.Add(ParserState.Class, new ClassParserAction());
				dictionary.Add(ParserState.DeleteExportAttribute, new DeleteExportAttributeParserAction());
				dictionary.Add(ParserState.MethodDeclaration, new MethodDeclarationParserAction());
				dictionary.Add(ParserState.Method, new MethodParserAction());
				dictionary.Add(ParserState.MethodProperties, new MethodPropertiesParserAction());
				dictionary.Add(ParserState.Normal, new NormalParserAction());
				Dictionary<ParserState, IParserStateAction> dictionary2 = dictionary;
				foreach (IParserStateAction value in dictionary2.Values)
				{
					value.Parser = parser;
				}
				return dictionary2;
			}
		}

		private HashSet<string> _MethodAttributes;

		[Localizable(false)]
		private static readonly string[] _DefaultMethodAttributes = new string[20]
		{
			"static", "public", "private", "family", "final", "specialname", "virtual", "abstract", "assembly", "famandassem",
			"famorassem", "privatescope", "hidebysig", "newslot", "strict", "rtspecialname", "flags", "unmanagedexp", "reqsecobj", "pinvokeimpl"
		};

		public string DllExportAttributeAssemblyName
		{
			get
			{
				return Exports.DllExportAttributeAssemblyName;
			}
		}

		public string DllExportAttributeFullName
		{
			get
			{
				return Exports.DllExportAttributeFullName;
			}
		}

		public string DllExportAttributeIlAsmFullName
		{
			get
			{
				string text = Exports.DllExportAttributeFullName;
				if (!string.IsNullOrEmpty(text))
				{
					int num = text.LastIndexOf('.');
					text = ((num <= -1) ? ("'" + text + "'") : ("'" + text.Substring(0, num) + "'.'" + text.Substring(num + 1) + "'"));
				}
				return text;
			}
		}

		public IInputValues InputValues { get; set; }

		public AssemblyExports Exports { get; set; }

		public string TempDirectory { get; set; }

		public bool ProfileActions { get; set; }

		public HashSet<string> MethodAttributes
		{
			get
			{
				lock (this)
				{
					return _MethodAttributes ?? (_MethodAttributes = GetMethodAttributes());
				}
			}
		}

		public IlParser(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public IEnumerable<string> GetLines(CpuPlatform cpu)
		{
			using (GetNotifier().CreateContextName(this, Resources.ParseILContextName))
			{
				Dictionary<ParserState, IParserStateAction> actionsByState = ParserStateAction.GetActionsByState(this);
				List<string> list = new List<string>(1000000);
				ParserStateValues state = new ParserStateValues(cpu, list)
				{
					State = ParserState.Normal
				};
				Stopwatch stopwatch = Stopwatch.StartNew();
				using (FileStream stream = new FileStream(Path.Combine(TempDirectory, InputValues.FileName + ".il"), FileMode.Open))
				{
					using (StreamReader streamReader = new StreamReader(stream, Encoding.Unicode))
					{
						while (!streamReader.EndOfStream)
						{
							list.Add(streamReader.ReadLine());
						}
					}
				}
				Action<IParserStateAction, string> action2 = delegate(IParserStateAction action, string trimmedLine)
				{
					string name = action.GetType().Name;
					using (GetNotifier().CreateContextName(action, name))
					{
						action.Execute(state, trimmedLine);
					}
				};
				if (ProfileActions)
				{
					Action<IParserStateAction, string> executeActionCore = action2;
					action2 = delegate(IParserStateAction action, string trimmedLine)
					{
						Stopwatch stopwatch2 = Stopwatch.StartNew();
						executeActionCore(action, trimmedLine);
						stopwatch2.Stop();
						action.Milliseconds += stopwatch2.ElapsedMilliseconds;
					};
				}
				Dictionary<string, int> usedScopeNames = new Dictionary<string, int>();
				for (int i = 0; i < list.Count; i++)
				{
					state.InputPosition = i;
					string text = list[i];
					IlParsingUtils.ParseIlSnippet(text, ParsingDirection.Forward, delegate(IlParsingUtils.IlSnippetLocation current)
					{
						if (!current.WithinString && current.CurrentChar == ']' && current.LastIdentifier != null && !usedScopeNames.ContainsKey(current.LastIdentifier))
						{
							usedScopeNames.Add(current.LastIdentifier, usedScopeNames.Count);
						}
						return true;
					});
					string arg = text.NullSafeTrim();
					state.AddLine = true;
					IParserStateAction value;
					if (!actionsByState.TryGetValue(state.State, out value))
					{
						GetNotifier().Notify(2, DllExportLogginCodes.NoParserActionError, Resources.No_action_for_parser_state_0_, state.State);
					}
					else
					{
						action2(value, arg);
					}
					if (state.AddLine)
					{
						state.Result.Add(text);
					}
				}
				List<string> list2 = state.Result;
				if (state.ExternalAssemlyDeclarations.Count > 0)
				{
					list2 = new List<string>(state.Result.Count);
					list2.AddRange(state.Result);
					List<ExternalAssemlyDeclaration> list3 = new List<ExternalAssemlyDeclaration>(state.ExternalAssemlyDeclarations.Count);
					Dictionary<string, int> foundAliases = new Dictionary<string, int>();
					foreach (string item in list2)
					{
						if (item.Length < 3 || !item.Contains("["))
						{
							continue;
						}
						IlParsingUtils.ParseIlSnippet(item, ParsingDirection.Forward, delegate(IlParsingUtils.IlSnippetLocation current)
						{
							if (current.WithinScope && !current.WithinString && current.LastIdentifier != null && !foundAliases.ContainsKey(current.LastIdentifier))
							{
								foundAliases.Add(current.LastIdentifier, foundAliases.Count);
							}
							return true;
						});
					}
					foreach (ExternalAssemlyDeclaration externalAssemlyDeclaration in state.ExternalAssemlyDeclarations)
					{
						if (!foundAliases.ContainsKey(externalAssemlyDeclaration.AliasName))
						{
							list3.Add(externalAssemlyDeclaration);
						}
					}
					if (list3.Count > 0)
					{
						list3.Reverse();
						foreach (ExternalAssemlyDeclaration item2 in list3)
						{
							int num = 0;
							int num2 = -1;
							for (int j = item2.InputLineIndex; j < list2.Count; j++)
							{
								string text2 = list2[j].TrimStart();
								if (text2 == "{")
								{
									num++;
								}
								else if (text2 == "}")
								{
									if (num == 1)
									{
										num2 = j;
										break;
									}
									num--;
								}
							}
							if (num2 > -1)
							{
								GetNotifier().Notify(-2, DllExportLogginCodes.RemovingReferenceToDllExportAttributeAssembly, string.Format(Resources.Deleting_reference_to_0_, item2.AssemblyName, (item2.AliasName != item2.AssemblyName) ? string.Format(Resources.AssemblyAlias, item2.AliasName) : ""));
								list2.RemoveRange(item2.InputLineIndex, num2 - item2.InputLineIndex + 1);
							}
						}
					}
				}
				stopwatch.Stop();
				GetNotifier().Notify(-2, "EXPPERF02", Resources.Parsing_0_lines_of_IL_took_1_ms_, list.Count, stopwatch.ElapsedMilliseconds);
				if (ProfileActions)
				{
					foreach (KeyValuePair<ParserState, IParserStateAction> item3 in actionsByState)
					{
						GetNotifier().Notify(-1, "EXPPERF03", Resources.Parsing_action_0_took_1_ms, item3.Key, item3.Value.Milliseconds);
					}
				}
				return list2;
			}
		}

		internal IDllExportNotifier GetNotifier()
		{
			return base.ServiceProvider.GetService<IDllExportNotifier>();
		}

		private HashSet<string> GetMethodAttributes()
		{
			string text = (InputValues.MethodAttributes ?? "").Trim();
			return new HashSet<string>(string.IsNullOrEmpty(text) ? _DefaultMethodAttributes : text.Split(new char[6] { ' ', ',', ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Distinct());
		}

		private static string GetExePath(string toolFileName, string installPath, string settingsName)
		{
			string text = "";
			if (!string.IsNullOrEmpty(installPath))
			{
				text = Path.Combine(Path.GetFullPath(installPath), toolFileName);
				if (!File.Exists(text))
				{
					text = Path.Combine(Path.Combine(Path.GetFullPath(installPath), "Bin"), toolFileName);
				}
			}
			else if (!string.IsNullOrEmpty((Settings.Default[settingsName] as string).NullSafeTrim()))
			{
				text = Settings.Default.ILDasmPath;
			}
			if (string.IsNullOrEmpty(text) || !File.Exists(text))
			{
				text = toolFileName;
			}
			return text;
		}

		private static IEnumerable<string> EnumerateStreamLines(StreamReader streamReader)
		{
			while (!streamReader.EndOfStream)
			{
				yield return streamReader.ReadLine();
			}
		}

		internal static int RunIlTool(string installPath, string toolFileName, string requiredPaths, string workingDirectory, string settingsName, string arguments, string toolLoggingCode, string verboseLoggingCode, IDllExportNotifier notifier, int timeout, Func<string, bool> suppressErrorOutputLine = null)
		{
			using (notifier.CreateContextName(null, toolFileName))
			{
				if (suppressErrorOutputLine != null)
				{
					Func<string, bool> suppressErrorOutputLineCore = suppressErrorOutputLine;
					suppressErrorOutputLine = (string line) => line != null && suppressErrorOutputLineCore(line);
				}
				else
				{
					suppressErrorOutputLine = (string l) => false;
				}
				string fileName = Path.GetFileName(toolFileName);
				string exePath = GetExePath(fileName, installPath, settingsName);
				fileName = Path.GetFileNameWithoutExtension(fileName);
				using (Process process = new Process())
				{
					notifier.Notify(-2, toolLoggingCode, Resources.calling_0_with_1_, exePath, arguments);
					ProcessStartInfo processStartInfo = new ProcessStartInfo(exePath, arguments);
					processStartInfo.UseShellExecute = false;
					processStartInfo.CreateNoWindow = true;
					processStartInfo.RedirectStandardOutput = true;
					processStartInfo.RedirectStandardError = true;
					ProcessStartInfo processStartInfo2 = processStartInfo;
					if (!string.IsNullOrEmpty(workingDirectory))
					{
						processStartInfo2.WorkingDirectory = Path.GetFullPath(workingDirectory);
					}
					if (!string.IsNullOrEmpty(requiredPaths))
					{
						processStartInfo2.EnvironmentVariables["PATH"] = requiredPaths.Trim(';') + ";" + (processStartInfo2.EnvironmentVariables.ContainsKey("PATH") ? processStartInfo2.EnvironmentVariables["PATH"] : null);
					}
					process.StartInfo = processStartInfo2;
					process.Start();
					Stopwatch stopwatch = Stopwatch.StartNew();
					StringBuilder stringBuilder = new StringBuilder();
					StringBuilder stringBuilder2 = new StringBuilder();
					Action<IEnumerable<string>, StringBuilder> appendLines = delegate(IEnumerable<string> lines, StringBuilder sb)
					{
						lines.Aggregate(sb, (StringBuilder r, string line) => r.AppendLine(line));
					};
					Action<StreamReader, StringBuilder> action = delegate(StreamReader sr, StringBuilder sb)
					{
						appendLines(EnumerateStreamLines(sr), sb);
					};
					Action<StreamReader, StringBuilder> action2 = delegate(StreamReader sr, StringBuilder sb)
					{
						appendLines(from line in EnumerateStreamLines(sr)
							where !suppressErrorOutputLine(line)
							select line, sb);
					};
					while (stopwatch.ElapsedMilliseconds < timeout && !process.HasExited)
					{
						action(process.StandardOutput, stringBuilder);
						action2(process.StandardError, stringBuilder2);
					}
					bool hasExited = process.HasExited;
					action(process.StandardOutput, stringBuilder);
					action2(process.StandardError, stringBuilder2);
					if (hasExited)
					{
						notifier.Notify(-2, toolLoggingCode, Resources.R_0_1_returned_gracefully, fileName, exePath);
						int exitCode = process.ExitCode;
						if (exitCode != 0 || stringBuilder2.Length > 0)
						{
							throw new InvalidOperationException(((stringBuilder2.Length > 0) ? stringBuilder2 : stringBuilder).ToString());
						}
						if (stringBuilder2.Length > 0)
						{
							notifier.Notify(-3, verboseLoggingCode, stringBuilder2.ToString());
						}
						if (stringBuilder.Length > 0)
						{
							notifier.Notify(-3, verboseLoggingCode, stringBuilder.ToString());
						}
						return exitCode;
					}
					bool flag = false;
					Exception ex = null;
					try
					{
						process.Kill();
						flag = true;
					}
					catch (Exception ex2)
					{
						ex = ex2;
					}
					if (flag)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.R_0_did_not_return_after_1_ms, fileName, timeout));
					}
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.R_0_did_not_return_after_1_ms_and_it_could_not_be_stopped, fileName, timeout, ex.Message));
				}
			}
		}
	}
}
