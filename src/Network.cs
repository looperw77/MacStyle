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
	internal class WifiNetwork
	{
		public string Ssid;

		public string Profile;

		public int Signal;

		public int Auth;

		public int Cipher;

		public bool Secured;

		public bool Connected;

		public bool HasProfile;
	}
	internal static class Wifi
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct CONNPARAMS
		{
			public int mode;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string profile;

			public IntPtr ssid;

			public IntPtr bssids;

			public int bssType;

			public uint flags;
		}

		public static bool Present
		{
			get
			{
				IntPtr intPtr = Open();
				if (intPtr == IntPtr.Zero)
				{
					return false;
				}
				try
				{
					Guid g;
					return FirstInterface(intPtr, out g);
				}
				finally
				{
					WlanCloseHandle(intPtr, IntPtr.Zero);
				}
			}
		}

		[DllImport("wlanapi.dll")]
		private static extern int WlanOpenHandle(uint ver, IntPtr r, out uint negotiated, out IntPtr h);

		[DllImport("wlanapi.dll")]
		private static extern int WlanCloseHandle(IntPtr h, IntPtr r);

		[DllImport("wlanapi.dll")]
		private static extern int WlanEnumInterfaces(IntPtr h, IntPtr r, out IntPtr list);

		[DllImport("wlanapi.dll")]
		private static extern int WlanGetAvailableNetworkList(IntPtr h, ref Guid iface, uint flags, IntPtr r, out IntPtr list);

		[DllImport("wlanapi.dll")]
		private static extern int WlanScan(IntPtr h, ref Guid iface, IntPtr ssid, IntPtr ie, IntPtr r);

		[DllImport("wlanapi.dll")]
		private static extern int WlanConnect(IntPtr h, ref Guid iface, ref CONNPARAMS p, IntPtr r);

		[DllImport("wlanapi.dll")]
		private static extern int WlanDisconnect(IntPtr h, ref Guid iface, IntPtr r);

		[DllImport("wlanapi.dll", CharSet = CharSet.Unicode)]
		private static extern int WlanSetProfile(IntPtr h, ref Guid iface, uint flags, string xml, string sec, bool overwrite, IntPtr r, out uint reason);

		[DllImport("wlanapi.dll")]
		private static extern void WlanFreeMemory(IntPtr p);

		private static IntPtr Open()
		{
			if (WlanOpenHandle(2u, IntPtr.Zero, out var _, out var h) != 0)
			{
				return IntPtr.Zero;
			}
			return h;
		}

		private static bool FirstInterface(IntPtr h, out Guid g)
		{
			g = Guid.Empty;
			if (WlanEnumInterfaces(h, IntPtr.Zero, out var list) != 0)
			{
				return false;
			}
			try
			{
				int num = Marshal.ReadInt32(list, 0);
				if (num <= 0)
				{
					return false;
				}
				g = (Guid)Marshal.PtrToStructure(new IntPtr(list.ToInt64() + 8), typeof(Guid));
				return true;
			}
			finally
			{
				WlanFreeMemory(list);
			}
		}

		public static void Scan()
		{
			IntPtr intPtr = Open();
			if (intPtr == IntPtr.Zero)
			{
				return;
			}
			try
			{
				if (FirstInterface(intPtr, out var g))
				{
					WlanScan(intPtr, ref g, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
				}
			}
			finally
			{
				WlanCloseHandle(intPtr, IntPtr.Zero);
			}
		}

		public static List<WifiNetwork> Networks()
		{
			Dictionary<string, WifiNetwork> dictionary = new Dictionary<string, WifiNetwork>();
			IntPtr intPtr = Open();
			if (intPtr == IntPtr.Zero)
			{
				return new List<WifiNetwork>();
			}
			try
			{
				if (!FirstInterface(intPtr, out var g))
				{
					return new List<WifiNetwork>();
				}
				if (WlanGetAvailableNetworkList(intPtr, ref g, 0u, IntPtr.Zero, out var list) != 0)
				{
					return new List<WifiNetwork>();
				}
				try
				{
					int num = Marshal.ReadInt32(list, 0);
					for (int i = 0; i < num; i++)
					{
						IntPtr ptr = new IntPtr(list.ToInt64() + 8 + i * 628);
						string text = Marshal.PtrToStringUni(ptr);
						int val = Marshal.ReadInt32(ptr, 512);
						byte[] array = new byte[Math.Max(0, Math.Min(32, val))];
						Marshal.Copy(new IntPtr(ptr.ToInt64() + 516), array, 0, array.Length);
						string text2 = Encoding.UTF8.GetString(array);
						if (string.IsNullOrEmpty(text2))
						{
							continue;
						}
						WifiNetwork wifiNetwork = new WifiNetwork();
						wifiNetwork.Ssid = text2;
						wifiNetwork.Profile = text ?? "";
						wifiNetwork.Signal = Marshal.ReadInt32(ptr, 604);
						wifiNetwork.Secured = Marshal.ReadInt32(ptr, 608) != 0;
						wifiNetwork.Auth = Marshal.ReadInt32(ptr, 612);
						wifiNetwork.Cipher = Marshal.ReadInt32(ptr, 616);
						WifiNetwork wifiNetwork2 = wifiNetwork;
						int num2 = Marshal.ReadInt32(ptr, 620);
						wifiNetwork2.Connected = (num2 & 1) != 0;
						wifiNetwork2.HasProfile = (num2 & 2) != 0 && wifiNetwork2.Profile != "";
						if (!dictionary.TryGetValue(text2, out var value))
						{
							dictionary[text2] = wifiNetwork2;
							continue;
						}
						if (wifiNetwork2.Connected)
						{
							value.Connected = true;
						}
						if (wifiNetwork2.HasProfile && !value.HasProfile)
						{
							value.HasProfile = true;
							value.Profile = wifiNetwork2.Profile;
						}
						value.Signal = Math.Max(value.Signal, wifiNetwork2.Signal);
					}
				}
				finally
				{
					WlanFreeMemory(list);
				}
			}
			finally
			{
				WlanCloseHandle(intPtr, IntPtr.Zero);
			}
			List<WifiNetwork> list2 = dictionary.Values.ToList();
			list2.Sort(delegate(WifiNetwork a, WifiNetwork b)
			{
				if (a.Connected == b.Connected)
				{
					return b.Signal.CompareTo(a.Signal);
				}
				return (!a.Connected) ? 1 : (-1);
			});
			return list2;
		}

		public static string Connect(WifiNetwork w, string password)
		{
			IntPtr intPtr = Open();
			if (intPtr == IntPtr.Zero)
			{
				return "الواي فاي مش شغال";
			}
			try
			{
				if (!FirstInterface(intPtr, out var g))
				{
					return "مفيش كارت واي فاي";
				}
				string profile = (w.HasProfile ? w.Profile : w.Ssid);
				if (!w.HasProfile)
				{
					string text = ProfileXml(w, password);
					if (text == null)
					{
						return "نوع الحماية ده مش مدعوم، افتحه من إعدادات ويندوز";
					}
					if (WlanSetProfile(intPtr, ref g, 0u, text, null, overwrite: true, IntPtr.Zero, out var _) != 0)
					{
						return "الباسورد أو الشبكة فيها مشكلة";
					}
				}
				CONNPARAMS p = new CONNPARAMS
				{
					mode = 0,
					profile = profile,
					bssType = 1
				};
				return (WlanConnect(intPtr, ref g, ref p, IntPtr.Zero) == 0) ? null : "مقدرتش أتصل";
			}
			finally
			{
				WlanCloseHandle(intPtr, IntPtr.Zero);
			}
		}

		public static void Disconnect()
		{
			IntPtr intPtr = Open();
			if (intPtr == IntPtr.Zero)
			{
				return;
			}
			try
			{
				if (FirstInterface(intPtr, out var g))
				{
					WlanDisconnect(intPtr, ref g, IntPtr.Zero);
				}
			}
			finally
			{
				WlanCloseHandle(intPtr, IntPtr.Zero);
			}
		}

		private static string ProfileXml(WifiNetwork w, string password)
		{
			string value = ((w.Cipher == 2) ? "TKIP" : "AES");
			string text;
			switch (w.Auth)
			{
			case 1:
				text = "open";
				value = "none";
				break;
			case 4:
				text = "WPAPSK";
				break;
			case 7:
				text = "WPA2PSK";
				break;
			case 9:
				text = "WPA3SAE";
				value = "AES";
				break;
			default:
				return null;
			}
			string value2 = SecurityElement.Escape(w.Ssid);
			StringBuilder stringBuilder = new StringBuilder();
			byte[] bytes = Encoding.UTF8.GetBytes(w.Ssid);
			foreach (byte b in bytes)
			{
				stringBuilder.Append(b.ToString("X2"));
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\">");
			stringBuilder2.Append("<name>").Append(value2).Append("</name><SSIDConfig><SSID><hex>")
				.Append(stringBuilder)
				.Append("</hex><name>")
				.Append(value2)
				.Append("</name></SSID></SSIDConfig>");
			stringBuilder2.Append("<connectionType>ESS</connectionType><connectionMode>auto</connectionMode><MSM><security><authEncryption>");
			stringBuilder2.Append("<authentication>").Append(text).Append("</authentication><encryption>")
				.Append(value)
				.Append("</encryption><useOneX>false</useOneX></authEncryption>");
			if (text != "open")
			{
				stringBuilder2.Append("<sharedKey><keyType>passPhrase</keyType><protected>false</protected><keyMaterial>").Append(SecurityElement.Escape(password ?? "")).Append("</keyMaterial></sharedKey>");
			}
			stringBuilder2.Append("</security></MSM></WLANProfile>");
			return stringBuilder2.ToString();
		}
	}
	internal static class Radios
	{
		public static async Task<Radio> Get(RadioKind kind)
		{
			try
			{
				await Radio.RequestAccessAsync();
				foreach (Radio item in await Radio.GetRadiosAsync())
				{
					if (item.Kind == kind)
					{
						return item;
					}
				}
			}
			catch (Exception ex)
			{
				Sys.Log("radio: " + ex.Message);
			}
			return null;
		}

		public static async Task<bool> Set(Radio r, bool on)
		{
			int num = default(int);
			_ = num;
			_ = 0;
			try
			{
				return await r.SetStateAsync(on ? RadioState.On : RadioState.Off) == RadioAccessStatus.Allowed;
			}
			catch
			{
				return false;
			}
		}
	}
	internal static class Brightness
	{
		private static int pending = -1;

		private static int running;

		public static int Get()
		{
			try
			{
				using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\wmi", "SELECT CurrentBrightness FROM WmiMonitorBrightness"))
				{
					using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator())
					{
						if (managementObjectEnumerator.MoveNext())
						{
							ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
							return Convert.ToInt32(managementObject["CurrentBrightness"]);
						}
					}
				}
			}
			catch
			{
			}
			return -1;
		}

		public static void Set(int v)
		{
			pending = v;
			if (Interlocked.Exchange(ref running, 1) == 1)
			{
				return;
			}
			Task.Run(delegate
			{
				try
				{
					int num = -2;
					while (pending != num)
					{
						num = pending;
						using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM WmiMonitorBrightnessMethods"))
						{
							using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator())
							{
								if (managementObjectEnumerator.MoveNext())
								{
									ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
									managementObject.InvokeMethod("WmiSetBrightness", new object[2]
									{
										1u,
										(byte)num
									});
								}
							}
						}
					}
				}
				catch
				{
				}
				finally
				{
					Interlocked.Exchange(ref running, 0);
				}
			});
		}
	}
}
