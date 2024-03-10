using System;

namespace NppPlugin.DllExport
{
	public sealed class NotificationContext : IEquatable<NotificationContext>
	{
		public string Name { get; private set; }

		public object Context { get; private set; }

		public NotificationContext(string name, object context)
		{
			Name = name;
			Context = context;
		}

		public bool Equals(NotificationContext other)
		{
			if (object.ReferenceEquals(null, other))
			{
				return false;
			}
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			if (object.Equals(Context, other.Context))
			{
				return string.Equals(Name, other.Name);
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
			if (obj is NotificationContext)
			{
				return Equals((NotificationContext)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((Context != null) ? Context.GetHashCode() : 0) * 397) ^ ((Name != null) ? Name.GetHashCode() : 0);
		}

		public static bool operator ==(NotificationContext left, NotificationContext right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(NotificationContext left, NotificationContext right)
		{
			return !object.Equals(left, right);
		}
	}
}
