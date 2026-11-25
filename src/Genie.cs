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
	internal static class Dwm
	{
		public struct SIZE
		{
			public int cx;

			public int cy;
		}

		public struct MARGINS
		{
			public int l;

			public int r;

			public int t;

			public int b;
		}

		public struct THUMB_PROPS
		{
			public int dwFlags;

			public Native.RECT rcDestination;

			public Native.RECT rcSource;

			public byte opacity;

			[MarshalAs(UnmanagedType.Bool)]
			public bool fVisible;

			[MarshalAs(UnmanagedType.Bool)]
			public bool fSourceClientAreaOnly;
		}

		public delegate void WinEventProc(IntPtr hook, uint ev, IntPtr hwnd, int idObject, int idChild, uint thread, uint time);

		public struct WINDOWPLACEMENT
		{
			public int length;

			public int flags;

			public int showCmd;

			public Native.POINT min;

			public Native.POINT max;

			public Native.RECT normal;
		}

		public struct ANIMATIONINFO
		{
			public uint cbSize;

			public int iMinAnimate;
		}

		public static bool WindowsMinAnimation
		{
			get
			{
				ANIMATIONINFO info = new ANIMATIONINFO
				{
					cbSize = (uint)Marshal.SizeOf(typeof(ANIMATIONINFO))
				};
				SystemParametersInfo(72u, info.cbSize, ref info, 0u);
				return info.iMinAnimate != 0;
			}
			set
			{
				ANIMATIONINFO info = new ANIMATIONINFO
				{
					cbSize = (uint)Marshal.SizeOf(typeof(ANIMATIONINFO)),
					iMinAnimate = (value ? 1 : 0)
				};
				SystemParametersInfo(73u, info.cbSize, ref info, 3u);
			}
		}

		[DllImport("dwmapi.dll")]
		public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

		[DllImport("dwmapi.dll")]
		public static extern int DwmUnregisterThumbnail(IntPtr thumb);

		[DllImport("dwmapi.dll")]
		public static extern int DwmUpdateThumbnailProperties(IntPtr thumb, ref THUMB_PROPS props);

		[DllImport("dwmapi.dll")]
		public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out SIZE size);

		[DllImport("dwmapi.dll")]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS m);

		[DllImport("dwmapi.dll")]
		public static extern int DwmFlush();

		[DllImport("user32.dll")]
		public static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr mod, WinEventProc proc, uint pid, uint tid, uint flags);

		[DllImport("user32.dll")]
		public static extern bool UnhookWinEvent(IntPtr hook);

		[DllImport("user32.dll")]
		public static extern bool GetWindowPlacement(IntPtr h, ref WINDOWPLACEMENT wp);

		[DllImport("user32.dll")]
		public static extern bool SystemParametersInfo(uint action, uint param, ref ANIMATIONINFO info, uint winIni);
	}
	internal class Genie
	{
		private readonly Config cfg;

		private readonly Dwm.WinEventProc proc;

		private readonly IntPtr hook;

		private readonly uint selfPid = (uint)Process.GetCurrentProcess().Id;

		private DateTime lastEvent = DateTime.MinValue;

		public int Running;

		public static Genie Current;

		public Genie(Config cfg)
		{
			Current = this;
			this.cfg = cfg;
			proc = OnEvent;
			hook = Dwm.SetWinEventHook(22u, 22u, IntPtr.Zero, proc, 0u, 0u, 2u);
			Apply();
		}

		public void Apply()
		{
			try
			{
				if (Dwm.WindowsMinAnimation == cfg.Genie)
				{
					Dwm.WindowsMinAnimation = !cfg.Genie;
				}
			}
			catch
			{
			}
		}

		public void Dispose()
		{
			if (hook != IntPtr.Zero)
			{
				Dwm.UnhookWinEvent(hook);
			}
		}

		private void OnEvent(IntPtr h, uint ev, IntPtr hwnd, int idObject, int idChild, uint thread, uint time)
		{
			try
			{
				if (!cfg.Genie || idObject != 0 || hwnd == IntPtr.Zero)
				{
					return;
				}
				Stopwatch sinceEvent = Stopwatch.StartNew();
				DateTime now = DateTime.Now;
				bool flag = (now - lastEvent).TotalMilliseconds < 160.0;
				lastEvent = now;
				if (flag || Running > 1 || (Native.GetWindowLong(hwnd, -20) & 0x80) != 0 || (Native.GetWindowLong(hwnd, -16) & 0xC00000) != 12582912)
				{
					return;
				}
				switch (Native.ClassOf(hwnd))
				{
				case "Progman":
					return;
				case "WorkerW":
					return;
				case "Shell_TrayWnd":
					return;
				}
				Native.GetWindowPid(hwnd, out var pid);
				if (pid == selfPid)
				{
					return;
				}
				Dwm.WINDOWPLACEMENT wp = default(Dwm.WINDOWPLACEMENT);
				wp.length = Marshal.SizeOf(wp);
				if (!Dwm.GetWindowPlacement(hwnd, ref wp))
				{
					return;
				}
				Native.MONITORINFO mi = default(Native.MONITORINFO);
				mi.cbSize = Marshal.SizeOf(mi);
				if (!Native.GetMonitorInfo(Native.MonitorFromWindow(hwnd, 2u), ref mi))
				{
					return;
				}
				Native.RECT win;
				if ((wp.flags & 2) != 0)
				{
					win = mi.rcWork;
				}
				else
				{
					win = wp.normal;
					win.L += mi.rcWork.L;
					win.R += mi.rcWork.L;
					win.T += mi.rcWork.T;
					win.B += mi.rcWork.T;
				}
				if (win.R - win.L >= 80 && win.B - win.T >= 60)
				{
					double tx = (double)(mi.rcMonitor.R - mi.rcMonitor.L) / 2.0;
					double ty = mi.rcMonitor.B - mi.rcMonitor.T - 26;
					double tw = 54.0;
					if (Dock.Current != null && Dock.Current.TargetFor(hwnd, out var px, out var py, out var pw))
					{
						tx = px - (double)mi.rcMonitor.L;
						ty = py - (double)mi.rcMonitor.T;
						tw = pw;
					}
					GenieWindow genieWindow = new GenieWindow(this, hwnd, win, mi.rcMonitor, tx, ty, tw);
					genieWindow.sinceEvent = sinceEvent;
					genieWindow.Show();
				}
			}
			catch (Exception ex)
			{
				Sys.Log("genie: " + ex.Message);
			}
		}
	}
	internal class GenieWindow : Window
	{
		private const int Strips = 60;

		private const double Duration = 560.0;

		private readonly Genie owner;

		private readonly IntPtr src;

		private readonly Native.RECT win;

		private readonly Native.RECT mon;

		private readonly double tx;

		private readonly double ty;

		private readonly double tw;

		private double ox;

		private double oy;

		private readonly IntPtr[] thumbs = new IntPtr[60];

		private IntPtr me;

		private Stopwatch clock;

		private double srcW;

		private double srcH;

		private bool done;

		public Stopwatch sinceEvent;

		private int frames;

		private double frameMs;

		private long startDelay;

		public GenieWindow(Genie owner, IntPtr src, Native.RECT win, Native.RECT mon, double tx, double ty, double tw)
		{
			GenieWindow genieWindow = this;
			this.tx = tx;
			this.ty = ty;
			this.tw = tw;
			this.owner = owner;
			this.src = src;
			this.win = win;
			this.mon = mon;
			owner.Running++;
			base.WindowStyle = WindowStyle.None;
			base.ResizeMode = ResizeMode.NoResize;
			base.ShowInTaskbar = false;
			base.ShowActivated = false;
			base.Topmost = true;
			base.Background = System.Windows.Media.Brushes.Transparent;
			base.WindowStartupLocation = WindowStartupLocation.Manual;
			EventHandler value = delegate
			{
				genieWindow.me = new WindowInteropHelper(genieWindow).Handle;
				HwndSource hwndSource = HwndSource.FromHwnd(genieWindow.me);
				if (hwndSource != null && hwndSource.CompositionTarget != null)
				{
					hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
				}
				Dwm.MARGINS m = new Dwm.MARGINS
				{
					l = -1,
					r = -1,
					t = -1,
					b = -1
				};
				Dwm.DwmExtendFrameIntoClientArea(genieWindow.me, ref m);
				int windowLong = Native.GetWindowLong(genieWindow.me, -20);
				Native.SetWindowLong(genieWindow.me, -20, windowLong | 0x80 | 0x8000000 | 0x20);
				int num = (int)Math.Max(mon.L, Math.Min(win.L, (double)mon.L + tx - tw) - 4.0);
				int num2 = Math.Max(mon.T, win.T - 4);
				int num3 = (int)Math.Min(mon.R, Math.Max(win.R, (double)mon.L + tx + tw) + 4.0);
				int num4 = (int)Math.Min(mon.B, Math.Max(win.B, (double)mon.T + ty + 10.0) + 4.0);
				genieWindow.ox = num - mon.L;
				genieWindow.oy = num2 - mon.T;
				Native.SetWindowPos(genieWindow.me, new IntPtr(-1), num, num2, num3 - num, num4 - num2, 80u);
			};
			base.SourceInitialized += value;
			base.Loaded += delegate
			{
				Begin();
			};
		}

		private void Begin()
		{
			for (int i = 0; i < 60; i++)
			{
				if (Dwm.DwmRegisterThumbnail(me, src, out thumbs[i]) != 0)
				{
					Finish();
					return;
				}
			}
			Dwm.DwmQueryThumbnailSourceSize(thumbs[0], out var size);
			srcW = ((size.cx > 0) ? size.cx : (win.R - win.L));
			srcH = ((size.cy > 0) ? size.cy : (win.B - win.T));
			clock = Stopwatch.StartNew();
			startDelay = ((sinceEvent != null) ? sinceEvent.ElapsedMilliseconds : (-1));
			Frame(0.0);
			Thread thread = new Thread((ThreadStart)delegate
			{
				try
				{
					while (true)
					{
						Dwm.DwmFlush();
						double num = clock.Elapsed.TotalMilliseconds / 560.0;
						if (num >= 1.0)
						{
							break;
						}
						Stopwatch stopwatch = Stopwatch.StartNew();
						Frame(num);
						frameMs += stopwatch.Elapsed.TotalMilliseconds;
						frames++;
					}
				}
				catch
				{
				}
				base.Dispatcher.BeginInvoke((Action)delegate
				{
					Sys.Log(string.Format(CultureInfo.InvariantCulture, "genie: start {0}ms, {1} frames, {2:0.0}ms per frame", startDelay, frames, (frames > 0) ? (frameMs / (double)frames) : 0.0));
					Finish();
				});
			});
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Highest;
			thread.Start();
		}

		private static double Smooth(double a, double b, double x)
		{
			double num = Math.Max(0.0, Math.Min(1.0, (x - a) / (b - a)));
			return num * num * (3.0 - 2.0 * num);
		}

		private void Frame(double t)
		{
			double num = win.L - mon.L;
			double num2 = win.T - mon.T;
			double num3 = win.R - win.L;
			double num4 = win.B - win.T;
			double num5 = ((t < 0.5) ? (4.0 * t * t * t) : (1.0 - Math.Pow(-2.0 * t + 2.0, 3.0) / 2.0));
			double num6 = Smooth(0.0, 0.38, t);
			byte opacity = (byte)(255.0 * (1.0 - Smooth(0.86, 1.0, t)));
			double num7 = Math.Max(1.0, ty - num2);
			for (int i = 0; i < 60; i++)
			{
				double num8 = (double)i / 60.0;
				double num9 = (double)(i + 1) / 60.0;
				double num10 = Math.Max(0.0, Math.Min(1.0, num5 * 1.55 - (1.0 - num8) * 0.55));
				double num11 = Math.Max(0.0, Math.Min(1.0, num5 * 1.55 - (1.0 - num9) * 0.55));
				double num12 = num2 + num4 * num8;
				double num13 = num2 + num4 * num9;
				double num14 = num12 + (ty - num12) * num10;
				double num15 = num13 + (ty - num13) * num11;
				double num16 = (num14 + num15) / 2.0;
				double num17 = Math.Max(0.0, Math.Min(1.0, (num16 - num2) / num7));
				double num18 = Math.Max(num6 * num17 * num17 * (3.0 - 2.0 * num17), Math.Min(num10, num11));
				double num19 = num + (tx - tw / 2.0 - num) * num18;
				double num20 = num + num3 + (tx + tw / 2.0 - (num + num3)) * num18;
				Dwm.THUMB_PROPS props = new Dwm.THUMB_PROPS
				{
					dwFlags = 15,
					rcDestination = new Native.RECT
					{
						L = (int)Math.Round(num19 - ox),
						T = (int)Math.Floor(num14 - oy),
						R = (int)Math.Round(num20 - ox),
						B = (int)Math.Ceiling(num15 - oy) + 1
					},
					rcSource = new Native.RECT
					{
						L = 0,
						T = (int)(num8 * srcH),
						R = (int)srcW,
						B = (int)Math.Ceiling(num9 * srcH)
					},
					opacity = opacity,
					fVisible = (num15 - num14 > 0.05)
				};
				Dwm.DwmUpdateThumbnailProperties(thumbs[i], ref props);
			}
		}

		private void Finish()
		{
			if (done)
			{
				return;
			}
			done = true;
			IntPtr[] array = thumbs;
			foreach (IntPtr intPtr in array)
			{
				if (intPtr != IntPtr.Zero)
				{
					Dwm.DwmUnregisterThumbnail(intPtr);
				}
			}
			owner.Running--;
			Close();
		}
	}
}
