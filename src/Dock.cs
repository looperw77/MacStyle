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
	internal class DockItem
	{
		public string Kind = "app";

		public string Name = "";

		public string Path = "";

		public string Exe = "";

		public string Aumid = "";

		public string Args = "";

		public bool Pinned;

		public List<IntPtr> Windows = new List<IntPtr>();

		public ImageSource Icon;

		public System.Windows.Controls.Image Img;

		public Ellipse Dot;

		public System.Windows.Shapes.Rectangle Hit;

		public readonly MatrixTransform ImgT = new MatrixTransform();

		public readonly MatrixTransform DotT = new MatrixTransform();

		public readonly MatrixTransform HitT = new MatrixTransform();

		public double Appear = 1.0;

		public double AppearTarget = 1.0;

		public double DotAlpha;

		public double BaseC;

		public double DispX;

		public double DispW;

		public double IconTop;

		public double LaunchAt = -1.0;

		public double BounceStop = -1.0;
	}
	internal class WinInfo
	{
		public IntPtr H;

		public string Exe = "";

		public string Aumid = "";
	}
	internal class Dock : Window
	{
		public static Dock Current;

		private static readonly HashSet<string> SkipExe = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "searchhost.exe", "startmenuexperiencehost.exe", "shellexperiencehost.exe", "textinputhost.exe", "lockapp.exe", "seelen-ui.exe", "slu-service.exe", "lively.exe", "livelywpf.exe" };

		private readonly Config cfg;

		private readonly Canvas root = new Canvas();

		private readonly System.Windows.Shapes.Path shadowPath = new System.Windows.Shapes.Path();

		private readonly System.Windows.Shapes.Path panelPath = new System.Windows.Shapes.Path();

		private readonly RectangleGeometry panelGeo = new RectangleGeometry();

		private readonly RectangleGeometry shadowGeo = new RectangleGeometry();

		private readonly System.Windows.Shapes.Rectangle sepLine = new System.Windows.Shapes.Rectangle();

		private readonly MatrixTransform sepT = new MatrixTransform();

		private readonly Border label = new Border();

		private readonly TextBlock labelText = new TextBlock();

		private readonly TranslateTransform labelT = new TranslateTransform();

		private readonly List<DockItem> apps = new List<DockItem>();

		private readonly DockItem start;

		private readonly DockItem trash;

		private readonly DispatcherTimer poll;

		private readonly DispatcherTimer refresh;

		private readonly DispatcherTimer slow;

		private readonly Stopwatch clock = Stopwatch.StartNew();

		private readonly int selfPid = Process.GetCurrentProcess().Id;

		private System.Windows.Media.Brush dotBrush = System.Windows.Media.Brushes.White;

		private IntPtr hwnd;

		private double dpi = 1.0;

		private Mon mon;

		private double winW;

		private double winH;

		private int winL;

		private int winT;

		private readonly EdgeDwell dwell = new EdgeDwell();

		private double lastT;

		private bool rendering;

		private bool menuOpen;

		private bool appBar;

		private bool fullscreen;

		private bool pressed;

		private double shown;

		private double shownTarget;

		private double hover;

		private double hoverTarget;

		private DateTime leaveAt = DateTime.MinValue;

		private double mx = -9999.0;

		private double my = -9999.0;

		private DockItem pressItem;

		private DockItem dragItem;

		private DockItem hoverItem;

		private System.Windows.Point pressPos;

		private double S = 46.0;

		private double M = 1.55;

		private double slot;

		private double padX;

		private double padY;

		private double sepW;

		private double bottomMargin = 6.0;

		private double panelTopNow;

		private double panelTopBase;

		private double dispLeft;

		private double dispRight;

		private double baseLeftLast;

		private double baseTotalLast;

		private long trashCount = -1L;

		public bool IsShown
		{
			get
			{
				if (cfg.DockEnabled)
				{
					return shown > 0.5;
				}
				return false;
			}
		}

		private static string PinFile => System.IO.Path.Combine(Brand.DataDir, "dock.txt");

		public event Action OpenSettingsRequested;

		public event Action LauncherRequested;

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint a, uint b, bool attach);

		[DllImport("user32.dll")]
		private static extern bool BringWindowToTop(IntPtr h);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		public Dock(Config cfg)
		{
			Current = this;
			this.cfg = cfg;
			base.Title = "MacStyle Dock";
			base.WindowStyle = WindowStyle.None;
			base.AllowsTransparency = true;
			base.Background = System.Windows.Media.Brushes.Transparent;
			base.Topmost = true;
			base.ShowInTaskbar = false;
			base.ShowActivated = false;
			base.ResizeMode = ResizeMode.NoResize;
			base.FlowDirection = System.Windows.FlowDirection.LeftToRight;
			base.WindowStartupLocation = WindowStartupLocation.Manual;
			base.Content = root;
			shadowPath.Data = shadowGeo;
			shadowPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 0));
			panelPath.Data = panelGeo;
			panelPath.StrokeThickness = 1.0;
			panelPath.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(48, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			root.Children.Add(shadowPath);
			root.Children.Add(panelPath);
			sepLine.Width = 1.0;
			sepLine.Height = 100.0;
			sepLine.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			sepLine.RenderTransform = sepT;
			root.Children.Add(sepLine);
			labelText.Foreground = System.Windows.Media.Brushes.White;
			labelText.FontSize = 12.5;
			labelText.FontFamily = Island.UiFont;
			label.Child = labelText;
			label.Padding = new Thickness(10.0, 4.0, 10.0, 5.0);
			label.CornerRadius = new CornerRadius(7.0);
			label.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(225, 32, 30, 38));
			label.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			label.BorderThickness = new Thickness(1.0);
			label.RenderTransform = labelT;
			label.Opacity = 0.0;
			label.IsHitTestVisible = false;
			System.Windows.Controls.Panel.SetZIndex(label, 1000);
			root.Children.Add(label);
			start = new DockItem
			{
				Kind = "start",
				Name = "كل البرامج",
				Pinned = true
			};
			trash = new DockItem
			{
				Kind = "trash",
				Name = "سلة المحذوفات",
				Pinned = true
			};
			LoadPinned();
			base.MouseLeftButtonDown += OnDown;
			base.MouseMove += OnMove;
			base.MouseLeftButtonUp += OnUp;
			base.MouseRightButtonUp += OnRight;
			poll = new DispatcherTimer(DispatcherPriority.Render)
			{
				Interval = TimeSpan.FromMilliseconds(25.0)
			};
			DispatcherTimer dispatcherTimer = poll;
			EventHandler value = delegate
			{
				if (UpdateTargets())
				{
					StartRender();
				}
			};
			dispatcherTimer.Tick += value;
			refresh = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(700.0)
			};
			refresh.Tick += delegate
			{
				RefreshWindows();
			};
			slow = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1.0)
			};
			slow.Tick += delegate
			{
				OnSlow();
			};
			base.SourceInitialized += delegate
			{
				hwnd = new WindowInteropHelper(this).Handle;
				Native.MakeOverlay(hwnd, clickThrough: false);
			};
			base.Loaded += delegate
			{
				ApplyConfig();
				RefreshWindows();
				poll.Start();
				refresh.Start();
				slow.Start();
				OnSlow();
			};
		}

		private IEnumerable<DockItem> AllItems()
		{
			yield return start;
			foreach (DockItem app in apps)
			{
				yield return app;
			}
			yield return trash;
		}

		public void ApplyConfig()
		{
			Metrics();
			System.Windows.Media.Color accentColor = cfg.AccentColor;
			System.Windows.Media.Color color = ColorUtil.Mix(System.Windows.Media.Color.FromRgb(24, 20, 32), accentColor, 0.12);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
			linearGradientBrush.StartPoint = new System.Windows.Point(0.0, 0.0);
			linearGradientBrush.EndPoint = new System.Windows.Point(0.0, 1.0);
			LinearGradientBrush linearGradientBrush2 = linearGradientBrush;
			linearGradientBrush2.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(150, (byte)Math.Min(255, color.R + 22), (byte)Math.Min(255, color.G + 22), (byte)Math.Min(255, color.B + 26)), 0.0));
			linearGradientBrush2.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(178, color.R, color.G, color.B), 1.0));
			panelPath.Fill = linearGradientBrush2;
			dotBrush = new SolidColorBrush(ColorUtil.Mix(accentColor, Colors.White, 0.45));
			start.Icon = LaunchpadIcon(accentColor);
			if (trash.Icon == null)
			{
				trash.Icon = ShellApi.Icon("shell:RecycleBinFolder", 256, useCache: false);
			}
			foreach (DockItem item in AllItems())
			{
				BuildVisual(item);
			}
			if (!cfg.DockEnabled)
			{
				StopRender();
				SetAppBar(on: false);
				Taskbar.Restore();
				base.Visibility = Visibility.Hidden;
				return;
			}
			Taskbar.Hide();
			base.Visibility = Visibility.Visible;
			Resize();
			SetAppBar(!cfg.DockAutoHide);
			if (!cfg.DockAutoHide)
			{
				shownTarget = 1.0;
			}
			StartRender();
		}

		private bool Metrics()
		{
			mon = Screens.Primary();
			dpi = mon.Dpi;
			double old = S * M;
			M = Math.Max(1.0, cfg.DockMagnify);
			S = Fit.DockIcon(cfg.DockIconSize, apps.Count + 2, M, mon.W);
			slot = S + Math.Round(S * 0.2);
			padX = Math.Round(S * 0.2);
			padY = Math.Round(S * 0.16);
			sepW = Math.Round(S * 0.42);
			return S * M != old;
		}

		private void BuildVisual(DockItem it)
		{
			if (it.Img == null)
			{
				it.Hit = new System.Windows.Shapes.Rectangle
				{
					Width = 100.0,
					Height = 100.0,
					Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0)),
					RenderTransform = it.HitT
				};
				it.Img = new System.Windows.Controls.Image
				{
					RenderTransform = it.ImgT,
					IsHitTestVisible = false,
					Stretch = Stretch.Uniform
				};
				RenderOptions.SetBitmapScalingMode(it.Img, BitmapScalingMode.HighQuality);
				it.Dot = new Ellipse
				{
					Width = 5.0,
					Height = 5.0,
					RenderTransform = it.DotT,
					IsHitTestVisible = false,
					Opacity = 0.0
				};
				int num = root.Children.IndexOf(label);
				root.Children.Insert(num, it.Hit);
				root.Children.Insert(num + 1, it.Img);
				root.Children.Insert(num + 2, it.Dot);
			}
			System.Windows.Controls.Image img = it.Img;
			double width = (it.Img.Height = S * M);
			img.Width = width;
			it.Img.Source = it.Icon ?? Letter(it.Name);
			it.Dot.Fill = dotBrush;
		}

		private void RemoveVisual(DockItem it)
		{
			if (it.Img != null)
			{
				root.Children.Remove(it.Hit);
				root.Children.Remove(it.Img);
				root.Children.Remove(it.Dot);
				it.Img = null;
			}
		}

		private void Resize()
		{
			if (Metrics())
			{
				foreach (DockItem item in AllItems())
				{
					BuildVisual(item);
				}
			}
			int num = apps.Count + 2;
			double num2 = (double)num * slot + sepW;
			double num3 = (M - 1.0) * slot * 2.6 * 1.15;
			double num4 = Math.Min(mon.W, Math.Max(480.0, Math.Ceiling(num2 + 2.0 * num3 + 2.0 * padX + 160.0)));
			double num5 = Math.Ceiling(S * M + S * 0.62 + 46.0 + padY + bottomMargin + 6.0);
			int num6 = (int)Math.Ceiling(num4 * dpi);
			int num7 = (int)Math.Ceiling(num5 * dpi);
			winW = num6 / dpi;
			winH = num7 / dpi;
			winL = mon.CenterX - num6 / 2;
			winT = mon.Px.B - num7;
			Screens.Place(hwnd, winL, winT, num6, num7);
		}

		private static ImageSource LaunchpadIcon(System.Windows.Media.Color a)
		{
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen())
			{
				LinearGradientBrush brush = new LinearGradientBrush(ColorUtil.Mix(a, Colors.White, 0.3), ColorUtil.Mix(a, Colors.Black, 0.35), 90.0);
				drawingContext.DrawRoundedRectangle(brush, new System.Windows.Media.Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(70, byte.MaxValue, byte.MaxValue, byte.MaxValue)), 2.0), new Rect(10.0, 10.0, 108.0, 108.0), 26.0, 26.0);
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						drawingContext.DrawRoundedRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(235, byte.MaxValue, byte.MaxValue, byte.MaxValue)), null, new Rect(33 + j * 23, 33 + i * 23, 16.0, 16.0), 5.0, 5.0);
					}
				}
			}
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(128, 128, 96.0, 96.0, PixelFormats.Pbgra32);
			renderTargetBitmap.Render(drawingVisual);
			renderTargetBitmap.Freeze();
			return renderTargetBitmap;
		}

		private static ImageSource Letter(string name)
		{
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen())
			{
				drawingContext.DrawRoundedRectangle(new LinearGradientBrush(System.Windows.Media.Color.FromRgb(120, 118, 135), System.Windows.Media.Color.FromRgb(70, 68, 82), 90.0), null, new Rect(10.0, 10.0, 108.0, 108.0), 26.0, 26.0);
				string textToFormat = (string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant());
				FormattedText formattedText = new FormattedText(textToFormat, CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("Segoe UI Semibold"), 60.0, System.Windows.Media.Brushes.White);
				drawingContext.DrawText(formattedText, new System.Windows.Point(64.0 - formattedText.Width / 2.0, 64.0 - formattedText.Height / 2.0));
			}
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(128, 128, 96.0, 96.0, PixelFormats.Pbgra32);
			renderTargetBitmap.Render(drawingVisual);
			renderTargetBitmap.Freeze();
			return renderTargetBitmap;
		}

		private void LoadIcon(DockItem it)
		{
			ImageSource imageSource = null;
			bool flag = it.Path.IndexOf("\\WindowsApps\\", StringComparison.OrdinalIgnoreCase) >= 0;
			if (it.Path != "" && !flag && (File.Exists(it.Path) || Directory.Exists(it.Path)))
			{
				imageSource = ShellApi.Icon(it.Path, 256, useCache: true);
			}
			if (imageSource == null && it.Aumid != "")
			{
				imageSource = ShellApi.PackagedLogo(it.Aumid);
			}
			if (imageSource == null && it.Aumid != "")
			{
				imageSource = ShellApi.Icon("shell:AppsFolder\\" + it.Aumid, 48, useCache: true);
			}
			if (imageSource == null && it.Exe != "" && File.Exists(it.Exe))
			{
				imageSource = ShellApi.Icon(it.Exe, 256, useCache: true);
			}
			it.Icon = imageSource;
		}

		private void LoadPinned()
		{
			List<DockItem> list = new List<DockItem>();
			try
			{
				if (File.Exists(PinFile))
				{
					string[] array = File.ReadAllLines(PinFile, Encoding.UTF8);
					foreach (string text in array)
					{
						string[] array2 = text.Split('\t');
						if (array2.Length >= 5)
						{
							list.Add(new DockItem
							{
								Name = array2[0],
								Path = array2[1],
								Aumid = array2[2],
								Args = array2[3],
								Exe = array2[4],
								Pinned = true
							});
						}
					}
				}
				else
				{
					list = ImportPins();
				}
			}
			catch (Exception ex)
			{
				Sys.Log("dock pins: " + ex.Message);
			}
			foreach (DockItem item in list)
			{
				if (item.Exe == "")
				{
					ResolveExe(item);
				}
				LoadIcon(item);
				apps.Add(item);
			}
			if (!File.Exists(PinFile))
			{
				SavePinned();
			}
		}

		private void SavePinned()
		{
			try
			{
				Directory.CreateDirectory(Brand.DataDir);
				StringBuilder stringBuilder = new StringBuilder();
				foreach (DockItem app in apps)
				{
					if (app.Pinned)
					{
						stringBuilder.Append(Clean(app.Name)).Append('\t').Append(Clean(app.Path))
							.Append('\t')
							.Append(Clean(app.Aumid))
							.Append('\t')
							.Append(Clean(app.Args))
							.Append('\t')
							.Append(Clean(app.Exe))
							.Append("\r\n");
					}
				}
				File.WriteAllText(PinFile, stringBuilder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			}
			catch (Exception ex)
			{
				Sys.Log("dock save: " + ex.Message);
			}
		}

		private static string Clean(string s)
		{
			return (s ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
		}

		private static void ResolveExe(DockItem it)
		{
			if (it.Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				it.Exe = it.Path;
			}
			else if (it.Path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) && File.Exists(it.Path))
			{
				string text = ShellApi.LnkTarget(it.Path);
				if (!string.IsNullOrEmpty(text) && text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
				{
					it.Exe = text;
				}
			}
		}

		private static List<DockItem> ImportPins()
		{
			List<DockItem> list = new List<DockItem>();
			string text = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
			if (FilesApp.Installed)
			{
				list.Add(new DockItem
				{
					Name = "Files",
					Aumid = "Files_1y0xx7n9077q4!App",
					Pinned = true
				});
			}
			else
			{
				list.Add(new DockItem
				{
					Name = "File Explorer",
					Path = text,
					Exe = text,
					Pinned = true
				});
			}
			try
			{
				string path = System.IO.Path.Combine(Seelen.Dir, "data\\seelen-weg\\state.yml");
				if (File.Exists(path))
				{
					list.AddRange(ParseSeelen(File.ReadAllLines(path, Encoding.UTF8)));
				}
			}
			catch
			{
			}
			if (list.Count <= 1)
			{
				try
				{
					string path2 = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");
					if (Directory.Exists(path2))
					{
						string[] files = Directory.GetFiles(path2, "*.lnk");
						foreach (string path3 in files)
						{
							list.Add(new DockItem
							{
								Name = System.IO.Path.GetFileNameWithoutExtension(path3),
								Path = path3,
								Pinned = true
							});
						}
					}
				}
				catch
				{
				}
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<DockItem> list2 = new List<DockItem>();
			foreach (DockItem item2 in list)
			{
				if (item2.Exe == "")
				{
					ResolveExe(item2);
				}
				string item = ((item2.Aumid != "") ? ("a:" + item2.Aumid) : ((item2.Exe != "") ? ("e:" + item2.Exe) : ("p:" + item2.Path)));
				if ((!(item2.Exe != "") || !hashSet.Contains("e:" + item2.Exe)) && !hashSet.Contains(item))
				{
					hashSet.Add(item);
					if (item2.Exe != "")
					{
						hashSet.Add("e:" + item2.Exe);
					}
					list2.Add(item2);
				}
			}
			return list2;
		}

		private static List<DockItem> ParseSeelen(string[] lines)
		{
			List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
			bool flag = false;
			Dictionary<string, string> dictionary = null;
			foreach (string text in lines)
			{
				if (text.Length > 0 && text[0] != ' ' && text[0] != '-')
				{
					flag = text.TrimEnd() == "center:";
					dictionary = null;
				}
				else
				{
					if (!flag)
					{
						continue;
					}
					string text2 = text;
					if (text2.StartsWith("- "))
					{
						dictionary = new Dictionary<string, string>();
						list.Add(dictionary);
						text2 = "  " + text2.Substring(2);
					}
					if (dictionary == null)
					{
						continue;
					}
					int num = text2.Length - text2.TrimStart().Length;
					string text3 = text2.Trim();
					int num2 = text3.IndexOf(':');
					if (num2 > 0)
					{
						string text4 = text3.Substring(0, num2);
						string text5 = text3.Substring(num2 + 1).Trim();
						if (text5.Length >= 2 && ((text5[0] == '\'' && text5[text5.Length - 1] == '\'') || (text5[0] == '"' && text5[text5.Length - 1] == '"')))
						{
							text5 = text5.Substring(1, text5.Length - 2).Replace("''", "'");
						}
						if (text5 == "null")
						{
							text5 = "";
						}
						dictionary[((num >= 4) ? "relaunch." : "") + text4] = text5;
					}
				}
			}
			List<DockItem> list2 = new List<DockItem>();
			foreach (Dictionary<string, string> item in list)
			{
				if (item.TryGetValue("type", out var value) && !(value != "AppOrFile") && item.TryGetValue("pinned", out var value2) && !(value2 != "true"))
				{
					DockItem dockItem = new DockItem();
					dockItem.Pinned = true;
					DockItem dockItem2 = dockItem;
					if (item.TryGetValue("displayName", out var value3))
					{
						dockItem2.Name = value3;
					}
					if (item.TryGetValue("path", out value3))
					{
						dockItem2.Path = value3;
					}
					if (item.TryGetValue("umid", out value3))
					{
						dockItem2.Aumid = value3;
					}
					string value4;
					if (dockItem2.Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
					{
						dockItem2.Exe = dockItem2.Path;
					}
					else if (item.TryGetValue("relaunch.command", out value4) && value4.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
					{
						dockItem2.Exe = value4;
					}
					if (!(dockItem2.Name == "") || !(dockItem2.Path == "") || !(dockItem2.Aumid == ""))
					{
						list2.Add(dockItem2);
					}
				}
			}
			return list2;
		}

		private static bool Eq(string a, string b)
		{
			if (!string.IsNullOrEmpty(a))
			{
				return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		internal List<WinInfo> EnumerateAppWindows()
		{
			List<WinInfo> list = new List<WinInfo>();
			Dictionary<uint, string> exeCache = new Dictionary<uint, string>();
			Dictionary<uint, string> aumidCache = new Dictionary<uint, string>();
			Native.EnumWindows(delegate(IntPtr h, IntPtr l)
			{
				try
				{
					if (!Native.IsWindowVisible(h))
					{
						return true;
					}
					if (Native.GetWindow(h, 4u) != IntPtr.Zero)
					{
						return true;
					}
					int windowLong = Native.GetWindowLong(h, -20);
					if ((windowLong & 0x40000) == 0 && ((windowLong & 0x80) != 0 || (windowLong & 0x8000000) != 0))
					{
						return true;
					}
					if (Native.GetWindowTextLength(h) == 0)
					{
						return true;
					}
					if (ShellApi.Cloaked(h))
					{
						return true;
					}
					switch (Native.ClassOf(h))
					{
					case "Progman":
					case "WorkerW":
					case "Shell_TrayWnd":
					case "Shell_SecondaryTrayWnd":
					case "Windows.UI.Core.CoreWindow":
						return true;
					default:
					{
						Native.GetWindowPid(h, out var pid);
						if (pid == selfPid)
						{
							return true;
						}
						if (!exeCache.TryGetValue(pid, out var value))
						{
							value = Native.ProcessPath((int)pid) ?? "";
							exeCache[pid] = value;
						}
						string fileName = System.IO.Path.GetFileName(value);
						if (SkipExe.Contains(fileName))
						{
							return true;
						}
						WinInfo winInfo = new WinInfo
						{
							H = h,
							Exe = value
						};
						string value2 = ShellApi.WindowAumid(h);
						if (string.IsNullOrEmpty(value2) && !aumidCache.TryGetValue(pid, out value2))
						{
							value2 = ShellApi.ProcessAumid((int)pid) ?? "";
							aumidCache[pid] = value2;
						}
						winInfo.Aumid = value2 ?? "";
						if (string.Equals(fileName, "ApplicationFrameHost.exe", StringComparison.OrdinalIgnoreCase))
						{
							if (winInfo.Aumid == "")
							{
								return true;
							}
							winInfo.Exe = "";
						}
						list.Add(winInfo);
						break;
					}
					}
				}
				catch
				{
				}
				return true;
			}, IntPtr.Zero);
			return list;
		}

		private static string AppName(WinInfo w)
		{
			string text = null;
			if (w.Aumid != "")
			{
				text = ShellApi.DisplayName("shell:AppsFolder\\" + w.Aumid);
			}
			if (string.IsNullOrEmpty(text) && w.Exe != "" && File.Exists(w.Exe))
			{
				try
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(w.Exe);
					text = ((!string.IsNullOrWhiteSpace(versionInfo.FileDescription)) ? versionInfo.FileDescription.Trim() : versionInfo.ProductName);
				}
				catch
				{
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = System.IO.Path.GetFileNameWithoutExtension(w.Exe);
			}
			return text ?? "";
		}

		private void RefreshWindows()
		{
			if (!cfg.DockEnabled)
			{
				return;
			}
			List<WinInfo> list;
			try
			{
				list = EnumerateAppWindows();
			}
			catch
			{
				return;
			}
			foreach (DockItem app in apps)
			{
				app.Windows.Clear();
			}
			bool flag = false;
			foreach (WinInfo item in list)
			{
				DockItem dockItem = null;
				if (item.Aumid != "")
				{
					foreach (DockItem app2 in apps)
					{
						if (Eq(app2.Aumid, item.Aumid))
						{
							dockItem = app2;
							break;
						}
					}
				}
				if (dockItem == null && item.Exe != "")
				{
					foreach (DockItem app3 in apps)
					{
						if (Eq(app3.Exe, item.Exe))
						{
							dockItem = app3;
							break;
						}
					}
				}
				if (dockItem == null)
				{
					DockItem dockItem2 = new DockItem();
					dockItem2.Exe = item.Exe;
					dockItem2.Aumid = item.Aumid;
					dockItem2.Pinned = false;
					dockItem2.Appear = 0.0;
					dockItem2.AppearTarget = 1.0;
					dockItem = dockItem2;
					dockItem.Name = AppName(item);
					LoadIcon(dockItem);
					BuildVisual(dockItem);
					apps.Add(dockItem);
					flag = true;
				}
				dockItem.Windows.Add(item.H);
			}
			foreach (DockItem app4 in apps)
			{
				if (!app4.Pinned)
				{
					app4.AppearTarget = ((app4.Windows.Count > 0) ? 1 : 0);
				}
			}
			if (flag)
			{
				Resize();
			}
			StartRender();
		}

		private void CleanupRemoved()
		{
			bool flag = false;
			for (int num = apps.Count - 1; num >= 0; num--)
			{
				DockItem dockItem = apps[num];
				if (!dockItem.Pinned && dockItem.AppearTarget == 0.0 && dockItem.Appear < 0.01 && dockItem != dragItem)
				{
					RemoveVisual(dockItem);
					apps.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				Resize();
			}
		}

		private void OnSlow()
		{
			if (!cfg.DockEnabled)
			{
				return;
			}
			Taskbar.Enforce();
			bool same = Screens.Primary().Same(mon);
			Resize();
			if (!same)
			{
				StartRender();
			}
			bool flag = ForegroundFullscreen();
			if (flag != fullscreen)
			{
				fullscreen = flag;
				StartRender();
			}
			if (fullscreen && shown < 0.01)
			{
				if (base.Visibility == Visibility.Visible)
				{
					base.Visibility = Visibility.Hidden;
				}
			}
			else
			{
				if (base.Visibility != Visibility.Visible)
				{
					base.Visibility = Visibility.Visible;
				}
				Native.TopMost(hwnd);
			}
			long num = ShellApi.RecycleCount();
			if (num > 0 != trashCount > 0 || trashCount < 0)
			{
				trashCount = num;
				ImageSource imageSource = ShellApi.Icon("shell:RecycleBinFolder", 256, useCache: false);
				if (imageSource != null)
				{
					trash.Icon = imageSource;
					if (trash.Img != null)
					{
						trash.Img.Source = imageSource;
					}
				}
			}
			if (appBar)
			{
				SetAppBar(on: true);
			}
		}

		private bool ForegroundFullscreen()
		{
			IntPtr foregroundWindow = Native.GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero || foregroundWindow == hwnd)
			{
				return false;
			}
			switch (Native.ClassOf(foregroundWindow))
			{
			case "Progman":
			case "WorkerW":
			case "Shell_TrayWnd":
				return false;
			default:
			{
				if ((Native.GetWindowLong(foregroundWindow, -16) & 0xC00000) == 12582912)
				{
					return false;
				}
				if (Native.IsZoomed(foregroundWindow))
				{
					return false;
				}
				if (!Native.GetWindowRect(foregroundWindow, out var r))
				{
					return false;
				}
				Native.MONITORINFO mi = default(Native.MONITORINFO);
				mi.cbSize = Marshal.SizeOf(mi);
				if (!Native.GetMonitorInfo(Native.MonitorFromWindow(foregroundWindow, 2u), ref mi))
				{
					return false;
				}
				if (mi.rcMonitor.Equals(mon.Px) && r.L <= mi.rcMonitor.L && r.T <= mi.rcMonitor.T && r.R >= mi.rcMonitor.R)
				{
					return r.B >= mi.rcMonitor.B;
				}
				return false;
			}
			}
		}

		private void SetAppBar(bool on)
		{
			if (hwnd == IntPtr.Zero)
			{
				return;
			}
			Taskbar.APPBARDATA d = new Taskbar.APPBARDATA
			{
				cbSize = Marshal.SizeOf(typeof(Taskbar.APPBARDATA)),
				hWnd = hwnd
			};
			if (!on)
			{
				if (appBar)
				{
					Taskbar.SHAppBarMessage(1u, ref d);
					appBar = false;
				}
				return;
			}
			if (!appBar)
			{
				d.uCallbackMessage = 32845u;
				Taskbar.SHAppBarMessage(0u, ref d);
				appBar = true;
			}
			int num = mon.Px.B;
			int num2 = (int)Math.Ceiling((S + 2.0 * padY + bottomMargin + 4.0) * dpi);
			d.uEdge = 3u;
			d.rc = new Native.RECT
			{
				L = mon.Px.L,
				T = num - num2,
				R = mon.Px.R,
				B = num
			};
			Taskbar.SHAppBarMessage(2u, ref d);
			d.rc.T = d.rc.B - num2;
			Taskbar.SHAppBarMessage(3u, ref d);
		}

		private bool UpdateTargets()
		{
			if (!cfg.DockEnabled || hwnd == IntPtr.Zero)
			{
				return false;
			}
			if (!Native.GetCursorPos(out var p))
			{
				return false;
			}
			double num = (p.X - winL) / dpi;
			double num2 = (p.Y - winT) / dpi;
			bool flag = Math.Abs(num - mx) > 0.01 || Math.Abs(num2 - my) > 0.01;
			mx = num;
			my = num2;
			double height = winH;
			bool inside = mon.Contains(p);
			double num3 = S + 2.0 * padY;
			double num4 = baseLeftLast - padX;
			double num5 = baseLeftLast + baseTotalLast + padX;
			double num6 = Math.Min(num4, dispLeft);
			double num7 = Math.Max(num5, dispRight);
			bool flag2 = inside && mx >= num6 - 2.0 && mx <= num7 + 2.0 && my >= height - bottomMargin - num3 - S * (M - 1.0) * hover - 6.0 && my <= height + 2.0;
			bool flag3 = inside && p.Y >= mon.Px.B - 1 && mx >= num4 - 40.0 && mx <= num5 + 40.0;
			flag3 = dwell.Update(clock.ElapsedMilliseconds, flag3, Native.MouseButtonDown()) || (flag3 && shownTarget > 0.0);
			bool flag4;
			if (fullscreen)
			{
				flag4 = false;
			}
			else if (!cfg.DockAutoHide)
			{
				flag4 = true;
			}
			else if (menuOpen || pressed || dragItem != null)
			{
				flag4 = true;
			}
			else if (flag3 || (shownTarget > 0.0 && flag2))
			{
				flag4 = true;
			}
			else if (shownTarget > 0.0)
			{
				if (leaveAt == DateTime.MinValue)
				{
					leaveAt = DateTime.Now;
				}
				flag4 = (DateTime.Now - leaveAt).TotalMilliseconds < 420.0;
			}
			else
			{
				flag4 = false;
			}
			if (flag2 || flag3 || menuOpen)
			{
				leaveAt = DateTime.MinValue;
			}
			shownTarget = (flag4 ? 1 : 0);
			hoverTarget = ((dragItem != null || (shown > 0.6 && flag2 && !menuOpen)) ? 1 : 0);
			if (!(Math.Abs(shown - shownTarget) > 0.001) && !(Math.Abs(hover - hoverTarget) > 0.001) && (!(hover > 0.001) || !flag) && !(hoverTarget > 0.0))
			{
				return Animating();
			}
			return true;
		}

		private bool Animating()
		{
			if (dragItem != null || Math.Abs(label.Opacity - (double)((hoverItem != null) ? 1 : 0)) > 0.01)
			{
				return true;
			}
			foreach (DockItem item in AllItems())
			{
				if (Math.Abs(item.Appear - item.AppearTarget) > 0.001)
				{
					return true;
				}
				if (Math.Abs(item.DotAlpha - (double)((item.Windows.Count > 0) ? 1 : 0)) > 0.001)
				{
					return true;
				}
				if (item.LaunchAt >= 0.0)
				{
					return true;
				}
			}
			return false;
		}

		private void StartRender()
		{
			if (!rendering && cfg.DockEnabled)
			{
				rendering = true;
				lastT = clock.Elapsed.TotalSeconds;
				CompositionTarget.Rendering += OnRender;
			}
		}

		private void StopRender()
		{
			if (rendering)
			{
				rendering = false;
				CompositionTarget.Rendering -= OnRender;
			}
		}

		private static double Approach(double v, double target, double dt, double tau)
		{
			v += (target - v) * (1.0 - Math.Exp((0.0 - dt) / tau));
			if (Math.Abs(target - v) < 0.0015)
			{
				v = target;
			}
			return v;
		}

		private void OnRender(object s, EventArgs e)
		{
			double totalSeconds = clock.Elapsed.TotalSeconds;
			double dt = Math.Min(0.05, Math.Max(0.001, totalSeconds - lastT));
			lastT = totalSeconds;
			bool flag = UpdateTargets();
			shown = Approach(shown, shownTarget, dt, (shownTarget > shown) ? 0.05 : 0.07);
			hover = Approach(hover, hoverTarget, dt, (hoverTarget > hover) ? 0.045 : 0.07);
			foreach (DockItem item in AllItems())
			{
				item.Appear = Approach(item.Appear, item.AppearTarget, dt, 0.08);
				item.DotAlpha = Approach(item.DotAlpha, (item.Windows.Count > 0) ? 1 : 0, dt, 0.1);
			}
			Layout(totalSeconds, dt);
			CleanupRemoved();
			if (!flag && !Animating())
			{
				StopRender();
			}
		}

		private void Layout(double now, double dt)
		{
			double width = winW;
			double height = winH;
			int num = apps.Count + 1;
			int num2 = num + 2;
			DockItem[] array = new DockItem[num2];
			double[] array2 = new double[num2];
			double[] array3 = new double[num2];
			array[0] = start;
			for (int i = 0; i < apps.Count; i++)
			{
				array[i + 1] = apps[i];
			}
			array[num] = null;
			array[num + 1] = trash;
			double num3 = 0.0;
			for (int j = 0; j < num2; j++)
			{
				array2[j] = ((array[j] == null) ? sepW : (slot * array[j].Appear));
				num3 += array2[j];
			}
			double num4 = (baseLeftLast = (width - num3) / 2.0);
			baseTotalLast = num3;
			double num5 = slot * 2.6;
			double num6 = Math.Max(num4, Math.Min(num4 + num3, mx));
			double num7 = 0.0;
			for (int k = 0; k < num2; k++)
			{
				double num8 = num4 + num7 + array2[k] / 2.0;
				if (array[k] != null)
				{
					array[k].BaseC = num8;
				}
				double num9 = Math.Abs(num6 - num8);
				double num10 = ((num9 >= num5) ? 0.0 : ((1.0 + Math.Cos(Math.PI * num9 / num5)) / 2.0));
				array3[k] = ((array[k] == null) ? 1.0 : (1.0 + (M - 1.0) * hover * num10));
				num7 += array2[k];
			}
			double num11 = num6 - num4;
			double num12 = 0.0;
			double num13 = 0.0;
			double num14 = 0.0;
			bool flag = false;
			for (int l = 0; l < num2; l++)
			{
				double num15 = array2[l] * array3[l];
				if (!flag && num11 <= num12 + array2[l])
				{
					double num16 = ((array2[l] > 0.0) ? ((num11 - num12) / array2[l]) : 0.0);
					num14 = num13 + num16 * num15;
					flag = true;
				}
				num12 += array2[l];
				num13 += num15;
			}
			double num17 = num13;
			if (!flag)
			{
				num14 = num13;
			}
			double val = num4 + num11 - num14;
			val = Math.Max(4.0 + padX, Math.Min(width - 4.0 - padX - num17, val));
			dispLeft = val - padX;
			dispRight = val + num17 + padX;
			double num18 = S + 2.0 * padY;
			double num19 = (1.0 - shown) * (num18 + bottomMargin + 14.0);
			double num20 = height - bottomMargin + num19;
			double num21 = (panelTopNow = num20 - num18);
			panelTopBase = height - bottomMargin - num18;
			double num22 = Math.Min(20.0, S * 0.38);
			panelGeo.Rect = new Rect(dispLeft, num21, dispRight - dispLeft, num18);
			RectangleGeometry rectangleGeometry = panelGeo;
			double radiusX = (panelGeo.RadiusY = num22);
			rectangleGeometry.RadiusX = radiusX;
			shadowGeo.Rect = new Rect(dispLeft + 1.0, num21 + 3.0, dispRight - dispLeft - 2.0, num18);
			RectangleGeometry rectangleGeometry2 = shadowGeo;
			double radiusX2 = (shadowGeo.RadiusY = num22);
			rectangleGeometry2.RadiusX = radiusX2;
			double num25 = Math.Min(1.0, shown * 1.3);
			panelPath.Opacity = num25;
			shadowPath.Opacity = num25;
			double num26 = val;
			DockItem dockItem = null;
			for (int m = 0; m < num2; m++)
			{
				double num27 = array2[m] * array3[m];
				DockItem dockItem2 = array[m];
				if (dockItem2 == null)
				{
					sepT.Matrix = new Matrix(1.0, 0.0, 0.0, S * 0.64 / 100.0, Math.Round(num26 + num27 / 2.0), num21 + padY + S * 0.18);
					sepLine.Opacity = num25;
					num26 += num27;
					continue;
				}
				double num28 = S * array3[m] * Math.Max(0.001, dockItem2.Appear);
				double num29 = Bounce(dockItem2, now);
				double offsetX = num26 + (num27 - num28) / 2.0;
				double num30 = num20 - padY - num28 - num29;
				if (dockItem2 == dragItem)
				{
					offsetX = mx - num28 / 2.0;
					num30 = Math.Min(num20 - padY - num28, my - num28 / 2.0);
				}
				double num31 = num28 / (S * M);
				dockItem2.ImgT.Matrix = new Matrix(num31, 0.0, 0.0, num31, offsetX, num30);
				dockItem2.Img.Opacity = Math.Min(1.0, dockItem2.Appear * 1.3) * num25 * ((dockItem2 == dragItem) ? 0.85 : 1.0);
				dockItem2.DotT.Matrix = new Matrix(1.0, 0.0, 0.0, 1.0, num26 + num27 / 2.0 - 2.5, num20 - padY * 0.5 - 2.5);
				dockItem2.Dot.Opacity = dockItem2.DotAlpha * num25 * dockItem2.Appear;
				double num32 = Math.Min(num30, num21);
				double num33 = Math.Max(0.01, num20 - num32);
				dockItem2.HitT.Matrix = new Matrix(Math.Max(0.01, num27) / 100.0, 0.0, 0.0, num33 / 100.0, num26, num32);
				dockItem2.DispX = num26;
				dockItem2.DispW = num27;
				dockItem2.IconTop = num30;
				if (hover > 0.5 && mx >= num26 && mx < num26 + num27 && my >= num32 - 4.0 && my <= height)
				{
					dockItem = dockItem2;
				}
				num26 += num27;
			}
			hoverItem = ((dockItem != null && dragItem == null && !menuOpen && shown > 0.9) ? dockItem : null);
			if (hoverItem != null)
			{
				if (labelText.Text != hoverItem.Name)
				{
					labelText.Text = hoverItem.Name;
					label.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
				}
				double width2 = label.DesiredSize.Width;
				double height2 = label.DesiredSize.Height;
				labelT.X = Math.Round(Math.Max(2.0, Math.Min(width - width2 - 2.0, hoverItem.DispX + hoverItem.DispW / 2.0 - width2 / 2.0)));
				labelT.Y = Math.Round(Math.Max(0.0, hoverItem.IconTop - height2 - 10.0));
			}
			label.Opacity = Approach(label.Opacity, (hoverItem != null) ? 1 : 0, dt, 0.05);
		}

		private double Bounce(DockItem it, double now)
		{
			if (it.LaunchAt < 0.0)
			{
				return 0.0;
			}
			double num = now - it.LaunchAt;
			if (it.BounceStop < 0.0 && (it.Windows.Count > 0 || num > 7.0))
			{
				it.BounceStop = Math.Max(1.0, Math.Ceiling(num / 0.6)) * 0.6;
			}
			if (it.BounceStop >= 0.0 && num >= it.BounceStop)
			{
				it.LaunchAt = -1.0;
				it.BounceStop = -1.0;
				return 0.0;
			}
			double num2 = num % 0.6 / 0.6;
			return S * 0.55 * 4.0 * num2 * (1.0 - num2);
		}

		private DockItem ItemAt(System.Windows.Point p)
		{
			foreach (DockItem item in AllItems())
			{
				if (item.Img != null && !(item.Appear < 0.3) && p.X >= item.DispX && p.X < item.DispX + item.DispW && p.Y >= Math.Min(item.IconTop, panelTopNow) - 2.0)
				{
					return item;
				}
			}
			return null;
		}

		private void OnDown(object s, MouseButtonEventArgs e)
		{
			System.Windows.Point position = e.GetPosition(root);
			pressItem = ItemAt(position);
			pressPos = position;
			pressed = true;
			CaptureMouse();
			e.Handled = true;
		}

		private void OnMove(object s, System.Windows.Input.MouseEventArgs e)
		{
			if (!pressed || pressItem == null || pressItem.Kind != "app")
			{
				return;
			}
			System.Windows.Point position = e.GetPosition(root);
			if (dragItem == null && (Math.Abs(position.X - pressPos.X) > 7.0 || Math.Abs(position.Y - pressPos.Y) > 7.0))
			{
				dragItem = pressItem;
				StartRender();
			}
			if (dragItem == null)
			{
				return;
			}
			int num = apps.IndexOf(dragItem);
			if (num >= 0)
			{
				if (num > 0 && position.X < apps[num - 1].DispX + apps[num - 1].DispW / 2.0)
				{
					apps.RemoveAt(num);
					apps.Insert(num - 1, dragItem);
				}
				else if (num < apps.Count - 1 && position.X > apps[num + 1].DispX + apps[num + 1].DispW / 2.0)
				{
					apps.RemoveAt(num);
					apps.Insert(num + 1, dragItem);
				}
			}
		}

		private void OnUp(object s, MouseButtonEventArgs e)
		{
			System.Windows.Point position = e.GetPosition(root);
			ReleaseMouseCapture();
			pressed = false;
			DockItem dockItem = dragItem;
			dragItem = null;
			if (dockItem != null)
			{
				if (position.Y < panelTopBase - S * 1.3)
				{
					dockItem.Pinned = false;
					if (dockItem.Windows.Count == 0)
					{
						dockItem.AppearTarget = 0.0;
					}
				}
				else
				{
					dockItem.Pinned = true;
				}
				SavePinned();
				StartRender();
				pressItem = null;
			}
			else
			{
				DockItem dockItem2 = ItemAt(position);
				if (dockItem2 != null && dockItem2 == pressItem)
				{
					Click(dockItem2);
				}
				pressItem = null;
			}
		}

		private void OnRight(object s, MouseButtonEventArgs e)
		{
			e.Handled = true;
			DockItem it = ItemAt(e.GetPosition(root));
			System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu
			{
				FlowDirection = System.Windows.FlowDirection.RightToLeft,
				FontFamily = Island.UiFont
			};
			Action<string, Action> action = delegate(string header, Action act)
			{
				System.Windows.Controls.MenuItem menuItem = new System.Windows.Controls.MenuItem
				{
					Header = header
				};
				menuItem.Click += delegate
				{
					try
					{
						act();
					}
					catch (Exception ex)
					{
						Sys.Log("dock menu: " + ex.Message);
					}
				};
				menu.Items.Add(menuItem);
			};
			if (it == null || it.Kind == "start")
			{
				if (it != null)
				{
					action("قائمة ابدأ بتاعة ويندوز", OpenStart);
				}
				action("إعدادات MacStyle", delegate
				{
					if (this.OpenSettingsRequested != null)
					{
						this.OpenSettingsRequested();
					}
				});
			}
			else if (it.Kind == "trash")
			{
				action("فتح سلة المحذوفات", delegate
				{
					Process.Start("explorer.exe", "shell:RecycleBinFolder");
				});
				if (ShellApi.RecycleCount() > 0)
				{
					action("تفريغ السلة", delegate
					{
						ShellApi.SHEmptyRecycleBin(IntPtr.Zero, null, 0u);
						trashCount = -1L;
						OnSlow();
					});
				}
			}
			else
			{
				menu.Items.Add(new System.Windows.Controls.MenuItem
				{
					Header = it.Name,
					IsEnabled = false,
					FontWeight = FontWeights.SemiBold
				});
				menu.Items.Add(new Separator());
				if (it.Windows.Count > 1)
				{
					foreach (IntPtr item in it.Windows.GetRange(0, Math.Min(8, it.Windows.Count)))
					{
						IntPtr hh = item;
						string text = ShellApi.Title(hh);
						if (text.Length > 48)
						{
							text = text.Substring(0, 45) + "...";
						}
						action(text, delegate
						{
							Activate(hh);
						});
					}
					menu.Items.Add(new Separator());
				}
				action((it.Windows.Count > 0) ? "فتح نافذة جديدة" : "فتح", delegate
				{
					Launch(it);
				});
				if (it.Pinned)
				{
					action("إزالة من الدوك", delegate
					{
						it.Pinned = false;
						if (it.Windows.Count == 0)
						{
							it.AppearTarget = 0.0;
						}
						SavePinned();
						StartRender();
					});
				}
				else
				{
					action("تثبيت في الدوك", delegate
					{
						it.Pinned = true;
						SavePinned();
					});
				}
				string file = ((it.Exe != "" && File.Exists(it.Exe)) ? it.Exe : ((it.Path != "" && File.Exists(it.Path)) ? it.Path : null));
				if (file != null)
				{
					action("إظهار في المجلد", delegate
					{
						Process.Start("explorer.exe", "/select,\"" + file + "\"");
					});
				}
				if (it.Windows.Count > 0)
				{
					menu.Items.Add(new Separator());
					List<IntPtr> wins = new List<IntPtr>(it.Windows);
					action((wins.Count > 1) ? "إغلاق كل النوافذ" : "إغلاق", delegate
					{
						foreach (IntPtr item2 in wins)
						{
							Native.PostMessage(item2, 16u, IntPtr.Zero, IntPtr.Zero);
						}
					});
				}
			}
			menu.Opened += delegate
			{
				menuOpen = true;
				StartRender();
			};
			menu.Closed += delegate
			{
				menuOpen = false;
				StartRender();
			};
			double num = ((it != null) ? it.BaseC : mx);
			menu.PlacementTarget = this;
			menu.Placement = PlacementMode.Top;
			menu.PlacementRectangle = new Rect(num - 1.0, panelTopBase - 10.0, 2.0, 2.0);
			menu.HorizontalOffset = 0.0;
			menu.IsOpen = true;
		}

		private void Click(DockItem it)
		{
			if (it.Kind == "start")
			{
				if (this.LauncherRequested != null)
				{
					this.LauncherRequested();
				}
				else
				{
					OpenStart();
				}
				return;
			}
			if (it.Kind == "trash")
			{
				try
				{
					Process.Start("explorer.exe", "shell:RecycleBinFolder");
					return;
				}
				catch
				{
					return;
				}
			}
			RefreshWindows();
			if (it.Windows.Count == 0)
			{
				Launch(it);
				return;
			}
			IntPtr foregroundWindow = Native.GetForegroundWindow();
			int num = it.Windows.IndexOf(foregroundWindow);
			if (num >= 0 && !Native.IsIconic(foregroundWindow))
			{
				if (it.Windows.Count > 1)
				{
					Activate(it.Windows[it.Windows.Count - 1]);
				}
				else
				{
					Native.ShowWindow(foregroundWindow, 6);
				}
			}
			else
			{
				Activate(it.Windows[0]);
			}
		}

		private static void OpenStart()
		{
			keybd_event(91, 0, 0u, UIntPtr.Zero);
			keybd_event(91, 0, 2u, UIntPtr.Zero);
		}

		internal static void Activate(IntPtr h)
		{
			if (Native.IsIconic(h))
			{
				Native.ShowWindow(h, 9);
			}
			if (!Native.SetForegroundWindow(h))
			{
				IntPtr foregroundWindow = Native.GetForegroundWindow();
				uint windowThreadProcessId = Native.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
				uint currentThreadId = GetCurrentThreadId();
				if (windowThreadProcessId != 0 && windowThreadProcessId != currentThreadId)
				{
					AttachThreadInput(currentThreadId, windowThreadProcessId, attach: true);
				}
				Native.SetForegroundWindow(h);
				BringWindowToTop(h);
				if (windowThreadProcessId != 0 && windowThreadProcessId != currentThreadId)
				{
					AttachThreadInput(currentThreadId, windowThreadProcessId, attach: false);
				}
			}
		}

		private void Launch(DockItem it)
		{
			it.LaunchAt = clock.Elapsed.TotalSeconds;
			it.BounceStop = ((it.Windows.Count > 0) ? 1.2 : (-1.0));
			StartRender();
			try
			{
				string path = it.Path;
				string exe = it.Exe;
				if (path != "" && path.IndexOf("\\WindowsApps\\", StringComparison.OrdinalIgnoreCase) < 0 && (File.Exists(path) || Directory.Exists(path)))
				{
					ProcessStartInfo processStartInfo = new ProcessStartInfo(path, it.Args ?? "");
					processStartInfo.UseShellExecute = true;
					ProcessStartInfo processStartInfo2 = processStartInfo;
					string directoryName = System.IO.Path.GetDirectoryName((it.Exe != "") ? it.Exe : path);
					if (!string.IsNullOrEmpty(directoryName) && Directory.Exists(directoryName))
					{
						processStartInfo2.WorkingDirectory = directoryName;
					}
					Process.Start(processStartInfo2);
				}
				else if (exe != "" && exe.IndexOf("\\WindowsApps\\", StringComparison.OrdinalIgnoreCase) < 0 && File.Exists(exe))
				{
					ProcessStartInfo processStartInfo3 = new ProcessStartInfo(exe);
					processStartInfo3.UseShellExecute = true;
					processStartInfo3.WorkingDirectory = System.IO.Path.GetDirectoryName(exe);
					Process.Start(processStartInfo3);
				}
				else if (it.Aumid != "")
				{
					Process.Start("explorer.exe", "shell:AppsFolder\\" + it.Aumid);
				}
				else
				{
					it.LaunchAt = -1.0;
				}
			}
			catch (Exception ex)
			{
				it.LaunchAt = -1.0;
				Sys.Log("dock launch: " + ex.Message);
			}
		}

		public bool TargetFor(IntPtr h, out double px, out double py, out double pw)
		{
			px = (py = (pw = 0.0));
			if (!cfg.DockEnabled || base.Visibility != Visibility.Visible)
			{
				return false;
			}
			DockItem dockItem = null;
			foreach (DockItem app in apps)
			{
				if (app.Windows.Contains(h))
				{
					dockItem = app;
					break;
				}
			}
			if (dockItem == null)
			{
				Native.GetWindowPid(h, out var pid);
				string b = Native.ProcessPath((int)pid) ?? "";
				foreach (DockItem app2 in apps)
				{
					if (Eq(app2.Exe, b))
					{
						dockItem = app2;
						break;
					}
				}
			}
			if (dockItem == null)
			{
				return false;
			}
			double num = ((shown > 0.5) ? (winH - bottomMargin - padY - S / 2.0) : (winH + S * 0.3));
			px = winL + dockItem.BaseC * dpi;
			py = winT + num * dpi;
			pw = S * 0.9 * dpi;
			return true;
		}

		public void Reload()
		{
			RefreshWindows();
			ApplyConfig();
		}

		public void Shutdown()
		{
			StopRender();
			SetAppBar(on: false);
			Taskbar.Restore();
		}
	}
}
