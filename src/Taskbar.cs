using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using Microsoft.Win32;
using Windows.Devices.Radios;
using Windows.Foundation;
using Windows.Media.Control;
using Windows.Storage.Streams;
using Rect = System.Windows.Rect;

namespace MacStyle
{
	internal static class Taskbar
	{
		public struct APPBARDATA
		{
			public int cbSize;

			public IntPtr hWnd;

			public uint uCallbackMessage;

			public uint uEdge;

			public Native.RECT rc;

			public IntPtr lParam;
		}

		private static bool hiding;

		private static string StateFile => System.IO.Path.Combine(Brand.DataDir, "taskbar.txt");

		[DllImport("shell32.dll")]
		public static extern UIntPtr SHAppBarMessage(uint msg, ref APPBARDATA d);

		private static List<IntPtr> TrayWindows()
		{
			List<IntPtr> list = new List<IntPtr>();
			string[] array = new string[2] { "Shell_TrayWnd", "Shell_SecondaryTrayWnd" };
			foreach (string cls in array)
			{
				IntPtr intPtr = IntPtr.Zero;
				while ((intPtr = Native.FindWindowEx(IntPtr.Zero, intPtr, cls, null)) != IntPtr.Zero)
				{
					list.Add(intPtr);
				}
			}
			return list;
		}

		public static void Hide()
		{
			hiding = true;
			try
			{
				APPBARDATA d = new APPBARDATA
				{
					cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
					hWnd = Native.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null)
				};
				int num = (int)(uint)SHAppBarMessage(4u, ref d);
				if (!File.Exists(StateFile))
				{
					Directory.CreateDirectory(Brand.DataDir);
					File.WriteAllText(StateFile, num.ToString(CultureInfo.InvariantCulture));
				}
				if ((num & 1) == 0)
				{
					d.lParam = new IntPtr(num | 1);
					SHAppBarMessage(10u, ref d);
				}
			}
			catch (Exception ex)
			{
				Sys.Log("taskbar hide: " + ex.Message);
			}
			Enforce();
		}

		public static void Enforce()
		{
			if (!hiding)
			{
				return;
			}
			IntPtr foregroundWindow = Native.GetForegroundWindow();
			if (foregroundWindow != IntPtr.Zero)
			{
				Native.GetWindowPid(foregroundWindow, out var pid);
				switch ((System.IO.Path.GetFileName(Native.ProcessPath((int)pid) ?? "") ?? "").ToLowerInvariant())
				{
				case "searchhost.exe":
					return;
				case "shellexperiencehost.exe":
					return;
				}
			}
			foreach (IntPtr item in TrayWindows())
			{
				if (Native.IsWindowVisible(item))
				{
					Native.ShowWindow(item, 0);
				}
			}
		}

		public static void Restore()
		{
			bool flag = hiding || File.Exists(StateFile);
			hiding = false;
			if (!flag)
			{
				return;
			}
			foreach (IntPtr item in TrayWindows())
			{
				Native.ShowWindow(item, 5);
			}
			try
			{
				if (File.Exists(StateFile))
				{
					int.TryParse(File.ReadAllText(StateFile).Trim(), out var result);
					APPBARDATA d = new APPBARDATA
					{
						cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
						hWnd = Native.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null),
						lParam = new IntPtr(result)
					};
					SHAppBarMessage(10u, ref d);
					File.Delete(StateFile);
				}
			}
			catch
			{
			}
		}
	}
}
