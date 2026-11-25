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
	internal static class Sys
	{
		public static int Run(string file, string args, int timeoutMs)
		{
			try
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo(file, args);
				processStartInfo.CreateNoWindow = true;
				processStartInfo.UseShellExecute = false;
				processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				ProcessStartInfo startInfo = processStartInfo;
				using (Process process = Process.Start(startInfo))
				{
					if (!process.WaitForExit(timeoutMs))
					{
						try
						{
							process.Kill();
						}
						catch
						{
						}
						return -1;
					}
					return process.ExitCode;
				}
			}
			catch
			{
				return -2;
			}
		}

		public static void Kill(string name)
		{
			Process[] processesByName = Process.GetProcessesByName(name);
			foreach (Process process in processesByName)
			{
				if (process.Id != Process.GetCurrentProcess().Id)
				{
					try
					{
						process.Kill();
						process.WaitForExit(4000);
					}
					catch
					{
					}
				}
			}
		}

		public static bool IsRunning(string name)
		{
			return Process.GetProcessesByName(name).Length > 0;
		}

		public static bool PackageInstalled(string familyPrefix)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\Repository\\Packages"))
				{
					if (registryKey == null)
					{
						return false;
					}
					string[] subKeyNames = registryKey.GetSubKeyNames();
					foreach (string text in subKeyNames)
					{
						if (text.StartsWith(familyPrefix, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}
			}
			catch
			{
			}
			return false;
		}

		public static void Log(string msg)
		{
			try
			{
				Directory.CreateDirectory(Brand.DataDir);
				string text = System.IO.Path.Combine(Brand.DataDir, "log.txt");
				if (File.Exists(text) && new FileInfo(text).Length > 300000)
				{
					File.Delete(text);
				}
				File.AppendAllText(text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + msg + "\r\n");
			}
			catch
			{
			}
		}
	}
	internal static class FilesApp
	{
		public static bool Installed => Sys.PackageInstalled("Files_");

		public static bool Apply(Config c)
		{
			if (Sys.IsRunning("Files"))
			{
				return false;
			}
			try
			{
				string text = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages\\Files_1y0xx7n9077q4\\LocalState\\settings");
				Directory.CreateDirectory(text);
				string path = System.IO.Path.Combine(text, "user_settings.json");
				JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
				Dictionary<string, object> dictionary = null;
				try
				{
					if (File.Exists(path))
					{
						dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));
					}
				}
				catch
				{
				}
				if (dictionary == null)
				{
					dictionary = new Dictionary<string, object>();
				}
				dictionary["AppThemeMode"] = "Dark";
				dictionary["AppThemeBackdropMaterial"] = 3;
				dictionary["AppThemeBackgroundColor"] = "#32" + ColorUtil.Hex6(c.AccentColor);
				dictionary["AppThemeFontFamily"] = "Segoe UI Variable";
				dictionary["DefaultLayoutMode"] = 3;
				dictionary["ShowRunningAsAdminPrompt"] = false;
				dictionary["ShowBackgroundRunningNotification"] = false;
				File.WriteAllText(path, javaScriptSerializer.Serialize(dictionary), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
				return true;
			}
			catch (Exception ex)
			{
				Sys.Log("files: " + ex.Message);
				return false;
			}
		}
	}
}
