using System;
using System.Windows.Forms;

namespace RevitShutteringAddin
{
	public class WindowHandle : IWin32Window
	{
		private readonly IntPtr _hwnd;

		public IntPtr Handle => _hwnd;

		public WindowHandle(IntPtr hwnd)
		{
			_hwnd = hwnd;
		}
	}
}
