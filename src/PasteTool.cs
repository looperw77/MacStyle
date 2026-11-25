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
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
	internal interface IServiceProvider
	{
		[PreserveSig]
		int QueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
	}
	[ComImport]
	[Guid("000214E2-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellBrowser
	{
		[PreserveSig]
		int GetWindow(out IntPtr phwnd);
	}
	internal class PasteTool
	{
		private class Target
		{
			public bool Desktop;

			public IntPtr Hwnd;
		}

		private const int WH_KEYBOARD_LL = 13;

		private const int WM_KEYDOWN = 256;

		private const int WM_SYSKEYDOWN = 260;

		private const uint CF_BITMAP = 2u;

		private const uint CF_DIB = 8u;

		private const uint CF_HDROP = 15u;

		private const uint CF_DIBV5 = 17u;

		private static readonly Guid SID_STopLevelBrowser = new Guid("4C96BE40-915C-11CF-99D3-00AA004AE837");

		private static readonly Guid IID_IShellBrowser = new Guid("000214E2-0000-0000-C000-000000000046");

		private readonly Config cfg;

		private readonly Action<string> onSaved;

		private readonly Native.LowLevelKeyboardProc proc;

		private IntPtr hook;

		private uint cfPng;

		public PasteTool(Config cfg, Action<string> onSaved)
		{
			this.cfg = cfg;
			this.onSaved = onSaved;
			proc = HookProc;
			cfPng = Native.RegisterClipboardFormat("PNG");
			hook = Native.SetWindowsHookEx(13, proc, Native.GetModuleHandle(null), 0u);
		}

		public void Dispose()
		{
			if (hook != IntPtr.Zero)
			{
				Native.UnhookWindowsHookEx(hook);
			}
		}

		private static bool Down(int vk)
		{
			return (Native.GetAsyncKeyState(vk) & 0x8000) != 0;
		}

		private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && cfg.PasteEnabled && (wParam == (IntPtr)256 || wParam == (IntPtr)260))
			{
				int num = Marshal.ReadInt32(lParam);
				if (num == 86 && Down(17) && !Down(18) && !Down(16) && !Down(91) && !Down(92))
				{
					try
					{
						Target t = DetectTarget();
						if (t != null && ClipboardHasImage())
						{
							Thread thread = new Thread((ThreadStart)delegate
							{
								DoPaste(t);
							});
							thread.SetApartmentState(ApartmentState.STA);
							thread.IsBackground = true;
							thread.Start();
							return (IntPtr)1;
						}
					}
					catch
					{
					}
				}
			}
			return Native.CallNextHookEx(hook, nCode, wParam, lParam);
		}

		private bool ClipboardHasImage()
		{
			if (Native.IsClipboardFormatAvailable(15u))
			{
				return false;
			}
			if (!Native.IsClipboardFormatAvailable(8u) && !Native.IsClipboardFormatAvailable(17u) && !Native.IsClipboardFormatAvailable(2u))
			{
				if (cfPng != 0)
				{
					return Native.IsClipboardFormatAvailable(cfPng);
				}
				return false;
			}
			return true;
		}

		private static Target DetectTarget()
		{
			IntPtr foregroundWindow = Native.GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero)
			{
				return null;
			}
			string text = Native.ClassOf(foregroundWindow);
			bool flag = text == "CabinetWClass" || text == "ExploreWClass";
			bool flag2 = text == "Progman" || text == "WorkerW";
			if (!flag && !flag2)
			{
				return null;
			}
			Native.GUITHREADINFO info = default(Native.GUITHREADINFO);
			info.cbSize = Marshal.SizeOf(info);
			if (Native.GetGUIThreadInfo(Native.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero), ref info) && info.hwndFocus != IntPtr.Zero)
			{
				string text2 = Native.ClassOf(info.hwndFocus);
				if (text2 != "DirectUIHWND" && text2 != "SysListView32" && text2 != "SHELLDLL_DefView" && info.hwndFocus != foregroundWindow)
				{
					return null;
				}
			}
			Target target = new Target();
			target.Desktop = flag2;
			target.Hwnd = foregroundWindow;
			return target;
		}

		private void DoPaste(Target t)
		{
			try
			{
				dynamic val = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
				dynamic val2 = null;
				string text = null;
				if (t.Desktop)
				{
					text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
					try
					{
						dynamic val3 = val.Windows();
						object obj = 0;
						object obj2 = null;
						int num;
						dynamic val4 = val3.FindWindowSW(ref obj, ref obj2, 8, out num, 1);
						val2 = val4.Document;
					}
					catch
					{
					}
				}
				else
				{
					dynamic val5 = FindExplorerWindow(val, t.Hwnd);
					if (val5 == null)
					{
						return;
					}
					val2 = val5.Document;
					text = (string)val2.Folder.Self.Path;
				}
				if (string.IsNullOrEmpty(text) || !Directory.Exists(text))
				{
					return;
				}
				string text2 = SaveClipboardImage(text);
				if (text2 != null)
				{
					if (onSaved != null)
					{
						onSaved(System.IO.Path.GetFileName(text2));
					}
					if (val2 != null)
					{
						SelectForRename(val2, System.IO.Path.GetFileName(text2));
					}
				}
			}
			catch (Exception ex)
			{
				Sys.Log("paste: " + ex.Message);
			}
		}

		private static dynamic FindExplorerWindow(dynamic shell, IntPtr fg)
		{
			IntPtr intPtr = Native.FindWindowEx(fg, IntPtr.Zero, "ShellTabWindowClass", null);
			dynamic val = shell.Windows();
			int num = val.Count;
			dynamic val2 = null;
			for (int i = 0; i < num; i++)
			{
				dynamic val3 = null;
				try
				{
					val3 = val.Item(i);
				}
				catch
				{
				}
				if (val3 == null)
				{
					continue;
				}
				long num2;
				try
				{
					num2 = Convert.ToInt64(val3.HWND);
				}
				catch
				{
					continue;
				}
				if (num2 != fg.ToInt64())
				{
					continue;
				}
				if (val2 == null)
				{
					val2 = val3;
				}
				if (intPtr == IntPtr.Zero)
				{
					return val3;
				}
				try
				{
					IServiceProvider serviceProvider = (IServiceProvider)(object)val3;
					Guid guidService = SID_STopLevelBrowser;
					Guid riid = IID_IShellBrowser;
					if (serviceProvider.QueryService(ref guidService, ref riid, out var ppvObject) == 0)
					{
						((IShellBrowser)ppvObject).GetWindow(out var phwnd);
						if (phwnd == intPtr)
						{
							return val3;
						}
					}
				}
				catch
				{
				}
			}
			return val2;
		}

		private static void SelectForRename(dynamic doc, string name)
		{
			for (int i = 0; i < 25; i++)
			{
				Thread.Sleep(120);
				try
				{
					dynamic val = doc.Folder.ParseName(name);
					if (!((val == null) ? true : false))
					{
						doc.SelectItem(val, 29);
						dynamic val2 = doc.FocusedItem;
						if (val2 != null && string.Equals((string)val2.Name, (string)val.Name, StringComparison.OrdinalIgnoreCase))
						{
							doc.SelectItem(val, 31);
							break;
						}
					}
				}
				catch
				{
				}
			}
		}

		private string SaveClipboardImage(string folder)
		{
			for (int i = 0; i < 6; i++)
			{
				try
				{
					System.Windows.Forms.IDataObject dataObject = System.Windows.Forms.Clipboard.GetDataObject();
					if (dataObject == null)
					{
						return null;
					}
					string text = ((cfg.PasteFormat == "jpg") ? ".jpg" : ".png");
					string text2 = "Screenshot " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
					string text3 = System.IO.Path.Combine(folder, text2 + text);
					int num = 2;
					while (File.Exists(text3))
					{
						text3 = System.IO.Path.Combine(folder, text2 + " (" + num + ")" + text);
						num++;
					}
					MemoryStream memoryStream = null;
					if (dataObject.GetDataPresent("PNG"))
					{
						memoryStream = dataObject.GetData("PNG") as MemoryStream;
					}
					if (text == ".png" && memoryStream != null && memoryStream.Length > 0)
					{
						File.WriteAllBytes(text3, memoryStream.ToArray());
						return text3;
					}
					System.Drawing.Image image = System.Windows.Forms.Clipboard.GetImage();
					if (image == null && memoryStream != null)
					{
						image = System.Drawing.Image.FromStream(memoryStream);
					}
					if (image == null)
					{
						return null;
					}
					using (image)
					{
						if (text == ".png")
						{
							image.Save(text3, ImageFormat.Png);
						}
						else
						{
							using (Bitmap bitmap = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
							{
								using (Graphics graphics = Graphics.FromImage(bitmap))
								{
									graphics.Clear(System.Drawing.Color.White);
									graphics.DrawImage(image, 0, 0, image.Width, image.Height);
								}
								ImageCodecInfo encoder = Array.Find(ImageCodecInfo.GetImageEncoders(), (ImageCodecInfo c) => c.FormatID == ImageFormat.Jpeg.Guid);
								EncoderParameters encoderParameters = new EncoderParameters(1);
								encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
								bitmap.Save(text3, encoder, encoderParameters);
							}
						}
					}
					return text3;
				}
				catch (ExternalException)
				{
					Thread.Sleep(80);
				}
			}
			return null;
		}
	}
}
