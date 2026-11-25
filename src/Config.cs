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
	internal static class Brand
	{
		public const string Name = "MacStyle";

		public const string Author = "Ahmed Kamel";

		public const string Version = "2.0";

		public const string ThemeId = "@ahmedkamel/macstyle";

		public const string ThemeFolder = "macstyle";

		public const string SeelenAppId = "Seelen.SeelenUI_p6yyn03m1894e!App";

		public const string FilesFamily = "Files_1y0xx7n9077q4";

		public static readonly string[][] Colors = new string[9][]
		{
			new string[2] { "بنفسجي", "#A76EFF" },
			new string[2] { "أزرق", "#3B82F6" },
			new string[2] { "سماوي", "#06B6D4" },
			new string[2] { "أخضر", "#22C55E" },
			new string[2] { "أصفر", "#F5B70B" },
			new string[2] { "برتقالي", "#F97316" },
			new string[2] { "وردي", "#EC4899" },
			new string[2] { "أحمر", "#EF4444" },
			new string[2] { "جرافيت", "#9CA3AF" }
		};

		public static string InstallDir => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\MacStyle");

		public static string DataDir => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MacStyle");
	}
	internal class Config
	{
		public string Accent = "#A76EFF";

		public bool IslandEnabled = true;

		public bool IslandAutoLine = true;

		public bool Clock24;

		public bool DockEnabled = true;

		public bool DockLine = true;

		public bool DockAutoHide = true;

		public double DockMagnify = 1.55;

		public int DockIconSize = 46;

		public bool PasteEnabled = true;

		public bool Genie = true;

		public bool VolumeHud = true;

		public bool WinKeyLauncher = true;

		public string PasteFormat = "png";

		public bool NotifyEnabled = true;

		public int NotifySeconds = 5;

		public double NotifyScale = 1.0;

		public string NotifyColor = "";

		public string NotifyMuted = "";

		private static string FilePath => System.IO.Path.Combine(Brand.DataDir, "config.ini");

		public System.Windows.Media.Color AccentColor => ColorUtil.FromHex(Accent);

		public System.Windows.Media.Color NotifyAccent => ColorUtil.FromHex((NotifyColor == "") ? Accent : NotifyColor);

		public static Config Load()
		{
			Config config = new Config();
			try
			{
				if (!File.Exists(FilePath))
				{
					return config;
				}
				string[] array = File.ReadAllLines(FilePath);
				foreach (string text in array)
				{
					int num = text.IndexOf('=');
					if (num < 0)
					{
						continue;
					}
					string text2 = text.Substring(0, num).Trim();
					string text3 = text.Substring(num + 1).Trim();
					switch (text2)
					{
					case "accent":
						if (text3.StartsWith("#") && text3.Length == 7)
						{
							config.Accent = text3.ToUpperInvariant();
						}
						break;
					case "islandEnabled":
						config.IslandEnabled = text3 == "1";
						break;
					case "islandAutoLine":
						config.IslandAutoLine = text3 == "1";
						break;
					case "clock24":
						config.Clock24 = text3 == "1";
						break;
					case "dockEnabled":
						config.DockEnabled = text3 == "1";
						break;
					case "dockLine":
						config.DockLine = text3 == "1";
						break;
					case "dockAutoHide":
						config.DockAutoHide = text3 == "1";
						break;
					case "dockMagnify":
					{
						if (double.TryParse(text3, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
						{
							config.DockMagnify = Math.Max(1.0, Math.Min(2.0, result2));
						}
						break;
					}
					case "dockIconSize":
					{
						if (int.TryParse(text3, out var result))
						{
							config.DockIconSize = Math.Max(32, Math.Min(72, result));
						}
						break;
					}
					case "pasteEnabled":
						config.PasteEnabled = text3 == "1";
						break;
					case "pasteFormat":
						config.PasteFormat = ((text3 == "jpg") ? "jpg" : "png");
						break;
					case "genie":
						config.Genie = text3 == "1";
						break;
					case "volumeHud":
						config.VolumeHud = text3 == "1";
						break;
					case "winKeyLauncher":
						config.WinKeyLauncher = text3 == "1";
						break;
					case "notifyEnabled":
						config.NotifyEnabled = text3 == "1";
						break;
					case "notifySeconds":
					{
						if (int.TryParse(text3, out var result3))
						{
							config.NotifySeconds = Math.Max(2, Math.Min(15, result3));
						}
						break;
					}
					case "notifyScale":
					{
						if (double.TryParse(text3, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
						{
							config.NotifyScale = Math.Max(0.8, Math.Min(1.3, result4));
						}
						break;
					}
					case "notifyColor":
						config.NotifyColor = ((text3.StartsWith("#") && text3.Length == 7) ? text3.ToUpperInvariant() : "");
						break;
					case "notifyMuted":
						config.NotifyMuted = text3;
						break;
					}
				}
			}
			catch
			{
			}
			return config;
		}

		public void Save()
		{
			try
			{
				Directory.CreateDirectory(Brand.DataDir);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("accent=" + Accent);
				stringBuilder.AppendLine("islandEnabled=" + B(IslandEnabled));
				stringBuilder.AppendLine("islandAutoLine=" + B(IslandAutoLine));
				stringBuilder.AppendLine("clock24=" + B(Clock24));
				stringBuilder.AppendLine("dockEnabled=" + B(DockEnabled));
				stringBuilder.AppendLine("dockLine=" + B(DockLine));
				stringBuilder.AppendLine("dockAutoHide=" + B(DockAutoHide));
				stringBuilder.AppendLine("dockMagnify=" + DockMagnify.ToString("0.00", CultureInfo.InvariantCulture));
				stringBuilder.AppendLine("dockIconSize=" + DockIconSize);
				stringBuilder.AppendLine("pasteEnabled=" + B(PasteEnabled));
				stringBuilder.AppendLine("pasteFormat=" + PasteFormat);
				stringBuilder.AppendLine("genie=" + B(Genie));
				stringBuilder.AppendLine("volumeHud=" + B(VolumeHud));
				stringBuilder.AppendLine("winKeyLauncher=" + B(WinKeyLauncher));
				stringBuilder.AppendLine("notifyEnabled=" + B(NotifyEnabled));
				stringBuilder.AppendLine("notifySeconds=" + NotifySeconds);
				stringBuilder.AppendLine("notifyScale=" + NotifyScale.ToString("0.00", CultureInfo.InvariantCulture));
				stringBuilder.AppendLine("notifyColor=" + NotifyColor);
				stringBuilder.AppendLine("notifyMuted=" + NotifyMuted);
				File.WriteAllText(FilePath, stringBuilder.ToString());
			}
			catch
			{
			}
		}

		private static string B(bool b)
		{
			if (!b)
			{
				return "0";
			}
			return "1";
		}
	}
	internal static class ColorUtil
	{
		public static System.Windows.Media.Color FromHex(string hex)
		{
			try
			{
				return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
			}
			catch
			{
				return System.Windows.Media.Color.FromRgb(167, 110, byte.MaxValue);
			}
		}

		public static System.Windows.Media.Color Mix(System.Windows.Media.Color a, System.Windows.Media.Color b, double t)
		{
			return System.Windows.Media.Color.FromArgb(byte.MaxValue, (byte)((double)(int)a.R + (double)(b.R - a.R) * t), (byte)((double)(int)a.G + (double)(b.G - a.G) * t), (byte)((double)(int)a.B + (double)(b.B - a.B) * t));
		}

		public static string Hex6(System.Windows.Media.Color c)
		{
			return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}

		public static LinearGradientBrush Gradient(System.Windows.Media.Color accent, double angle)
		{
			return new LinearGradientBrush(accent, Mix(accent, Colors.White, 0.38), angle);
		}
	}
}
