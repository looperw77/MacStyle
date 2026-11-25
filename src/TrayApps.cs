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
	internal class TrayApp
	{
		public string Name;

		public string Path;

		public string PackageFamily;

		public HashSet<int> Pids = new HashSet<int>();

		public ImageSource Icon;
	}
	internal static class TrayApps
	{
		private static readonly Dictionary<string, ImageSource> iconCache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

		private static readonly string[] Skip = new string[5] { "explorer.exe", "macstyle.exe", "dynamicisland.exe", "pastescreenshot.exe", "taskmgr.exe" };

		private static string ExpandKnownFolder(string p)
		{
			if (string.IsNullOrEmpty(p) || p[0] != '{')
			{
				return p;
			}
			int num = p.IndexOf('}');
			if (num < 0)
			{
				return p;
			}
			try
			{
				Guid id = new Guid(p.Substring(0, num + 1));
				if (Native.SHGetKnownFolderPath(ref id, 0u, IntPtr.Zero, out var path) == 0)
				{
					string text = Marshal.PtrToStringUni(path);
					Marshal.FreeCoTaskMem(path);
					return text + p.Substring(num + 1);
				}
			}
			catch
			{
			}
			return p;
		}

		public static List<TrayApp> GetRunning()
		{
			Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\NotifyIconSettings"))
				{
					if (registryKey != null)
					{
						string[] subKeyNames = registryKey.GetSubKeyNames();
						foreach (string name in subKeyNames)
						{
							using (RegistryKey registryKey2 = registryKey.OpenSubKey(name))
							{
								string text = ExpandKnownFolder(registryKey2.GetValue("ExecutablePath") as string);
								if (!string.IsNullOrEmpty(text))
								{
									byte[] array = registryKey2.GetValue("IconSnapshot") as byte[];
									if (!dictionary.ContainsKey(text) || (array != null && dictionary[text] == null))
									{
										dictionary[text] = array;
									}
								}
							}
						}
					}
				}
			}
			catch
			{
			}
			Dictionary<string, TrayApp> dictionary2 = new Dictionary<string, TrayApp>(StringComparer.OrdinalIgnoreCase);
			int id = Process.GetCurrentProcess().Id;
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				try
				{
					if (process.Id == id || process.Id <= 4)
					{
						continue;
					}
					string text2 = Native.ProcessPath(process.Id);
					if (text2 == null || !dictionary.ContainsKey(text2) || Array.IndexOf(Skip, System.IO.Path.GetFileName(text2).ToLowerInvariant()) >= 0)
					{
						continue;
					}
					if (!dictionary2.TryGetValue(text2, out var value))
					{
						TrayApp trayApp = new TrayApp();
						trayApp.Path = text2;
						trayApp.Name = NiceName(text2);
						trayApp.Icon = IconFor(text2, dictionary[text2]);
						value = trayApp;
						int num = text2.IndexOf("\\WindowsApps\\", StringComparison.OrdinalIgnoreCase);
						if (num >= 0)
						{
							string text3 = text2.Substring(num + 13).Split('\\')[0];
							string[] array2 = text3.Split('_');
							if (array2.Length >= 5)
							{
								value.PackageFamily = array2[0] + "_" + array2[array2.Length - 1];
							}
						}
						dictionary2[text2] = value;
					}
					value.Pids.Add(process.Id);
				}
				catch
				{
				}
				finally
				{
					process.Dispose();
				}
			}
			List<TrayApp> list = new List<TrayApp>(dictionary2.Values);
			list.Sort((TrayApp a, TrayApp b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
			return list;
		}

		private static string NiceName(string path)
		{
			try
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
				string text = ((!string.IsNullOrWhiteSpace(versionInfo.FileDescription)) ? versionInfo.FileDescription : versionInfo.ProductName);
				if (!string.IsNullOrWhiteSpace(text) && text.Length <= 28)
				{
					return Clean(text);
				}
				if (!string.IsNullOrWhiteSpace(versionInfo.ProductName) && versionInfo.ProductName.Length <= 28)
				{
					return Clean(versionInfo.ProductName);
				}
			}
			catch
			{
			}
			return Clean(System.IO.Path.GetFileNameWithoutExtension(path));
		}

		private static string Clean(string n)
		{
			n = n.Trim();
			if (n.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				n = n.Substring(0, n.Length - 4);
			}
			if (n.EndsWith(".Root", StringComparison.OrdinalIgnoreCase))
			{
				n = n.Substring(0, n.Length - 5);
			}
			return n;
		}

		private static ImageSource IconFor(string path, byte[] snap)
		{
			if (iconCache.TryGetValue(path, out var value))
			{
				return value;
			}
			try
			{
				if (snap != null && snap.Length > 0)
				{
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.StreamSource = new MemoryStream(snap);
					bitmapImage.EndInit();
					bitmapImage.Freeze();
					value = bitmapImage;
				}
			}
			catch
			{
				value = null;
			}
			if (value == null)
			{
				try
				{
					using (Icon icon = Icon.ExtractAssociatedIcon(path))
					{
						BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
						bitmapSource.Freeze();
						value = bitmapSource;
					}
				}
				catch
				{
				}
			}
			iconCache[path] = value;
			return value;
		}

		public static void Open(TrayApp app)
		{
			try
			{
				List<IntPtr> list = Native.AppWindows(app.Pids, visibleOnly: true);
				if (list.Count > 0)
				{
					IntPtr intPtr = list[0];
					if (Native.IsIconic(intPtr))
					{
						Native.ShowWindow(intPtr, 9);
					}
					Native.SetForegroundWindow(intPtr);
				}
				else if (app.PackageFamily != null)
				{
					Process.Start("explorer.exe", "shell:AppsFolder\\" + app.PackageFamily + "!App");
				}
				else
				{
					ProcessStartInfo processStartInfo = new ProcessStartInfo(app.Path);
					processStartInfo.UseShellExecute = true;
					processStartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(app.Path);
					Process.Start(processStartInfo);
				}
			}
			catch (Exception ex)
			{
				Sys.Log("tray open: " + ex.Message);
			}
		}

		public static void Quit(TrayApp app)
		{
			Task.Run(delegate
			{
				try
				{
					foreach (IntPtr item in Native.AppWindows(app.Pids, visibleOnly: false))
					{
						Native.PostMessage(item, 16u, IntPtr.Zero, IntPtr.Zero);
					}
					Thread.Sleep(2000);
					foreach (int pid in app.Pids)
					{
						try
						{
							Process processById = Process.GetProcessById(pid);
							if (!processById.HasExited)
							{
								processById.Kill();
							}
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
			});
		}

		public static void ShowInFolder(TrayApp app)
		{
			try
			{
				Process.Start("explorer.exe", "/select,\"" + app.Path + "\"");
			}
			catch
			{
			}
		}
	}
}
