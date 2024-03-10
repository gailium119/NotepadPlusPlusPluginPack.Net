using System;

namespace NppPlugin.DllExport
{
	public struct SourceCodePosition : IEquatable<SourceCodePosition>
	{
		private readonly int _Character;

		private readonly int _Line;

		public int Line
		{
			get
			{
				return _Line;
			}
		}

		public int Character
		{
			get
			{
				return _Character;
			}
		}

		public static SourceCodePosition? FromText(string lineText, string columnText)
		{
			int? num = null;
			int? num2 = null;
			int result;
			if (int.TryParse(lineText, out result))
			{
				num = result;
			}
			if (int.TryParse(columnText, out result))
			{
				num2 = result;
			}
			if (num.HasValue || num2.HasValue)
			{
				return new SourceCodePosition(num ?? (-1), num2 ?? (-1));
			}
			return null;
		}

		public SourceCodePosition(int line, int character)
		{
			_Line = line;
			_Character = character;
		}

		public bool Equals(SourceCodePosition other)
		{
			if (other._Line == _Line)
			{
				return other._Character == _Character;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (!object.ReferenceEquals(null, obj) && obj.GetType() == typeof(SourceCodePosition))
			{
				return Equals((SourceCodePosition)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (_Line * 397) ^ _Character;
		}

		public static bool operator ==(SourceCodePosition left, SourceCodePosition right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SourceCodePosition left, SourceCodePosition right)
		{
			return !left.Equals(right);
		}
	}
}
