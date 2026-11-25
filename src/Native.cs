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
	internal static class Native
	{
		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		public delegate bool EnumWindowsProc(IntPtr h, IntPtr l);

		public struct POINT
		{
			public int X;

			public int Y;
		}

		public struct RECT
		{
			public int L;

			public int T;

			public int R;

			public int B;
		}

		public struct MONITORINFO
		{
			public int cbSize;

			public RECT rcMonitor;

			public RECT rcWork;

			public uint dwFlags;
		}

		public struct GUITHREADINFO
		{
			public int cbSize;

			public int flags;

			public IntPtr hwndActive;

			public IntPtr hwndFocus;

			public IntPtr hwndCapture;

			public IntPtr hwndMenuOwner;

			public IntPtr hwndMoveSize;

			public IntPtr hwndCaret;

			public RECT rcCaret;
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll")]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll")]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetModuleHandle(string name);

		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr h, int i);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr h, int i, int v);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint f);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr h, out RECT r);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetClassName(IntPtr h, StringBuilder sb, int n);

		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromWindow(IntPtr h, uint f);

		[DllImport("user32.dll")]
		public static extern bool GetMonitorInfo(IntPtr m, ref MONITORINFO mi);

		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT p);

		[DllImport("user32.dll")]
		public static extern bool IsIconic(IntPtr h);

		[DllImport("user32.dll")]
		public static extern bool IsZoomed(IntPtr h);

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr pid);

		[DllImport("user32.dll")]
		public static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO info);

		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(int vk);

		[DllImport("user32.dll")]
		public static extern bool IsClipboardFormatAvailable(uint format);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern uint RegisterClipboardFormat(string name);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string cls, string title);

		[DllImport("user32.dll")]
		public static extern bool EnumWindows(EnumWindowsProc f, IntPtr l);

		[DllImport("user32.dll")]
		public static extern bool IsWindowVisible(IntPtr h);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr h, int cmd);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr h);

		[DllImport("user32.dll")]
		public static extern bool PostMessage(IntPtr h, uint msg, IntPtr w, IntPtr l);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindow(IntPtr h, uint cmd);

		[DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
		public static extern uint GetWindowPid(IntPtr h, out uint pid);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetWindowTextLength(IntPtr h);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern bool QueryFullProcessImageName(IntPtr h, int flags, StringBuilder sb, ref int size);

		[DllImport("kernel32.dll")]
		public static extern bool CloseHandle(IntPtr h);

		[DllImport("shell32.dll")]
		public static extern int SHGetKnownFolderPath(ref Guid id, uint flags, IntPtr token, out IntPtr path);

		public static string ProcessPath(int pid)
		{
			IntPtr intPtr = OpenProcess(4096u, inherit: false, pid);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			try
			{
				StringBuilder stringBuilder = new StringBuilder(1024);
				int size = stringBuilder.Capacity;
				return QueryFullProcessImageName(intPtr, 0, stringBuilder, ref size) ? stringBuilder.ToString() : null;
			}
			finally
			{
				CloseHandle(intPtr);
			}
		}

		public static List<IntPtr> AppWindows(HashSet<int> pids, bool visibleOnly)
		{
			List<IntPtr> list = new List<IntPtr>();
			EnumWindows(delegate(IntPtr h, IntPtr l)
			{
				GetWindowPid(h, out var pid);
				if (!pids.Contains((int)pid))
				{
					return true;
				}
				if (GetWindow(h, 4u) != IntPtr.Zero)
				{
					return true;
				}
				if (visibleOnly && (!IsWindowVisible(h) || GetWindowTextLength(h) == 0))
				{
					return true;
				}
				if ((GetWindowLong(h, -20) & 0x80) != 0)
				{
					return true;
				}
				list.Add(h);
				return true;
			}, IntPtr.Zero);
			return list;
		}

		public static string ClassOf(IntPtr h)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			GetClassName(h, stringBuilder, 256);
			return stringBuilder.ToString();
		}

		public static void MakeOverlay(IntPtr h, bool clickThrough)
		{
			int windowLong = GetWindowLong(h, -20);
			windowLong |= 0x8000080;
			if (clickThrough)
			{
				windowLong |= 0x80020;
			}
			SetWindowLong(h, -20, windowLong);
		}

		public static void SetClickThrough(IntPtr h, bool on)
		{
			int windowLong = GetWindowLong(h, -20);
			SetWindowLong(h, -20, on ? (windowLong | 0x20) : (windowLong & ~0x20));
		}

		public static bool MouseButtonDown()
		{
			return GetAsyncKeyState(1) < 0 || GetAsyncKeyState(2) < 0;
		}

		public static void TopMost(IntPtr h)
		{
			if (h != IntPtr.Zero)
			{
				SetWindowPos(h, new IntPtr(-1), 0, 0, 0, 0, 19u);
			}
		}
	}
}
