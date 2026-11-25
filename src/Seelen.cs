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
	internal static class Seelen
	{
		private const string Template = "{\"activeIconPacks\":[\"@system/icon-pack\"],\"activeThemes\":[\"@default/theme\"],\"backupSyncEnabled\":false,\"byTheme\":{},\"byWallpaper\":{},\"byWidget\":{},\"dateFormat\":\"ddd D MMM, hh:mm A\",\"devTools\":false,\"drpc\":false,\"hardwareAcceleration\":true,\"language\":\"en\",\"monitorsV3\":{},\"oldActiveThemes\":[],\"performanceMode\":{\"default\":\"Disabled\",\"onBattery\":\"Minimal\",\"onEnergySaver\":\"Extreme\"},\"pollingInterval\":3,\"startOfWeek\":\"Monday\",\"streamingMode\":false,\"suspendOnGameMode\":false,\"unlockShortcuts\":false,\"unstableOptimizations\":false,\"updater\":{\"channel\":\"Release\"},\"wallpaperCollections\":[]}";

		public static string Dir => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.seelen.seelen-ui");

		public static bool Installed => Sys.PackageInstalled("Seelen.SeelenUI_");

		public static void Start()
		{
			try
			{
				Process.Start("explorer.exe", "shell:AppsFolder\\Seelen.SeelenUI_p6yyn03m1894e!App");
			}
			catch
			{
			}
		}

		public static void Restart()
		{
			Sys.Kill("seelen-ui");
			Thread.Sleep(900);
			Start();
		}

		public static string WegCss(Config c)
		{
			System.Windows.Media.Color accentColor = c.AccentColor;
			System.Windows.Media.Color color = ColorUtil.Mix(System.Windows.Media.Color.FromRgb(22, 18, 30), accentColor, 0.1);
			double dockMagnify = c.DockMagnify;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			double num = c.DockIconSize;
			double[] array = new double[4] { 1.0, 0.62, 0.3, 0.1 };
			string[] array2 = new string[4];
			string[] array3 = new string[4];
			string[] array4 = new string[4];
			for (int i = 0; i < 4; i++)
			{
				double num2 = 1.0 + (dockMagnify - 1.0) * array[i];
				array2[i] = num2.ToString("0.000", invariantCulture);
				array3[i] = ((0.0 - (num2 - 1.0)) * 12.0).ToString("0.0", invariantCulture);
				array4[i] = ((num2 - 1.0) * num / 2.0).ToString("0.0", invariantCulture);
			}
			string text = string.Format(invariantCulture, "rgba({0}, {1}, {2}, 0.95)", accentColor.R, accentColor.G, accentColor.B);
			string text2 = string.Format(invariantCulture, "rgba({0}, {1}, {2}, 0.58)", color.R, color.G, color.B);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("/* MacStyle by Ahmed Kamel - generated, edit from MacStyle settings */");
			stringBuilder.AppendLine(".taskbar, .taskbar > .weg-items-container { overflow: visible !important; }");
			stringBuilder.AppendLine(".taskbar { background-color: " + text2 + " !important; border: 1px solid rgba(255,255,255,0.13); box-shadow: 0 12px 32px rgba(0,0,0,0.45), inset 0 1px 0 rgba(255,255,255,0.08); }");
			stringBuilder.AppendLine(".taskbar > .weg-items-container { gap: 0 !important; }");
			stringBuilder.AppendLine(".weg-item-drag-container { padding: 0 4px; margin: 0; transition: margin 190ms cubic-bezier(0.25, 1, 0.5, 1); }");
			stringBuilder.AppendLine(".weg-item { background-color: transparent !important; box-shadow: none !important; overflow: visible !important; transform-origin: 50% 100%; will-change: scale, translate;");
			stringBuilder.AppendLine("  transition: scale 190ms cubic-bezier(0.25, 1, 0.5, 1), translate 190ms cubic-bezier(0.25, 1, 0.5, 1), filter 160ms ease, width 200ms ease, height 200ms ease, padding 200ms ease !important; }");
			stringBuilder.AppendLine(".weg-item:hover { background-color: transparent !important; }");
			if (dockMagnify > 1.001)
			{
				string[][] array5 = new string[4][]
				{
					new string[1] { ".weg-item-drag-container:hover" },
					new string[2] { ".weg-item-drag-container:hover + .weg-item-drag-container", ".weg-item-drag-container:has(+ .weg-item-drag-container:hover)" },
					new string[2] { ".weg-item-drag-container:hover + * + .weg-item-drag-container", ".weg-item-drag-container:has(+ * + .weg-item-drag-container:hover)" },
					new string[2] { ".weg-item-drag-container:hover + * + * + .weg-item-drag-container", ".weg-item-drag-container:has(+ * + * + .weg-item-drag-container:hover)" }
				};
				for (int num3 = 3; num3 >= 0; num3--)
				{
					string text3 = string.Join(", ", array5[num3]);
					List<string> list = new List<string>();
					string[] array6 = array5[num3];
					foreach (string text4 in array6)
					{
						list.Add(text4 + " .weg-item");
					}
					stringBuilder.AppendLine(text3 + " { margin: 0 " + array4[num3] + "px; }");
					stringBuilder.AppendLine(string.Join(", ", list) + " { scale: " + array2[num3] + "; translate: 0 " + array3[num3] + "px; }");
				}
			}
			stringBuilder.AppendLine(".weg-item:active { transform: none !important; filter: brightness(0.6); }");
			stringBuilder.AppendLine("@keyframes mac-bounce { 0% { translate: 0 0; } 14% { translate: 0 -20px; } 28% { translate: 0 0; } 42% { translate: 0 -12px; } 56% { translate: 0 0; } 70% { translate: 0 -5px; } 84% { translate: 0 0; } 100% { translate: 0 0; } }");
			stringBuilder.AppendLine(".weg-item-drag-container:has(.weg-item-open-sign-active) .weg-item { animation: mac-bounce 1.2s cubic-bezier(0.3,0.7,0.4,1) 1; }");
			stringBuilder.AppendLine(".weg-item-open-sign { width: 4px !important; height: 4px !important; border-radius: 50% !important; }");
			stringBuilder.AppendLine(".weg-item-open-sign.weg-item-open-sign-active, .weg-item-open-sign.weg-item-open-sign-focused { background-color: " + text + " !important; }");
			stringBuilder.AppendLine(".weg-separator { opacity: 0.35; }");
			return stringBuilder.ToString();
		}

		public static void WriteTheme(Config c)
		{
			try
			{
				string path = System.IO.Path.Combine(Dir, "themes\\macstyle");
				Directory.CreateDirectory(System.IO.Path.Combine(path, "styles"));
				File.WriteAllText(System.IO.Path.Combine(path, "metadata.yml"), "id: \"@ahmedkamel/macstyle\"\nmetadata:\n  displayName: MacStyle\n  description: MacStyle by Ahmed Kamel - macOS dock look.\n  tags:\n    - weg\nstyles:\n  \"@seelen/weg\": !include styles/weg.css\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
				File.WriteAllText(System.IO.Path.Combine(path, "styles\\weg.css"), WegCss(c), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			}
			catch (Exception ex)
			{
				Sys.Log("theme: " + ex.Message);
			}
		}

		private static Dictionary<string, object> Sub(Dictionary<string, object> d, string key)
		{
			if (d.TryGetValue(key, out var value) && value is Dictionary<string, object>)
			{
				return (Dictionary<string, object>)value;
			}
			return (Dictionary<string, object>)(d[key] = new Dictionary<string, object>());
		}

		public static void PatchSettings(Config c, bool includeTheme)
		{
			try
			{
				Directory.CreateDirectory(Dir);
				string path = System.IO.Path.Combine(Dir, "settings.json");
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
					dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>("{\"activeIconPacks\":[\"@system/icon-pack\"],\"activeThemes\":[\"@default/theme\"],\"backupSyncEnabled\":false,\"byTheme\":{},\"byWallpaper\":{},\"byWidget\":{},\"dateFormat\":\"ddd D MMM, hh:mm A\",\"devTools\":false,\"drpc\":false,\"hardwareAcceleration\":true,\"language\":\"en\",\"monitorsV3\":{},\"oldActiveThemes\":[],\"performanceMode\":{\"default\":\"Disabled\",\"onBattery\":\"Minimal\",\"onEnergySaver\":\"Extreme\"},\"pollingInterval\":3,\"startOfWeek\":\"Monday\",\"streamingMode\":false,\"suspendOnGameMode\":false,\"unlockShortcuts\":false,\"unstableOptimizations\":false,\"updater\":{\"channel\":\"Release\"},\"wallpaperCollections\":[]}");
				}
				ArrayList arrayList = new ArrayList();
				if (dictionary.TryGetValue("activeThemes", out var value) && value is ArrayList)
				{
					arrayList = (ArrayList)value;
				}
				if (!arrayList.Contains("@default/theme"))
				{
					arrayList.Insert(0, "@default/theme");
				}
				arrayList.Remove("@local/mac-dock");
				if (includeTheme)
				{
					if (!arrayList.Contains("@ahmedkamel/macstyle"))
					{
						arrayList.Add("@ahmedkamel/macstyle");
					}
				}
				else
				{
					arrayList.Remove("@ahmedkamel/macstyle");
				}
				dictionary["activeThemes"] = arrayList;
				Dictionary<string, object> d = Sub(dictionary, "byWidget");
				Dictionary<string, object> dictionary2 = Sub(d, "@seelen/weg");
				dictionary2["enabled"] = true;
				dictionary2["position"] = "Bottom";
				dictionary2["mode"] = "MinContent";
				dictionary2["hideMode"] = (c.DockAutoHide ? "Always" : "Never");
				dictionary2["size"] = c.DockIconSize;
				dictionary2["margin"] = 8;
				dictionary2["padding"] = 8;
				dictionary2["spaceBetweenItems"] = 8;
				dictionary2["delayToShow"] = 100;
				dictionary2["delayToHide"] = 800;
				dictionary2["showInstanceCounter"] = true;
				dictionary2["showWindowTitle"] = false;
				Sub(d, "@seelen/fancy-toolbar")["enabled"] = false;
				Sub(d, "@seelen/wallpaper-manager")["enabled"] = false;
				Sub(d, "@seelen/window-manager")["enabled"] = false;
				File.WriteAllText(path, javaScriptSerializer.Serialize(dictionary), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
				string text = System.IO.Path.Combine(Dir, "data\\seelen-settings");
				Directory.CreateDirectory(text);
				string path2 = System.IO.Path.Combine(text, "welcomeModal.json");
				if (!File.Exists(path2))
				{
					File.WriteAllText(path2, "{\"welcomeShown\":true,\"alreadyReviewed\":true,\"lastReviewPrompt\":0}");
				}
			}
			catch (Exception ex)
			{
				Sys.Log("seelen settings: " + ex.Message);
			}
		}

		public static bool WegEnabled()
		{
			try
			{
				string path = System.IO.Path.Combine(Dir, "settings.json");
				if (!File.Exists(path))
				{
					return false;
				}
				Dictionary<string, object> dictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(path));
				if (!dictionary.TryGetValue("byWidget", out var value) || !(value is Dictionary<string, object>))
				{
					return true;
				}
				if (!((Dictionary<string, object>)value).TryGetValue("@seelen/weg", out var value2) || !(value2 is Dictionary<string, object>))
				{
					return true;
				}
				if (!((Dictionary<string, object>)value2).TryGetValue("enabled", out var value3))
				{
					return true;
				}
				return !(value3 is bool) || (bool)value3;
			}
			catch
			{
				return false;
			}
		}

		public static void DisableDock()
		{
			try
			{
				string path = System.IO.Path.Combine(Dir, "settings.json");
				if (File.Exists(path))
				{
					JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
					Dictionary<string, object> dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));
					Sub(Sub(dictionary, "byWidget"), "@seelen/weg")["enabled"] = false;
					if (dictionary.TryGetValue("activeThemes", out var value) && value is ArrayList)
					{
						((ArrayList)value).Remove("@ahmedkamel/macstyle");
					}
					File.WriteAllText(path, javaScriptSerializer.Serialize(dictionary), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
					string path2 = System.IO.Path.Combine(Dir, "themes\\macstyle");
					if (Directory.Exists(path2))
					{
						Directory.Delete(path2, recursive: true);
					}
				}
			}
			catch (Exception ex)
			{
				Sys.Log("seelen disable: " + ex.Message);
			}
		}

		public static void RemoveTheme()
		{
			try
			{
				string path = System.IO.Path.Combine(Dir, "themes\\macstyle");
				if (Directory.Exists(path))
				{
					Directory.Delete(path, recursive: true);
				}
				string path2 = System.IO.Path.Combine(Dir, "settings.json");
				if (File.Exists(path2))
				{
					JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
					Dictionary<string, object> dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path2));
					if (dictionary.TryGetValue("activeThemes", out var value) && value is ArrayList)
					{
						((ArrayList)value).Remove("@ahmedkamel/macstyle");
					}
					File.WriteAllText(path2, javaScriptSerializer.Serialize(dictionary), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
				}
			}
			catch
			{
			}
		}
	}
}
