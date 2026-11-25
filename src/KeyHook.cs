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
	internal class KeyHook
	{
		private readonly Native.LowLevelKeyboardProc proc;

		private readonly IntPtr hook;

		private readonly Config cfg;

		private readonly Audio audio;

		private readonly Dispatcher ui;

		private bool lwin;

		private bool rwin;

		private bool winAlone;

		public event Action ControlCenterKey;

		public event Action LauncherKey;

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

		public KeyHook(Config cfg, Audio audio, Dispatcher ui)
		{
			this.cfg = cfg;
			this.audio = audio;
			this.ui = ui;
			proc = OnKey;
			using (Process process = Process.GetCurrentProcess())
			{
				using (ProcessModule processModule = process.MainModule)
				{
					hook = Native.SetWindowsHookEx(13, proc, Native.GetModuleHandle(processModule.ModuleName), 0u);
				}
			}
		}

		public void Dispose()
		{
			if (hook != IntPtr.Zero)
			{
				Native.UnhookWindowsHookEx(hook);
			}
		}

		private IntPtr OnKey(int code, IntPtr wParam, IntPtr lParam)
		{
			try
			{
				if (code >= 0)
				{
					int vk = Marshal.ReadInt32(lParam);
					int num = Marshal.ReadInt32(lParam, 8);
					bool flag = (num & 0x10) != 0;
					int num2 = wParam.ToInt32();
					bool flag2 = num2 == 256 || num2 == 260;
					if (cfg.VolumeHud && audio.Available && (vk == 173 || vk == 174 || vk == 175))
					{
						if (flag2)
						{
							ui.BeginInvoke((Action)delegate
							{
								try
								{
									if (vk == 173)
									{
										audio.SetMuted(!audio.Muted, fromUi: false);
									}
									else
									{
										audio.Step((vk == 175) ? 1 : (-1));
									}
								}
								catch
								{
								}
							});
						}
						return new IntPtr(1);
					}
					if ((vk == 91 || vk == 92) && !flag)
					{
						if (flag2)
						{
							if (!lwin && !rwin)
							{
								winAlone = true;
							}
							if (vk == 91)
							{
								lwin = true;
							}
							else
							{
								rwin = true;
							}
						}
						else
						{
							if (vk == 91)
							{
								lwin = false;
							}
							else
							{
								rwin = false;
							}
							if (winAlone && !lwin && !rwin && cfg.WinKeyLauncher && this.LauncherKey != null)
							{
								winAlone = false;
								keybd_event(232, 0, 0u, UIntPtr.Zero);
								keybd_event(232, 0, 2u, UIntPtr.Zero);
								keybd_event((byte)vk, 0, 2u, UIntPtr.Zero);
								ui.BeginInvoke((Action)delegate
								{
									if (this.LauncherKey != null)
									{
										this.LauncherKey();
									}
								});
								return new IntPtr(1);
							}
							winAlone = false;
						}
					}
					else if (flag2 && (lwin || rwin) && vk != 232)
					{
						winAlone = false;
					}
					if (vk == 65 && (lwin || rwin))
					{
						if (flag2)
						{
							keybd_event(232, 0, 0u, UIntPtr.Zero);
							keybd_event(232, 0, 2u, UIntPtr.Zero);
							ui.BeginInvoke((Action)delegate
							{
								if (this.ControlCenterKey != null)
								{
									this.ControlCenterKey();
								}
							});
						}
						return new IntPtr(1);
					}
				}
			}
			catch
			{
			}
			return Native.CallNextHookEx(hook, code, wParam, lParam);
		}
	}
}
