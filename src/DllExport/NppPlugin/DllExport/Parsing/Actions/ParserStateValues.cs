using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NppPlugin.DllExport.Properties;

namespace NppPlugin.DllExport.Parsing.Actions
{
	public sealed class ParserStateValues
	{
		public sealed class MethodStateValues
		{
			public string Declaration { get; set; }

			public string ResultAttributes { get; set; }

			public string Name { get; set; }

			public string Attributes { get; set; }

			public string Result { get; set; }

			public string After { get; set; }

			public MethodStateValues()
			{
				Reset();
			}

			public override string ToString()
			{
				return Name.IfEmpty(Resources.no_name___) + "; " + Declaration.IfEmpty(Resources.no_declaration___);
			}

			public void Reset()
			{
				Declaration = "";
				Name = "";
				Attributes = "";
				Result = "";
				ResultAttributes = "";
				After = "";
			}
		}

		public readonly Stack<string> ClassNames = new Stack<string>();

		public readonly MethodStateValues Method = new MethodStateValues();

		private readonly CpuPlatform _Cpu;

		private readonly ReadOnlyCollection<string> _InputLines;

		private readonly List<string> _Result = new List<string>();

		public bool AddLine;

		public string ClassDeclaration;

		public int MethodPos;

		public ParserState State;

		private readonly IList<ExternalAssemlyDeclaration> _ReadonlyExternalAssemlyDeclarations;

		private readonly List<ExternalAssemlyDeclaration> _ExternalAssemlyDeclarations = new List<ExternalAssemlyDeclaration>();

		public IList<string> InputLines
		{
			get
			{
				return _InputLines;
			}
		}

		public int InputPosition { get; internal set; }

		public CpuPlatform Cpu
		{
			get
			{
				return _Cpu;
			}
		}

		public List<string> Result
		{
			get
			{
				return _Result;
			}
		}

		public IList<ExternalAssemlyDeclaration> ExternalAssemlyDeclarations
		{
			get
			{
				return _ReadonlyExternalAssemlyDeclarations;
			}
		}

		public SourceCodeRange GetRange()
		{
			for (int i = InputPosition; i < InputLines.Count; i++)
			{
				string text = InputLines[i];
				if (text != null && (text = text.Trim()).StartsWith(".line", StringComparison.Ordinal))
				{
					return SourceCodeRange.FromMsIlLine(text);
				}
			}
			return null;
		}

		public ParserStateValues(CpuPlatform cpu, IList<string> inputLines)
		{
			_Cpu = cpu;
			_InputLines = new ReadOnlyCollection<string>(inputLines);
			_ReadonlyExternalAssemlyDeclarations = _ExternalAssemlyDeclarations.AsReadOnly();
		}

		public ExternalAssemlyDeclaration RegisterMsCorelibAlias(string assemblyName, string alias)
		{
			ExternalAssemlyDeclaration externalAssemlyDeclaration = new ExternalAssemlyDeclaration(Result.Count, assemblyName, alias);
			_ExternalAssemlyDeclarations.Add(externalAssemlyDeclaration);
			return externalAssemlyDeclaration;
		}
	}
}
