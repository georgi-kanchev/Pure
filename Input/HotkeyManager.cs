namespace Purity.Input
{
	/// <summary>
	/// (Inherits <see cref="Device{T}"/>)<br></br><br></br>
	/// 
	/// Exposes the <see cref="Device{T}"/> functionality through the <see cref="Set"/> method.
	/// Used to combine multiple <see cref="Device{T}"/> inputs into a single one.
	/// Or in other words: ties multiple inputs to a single <typeparamref name="T"/>.
	/// </summary>
	public class HotkeyManager<T> : Device<T> where T : Enum
	{
		/// <summary>
		/// Determines whether a <paramref name="hotkey"/> <paramref name="isPressed"/>.
		/// Should be called continuously, just like <see cref="Device{T}.Update"/>. Used
		/// to combine multiple <see cref="Device{T}"/> inputs into a single one.
		/// </summary>
		public void Set(T hotkey, bool isPressed)
		{
			hotkeys[hotkey] = isPressed;
		}

		protected override bool IsPressedRaw(T input)
		{
			return hotkeys.ContainsKey(input) && hotkeys[input];
		}

		#region Backend
		private readonly Dictionary<T, bool> hotkeys = new();
		#endregion
	}
}
