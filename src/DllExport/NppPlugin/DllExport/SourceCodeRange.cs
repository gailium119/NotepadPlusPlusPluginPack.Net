using System.Text;

namespace NppPlugin.DllExport
{
	public sealed class SourceCodeRange
	{
		public string FileName { get; private set; }

		public SourceCodePosition StartPosition { get; private set; }

		public SourceCodePosition EndPosition { get; private set; }

		public static SourceCodeRange FromMsIlLine(string line)
		{
			SourceCodePosition start;
			SourceCodePosition end;
			string fileName;
			if (!ExtractLineParts(line, out start, out end, out fileName))
			{
				return null;
			}
			return new SourceCodeRange(fileName, start, end);
		}

		private static bool ExtractLineParts(string line, out SourceCodePosition start, out SourceCodePosition end, out string fileName)
		{
			start = default(SourceCodePosition);
			end = default(SourceCodePosition);
			fileName = null;
			line = line.TrimStart();
			if (!line.StartsWith(".line"))
			{
				return false;
			}
			line = line.Substring(5).Trim();
			if (string.IsNullOrEmpty(line))
			{
				return false;
			}
			int num = 0;
			string text = null;
			string text2 = null;
			string columnText = null;
			string text3 = null;
			StringBuilder stringBuilder = new StringBuilder(line.Length);
			bool flag = false;
			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];
				if (c == '\'')
				{
					if (!flag)
					{
						string text4 = line.Substring(num, i - num).Trim();
						if (text3 == null)
						{
							text3 = text4;
						}
					}
					flag = !flag;
				}
				else if (flag)
				{
					stringBuilder.Append(c);
				}
				else
				{
					if (c != ',' && c != ':')
					{
						continue;
					}
					string text5 = line.Substring(num, i - num).Trim();
					num = i + 1;
					switch (c)
					{
					case ',':
						if (text == null)
						{
							text = text5;
						}
						else
						{
							columnText = text5;
						}
						break;
					case ':':
						if (text2 == null)
						{
							text2 = text5;
						}
						else
						{
							text3 = text5;
						}
						break;
					}
				}
			}
			start = SourceCodePosition.FromText(text, columnText) ?? start;
			end = SourceCodePosition.FromText(text2, text3) ?? end;
			fileName = ((stringBuilder.Length > 0) ? stringBuilder.ToString() : null);
			return fileName != null;
		}

		public SourceCodeRange(string fileName, SourceCodePosition startPosition, SourceCodePosition endPosition)
		{
			FileName = fileName;
			StartPosition = startPosition;
			EndPosition = endPosition;
		}

		private bool Equals(SourceCodeRange other)
		{
			if (string.Equals(FileName, other.FileName) && StartPosition.Equals(other.StartPosition))
			{
				return EndPosition.Equals(other.EndPosition);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj is SourceCodeRange)
			{
				return Equals((SourceCodeRange)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((FileName != null) ? FileName.GetHashCode() : 0) * 397) ^ StartPosition.GetHashCode()) * 397) ^ EndPosition.GetHashCode();
		}
	}
}
