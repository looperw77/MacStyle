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
	[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
	internal interface IShellItem
	{
		[PreserveSig]
		int BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);

		[PreserveSig]
		int GetParent(out IShellItem ppsi);

		[PreserveSig]
		int GetDisplayName(uint sigdn, out IntPtr name);

		[PreserveSig]
		int GetAttributes(uint mask, out uint attribs);

		[PreserveSig]
		int Compare(IShellItem psi, uint hint, out int order);
	}
	[ComImport]
	[Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellItemImageFactory
	{
		[PreserveSig]
		int GetImage(Dwm.SIZE size, int flags, out IntPtr hbm);
	}
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
	internal interface IPropertyStore
	{
		[PreserveSig]
		int GetCount(out uint c);

		[PreserveSig]
		int GetAt(uint i, out ShellApi.PROPERTYKEY k);

		[PreserveSig]
		int GetValue(ref ShellApi.PROPERTYKEY k, out ShellApi.PROPVARIANT v);

		[PreserveSig]
		int SetValue(ref ShellApi.PROPERTYKEY k, ref ShellApi.PROPVARIANT v);

		[PreserveSig]
		int Commit();
	}
	internal static class ShellApi
	{
		public struct PROPERTYKEY
		{
			public Guid fmtid;

			public uint pid;
		}

		[StructLayout(LayoutKind.Explicit, Size = 24)]
		public struct PROPVARIANT
		{
			[FieldOffset(0)]
			public ushort vt;

			[FieldOffset(8)]
			public IntPtr p;
		}

		private struct BITMAPINFOHEADER
		{
			public int biSize;

			public int biWidth;

			public int biHeight;

			public short biPlanes;

			public short biBitCount;

			public int biCompression;

			public int biSizeImage;

			public int biXPelsPerMeter;

			public int biYPelsPerMeter;

			public int biClrUsed;

			public int biClrImportant;
		}

		private struct BITMAP
		{
			public int bmType;

			public int bmWidth;

			public int bmHeight;

			public int bmWidthBytes;

			public ushort bmPlanes;

			public ushort bmBitsPixel;

			public IntPtr bmBits;
		}

		public struct SHQUERYRBINFO
		{
			public int cbSize;

			public long i64Size;

			public long i64NumItems;
		}

		private static readonly Dictionary<string, ImageSource> cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern int SHCreateItemFromParsingName(string path, IntPtr pbc, ref Guid riid, out IShellItem item);

		[DllImport("shell32.dll")]
		private static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid, out IPropertyStore ps);

		[DllImport("ole32.dll")]
		private static extern int PropVariantClear(ref PROPVARIANT pv);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetApplicationUserModelId(IntPtr hProcess, ref uint len, StringBuilder sb);

		[DllImport("gdi32.dll")]
		private static extern int GetObject(IntPtr h, int c, ref BITMAP bm);

		[DllImport("gdi32.dll")]
		private static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint lines, byte[] bits, ref BITMAPINFOHEADER bi, uint usage);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr h);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDC(IntPtr h);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr h, IntPtr dc);

		[DllImport("dwmapi.dll")]
		private static extern int DwmGetWindowAttribute(IntPtr h, int attr, out int v, int size);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern int SHQueryRecycleBin(string root, ref SHQUERYRBINFO info);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		public static extern int SHEmptyRecycleBin(IntPtr hwnd, string root, uint flags);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowText(IntPtr h, StringBuilder sb, int n);

		public static ImageSource Icon(string parsing, int size, bool useCache)
		{
			if (useCache && cache.TryGetValue(parsing, out var value))
			{
				return value;
			}
			value = null;
			IShellItem item = null;
			try
			{
				Guid riid = typeof(IShellItem).GUID;
				if (SHCreateItemFromParsingName(parsing, IntPtr.Zero, ref riid, out item) == 0 && item != null)
				{
					IShellItemImageFactory shellItemImageFactory = (IShellItemImageFactory)item;
					double cover = 1.0;
					if (shellItemImageFactory.GetImage(new Dwm.SIZE
					{
						cx = size,
						cy = size
					}, 5, out var hbm) == 0 && hbm != IntPtr.Zero)
					{
						try
						{
							value = FromHBitmap(hbm, out cover);
						}
						finally
						{
							DeleteObject(hbm);
						}
					}
					if (value != null && size > 64 && cover < 0.1 && shellItemImageFactory.GetImage(new Dwm.SIZE
					{
						cx = 48,
						cy = 48
					}, 5, out hbm) == 0 && hbm != IntPtr.Zero)
					{
						try
						{
							double cover2;
							BitmapSource bitmapSource = FromHBitmap(hbm, out cover2);
							if (bitmapSource != null)
							{
								value = bitmapSource;
							}
						}
						finally
						{
							DeleteObject(hbm);
						}
					}
				}
			}
			catch
			{
			}
			finally
			{
				if (item != null)
				{
					Marshal.ReleaseComObject(item);
				}
			}
			if (value != null)
			{
				cache[parsing] = value;
			}
			return value;
		}

		private static BitmapSource FromHBitmap(IntPtr hbm, out double cover)
		{
			cover = 1.0;
			BITMAP bm = default(BITMAP);
			if (GetObject(hbm, Marshal.SizeOf(typeof(BITMAP)), ref bm) == 0)
			{
				return null;
			}
			int bmWidth = bm.bmWidth;
			int num = Math.Abs(bm.bmHeight);
			if (bmWidth <= 0 || num <= 0)
			{
				return null;
			}
			BITMAPINFOHEADER bi = new BITMAPINFOHEADER
			{
				biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
				biWidth = bmWidth,
				biHeight = -num,
				biPlanes = 1,
				biBitCount = 32
			};
			byte[] array = new byte[bmWidth * num * 4];
			IntPtr dC = GetDC(IntPtr.Zero);
			try
			{
				if (GetDIBits(dC, hbm, 0u, (uint)num, array, ref bi, 0u) == 0)
				{
					return null;
				}
			}
			finally
			{
				ReleaseDC(IntPtr.Zero, dC);
			}
			bool flag = false;
			bool flag2 = true;
			int num2 = 0;
			for (int i = 0; i < array.Length; i += 4)
			{
				byte b = array[i + 3];
				if (b != 0)
				{
					flag = true;
				}
				if (b > 200)
				{
					num2++;
				}
				if (array[i] > b || array[i + 1] > b || array[i + 2] > b)
				{
					flag2 = false;
				}
			}
			cover = (double)num2 / (double)(bmWidth * num);
			if (!flag)
			{
				for (int j = 3; j < array.Length; j += 4)
				{
					array[j] = byte.MaxValue;
				}
				flag2 = false;
			}
			BitmapSource bitmapSource = BitmapSource.Create(bmWidth, num, 96.0, 96.0, flag2 ? PixelFormats.Pbgra32 : PixelFormats.Bgra32, null, array, bmWidth * 4);
			bitmapSource.Freeze();
			return bitmapSource;
		}

		public static ImageSource PackagedLogo(string aumid)
		{
			string key = "logo:" + aumid;
			if (cache.TryGetValue(key, out var value))
			{
				return value;
			}
			value = null;
			try
			{
				int num = aumid.IndexOf('!');
				if (num <= 0)
				{
					return null;
				}
				string text = aumid.Substring(0, num);
				string text2 = aumid.Substring(num + 1);
				int num2 = text.LastIndexOf('_');
				if (num2 <= 0)
				{
					return null;
				}
				string text3 = text.Substring(0, num2);
				string text4 = text.Substring(num2 + 1);
				List<string> list = new List<string>();
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages"))
				{
					if (registryKey == null)
					{
						return null;
					}
					string[] subKeyNames = registryKey.GetSubKeyNames();
					foreach (string text5 in subKeyNames)
					{
						if (!text5.StartsWith(text3 + "_", StringComparison.OrdinalIgnoreCase) || !text5.EndsWith("_" + text4, StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						using (RegistryKey registryKey2 = registryKey.OpenSubKey(text5))
						{
							string text6 = ((registryKey2 == null) ? null : (registryKey2.GetValue("PackageRootFolder") as string));
							if (!string.IsNullOrEmpty(text6) && File.Exists(System.IO.Path.Combine(text6, "AppxManifest.xml")))
							{
								list.Add(text6);
							}
						}
					}
				}
				string text7 = null;
				string text8 = null;
				foreach (string item in list)
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(System.IO.Path.Combine(item, "AppxManifest.xml"));
					foreach (XmlElement item2 in xmlDocument.GetElementsByTagName("*"))
					{
						if (item2.LocalName != "Application" || item2.GetAttribute("Id") != text2)
						{
							continue;
						}
						foreach (XmlNode childNode in item2.ChildNodes)
						{
							if (childNode is XmlElement xmlElement2 && !(xmlElement2.LocalName != "VisualElements"))
							{
								text8 = xmlElement2.GetAttribute("Square44x44Logo");
								if (string.IsNullOrEmpty(text8))
								{
									text8 = xmlElement2.GetAttribute("Square150x150Logo");
								}
							}
						}
					}
					if (!string.IsNullOrEmpty(text8))
					{
						text7 = item;
						break;
					}
				}
				if (text7 == null)
				{
					return null;
				}
				string directoryName = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(text7, text8));
				string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(text8);
				string extension = System.IO.Path.GetExtension(text8);
				if (!Directory.Exists(directoryName))
				{
					return null;
				}
				string text9 = null;
				int num3 = -1;
				string[] files = Directory.GetFiles(directoryName, fileNameWithoutExtension + "*" + extension);
				foreach (string text10 in files)
				{
					string text11 = System.IO.Path.GetFileNameWithoutExtension(text10).Substring(fileNameWithoutExtension.Length).ToLowerInvariant();
					if (!text11.Contains("contrast") && !text11.Contains("lightunplated"))
					{
						int num4 = 0;
						Match match = Regex.Match(text11, "(targetsize|scale)-(\\d+)");
						if (match.Success)
						{
							num4 = int.Parse(match.Groups[2].Value);
						}
						int num5 = (text11.Contains("targetsize") ? ((text11.Contains("unplated") ? 20000 : 10000) + Math.Min(num4, 256) - ((num4 > 256) ? 1 : 0)) : ((!text11.Contains("scale")) ? 500 : (1000 + num4)));
						if (num5 > num3)
						{
							num3 = num5;
							text9 = text10;
						}
					}
				}
				if (text9 == null)
				{
					return null;
				}
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = new Uri(text9);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				value = bitmapImage;
			}
			catch (Exception ex)
			{
				Sys.Log("logo " + aumid + ": " + ex.Message);
				value = null;
			}
			if (value != null)
			{
				cache[key] = value;
			}
			return value;
		}

		public static string DisplayName(string parsing)
		{
			IShellItem item = null;
			try
			{
				Guid riid = typeof(IShellItem).GUID;
				if (SHCreateItemFromParsingName(parsing, IntPtr.Zero, ref riid, out item) != 0 || item == null)
				{
					return null;
				}
				if (item.GetDisplayName(0u, out var name) != 0 || name == IntPtr.Zero)
				{
					return null;
				}
				try
				{
					return Marshal.PtrToStringUni(name);
				}
				finally
				{
					Marshal.FreeCoTaskMem(name);
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				if (item != null)
				{
					Marshal.ReleaseComObject(item);
				}
			}
		}

		public static string WindowAumid(IntPtr hwnd)
		{
			IPropertyStore ps = null;
			try
			{
				Guid iid = typeof(IPropertyStore).GUID;
				if (SHGetPropertyStoreForWindow(hwnd, ref iid, out ps) != 0 || ps == null)
				{
					return null;
				}
				PROPERTYKEY k = new PROPERTYKEY
				{
					fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"),
					pid = 5u
				};
				if (ps.GetValue(ref k, out var v) != 0)
				{
					return null;
				}
				try
				{
					return (v.vt == 31 && v.p != IntPtr.Zero) ? Marshal.PtrToStringUni(v.p) : null;
				}
				finally
				{
					PropVariantClear(ref v);
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				if (ps != null)
				{
					Marshal.ReleaseComObject(ps);
				}
			}
		}

		public static string ProcessAumid(int pid)
		{
			IntPtr intPtr = Native.OpenProcess(4096u, inherit: false, pid);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			try
			{
				uint len = 512u;
				StringBuilder stringBuilder = new StringBuilder(512);
				return (GetApplicationUserModelId(intPtr, ref len, stringBuilder) == 0) ? stringBuilder.ToString() : null;
			}
			catch
			{
				return null;
			}
			finally
			{
				Native.CloseHandle(intPtr);
			}
		}

		public static bool Cloaked(IntPtr h)
		{
			if (DwmGetWindowAttribute(h, 14, out var v, 4) == 0)
			{
				return v != 0;
			}
			return false;
		}

		public static string Title(IntPtr h)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			GetWindowText(h, stringBuilder, 256);
			return stringBuilder.ToString();
		}

		public static long RecycleCount()
		{
			try
			{
				SHQUERYRBINFO info = new SHQUERYRBINFO
				{
					cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO))
				};
				return (SHQueryRecycleBin(null, ref info) == 0) ? info.i64NumItems : 0;
			}
			catch
			{
				return 0L;
			}
		}

		public static string LnkTarget(string lnk)
		{
			try
			{
				dynamic val = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
				dynamic val2 = val.CreateShortcut(lnk);
				return val2.TargetPath;
			}
			catch
			{
				return null;
			}
		}
	}
}
