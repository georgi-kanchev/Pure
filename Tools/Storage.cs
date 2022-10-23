using System.Reflection;

namespace Purity.Tools
{
	public class Storage<T>
	{
		public void Save(T obj, string filePath)
		{
			if(obj == null)
				return;

			var type = obj.GetType();
			var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			var members = type.GetFields(flags);
			for(int i = 0; i < members.Length; i++)
			{
				var member = members[i];
				var memberType = member.FieldType;
				var interfaces = memberType.GetInterfaces();
				var value = member.GetValue(obj);

				var subMembers = memberType.GetFields(flags);

				for(int j = 0; j < interfaces.Length; j++)
				{
					var name = interfaces[j].Name;
					if(typeof(IEnumerable<>).Name.Contains(name))
					{

						break;
					}
				}

				var size = Buffer.ByteLength((Array)value);
			}
		}

		#region Backend
		#endregion
	}
}