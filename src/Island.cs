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
using Windows.Media;
using Windows.Media.Control;
using Windows.Storage.Streams;
using Rect = System.Windows.Rect;

namespace MacStyle
{
	internal class Island : Window
	{
		private const double IdleW = 128.0;

		private const double IdleH = 32.0;

		private const double MediaW = 240.0;

		private const double HoverMediaW = 410.0;

		private const double HoverMediaH = 132.0;

		private const double HoverIdleW = 290.0;

		private const double HoverIdleH = 86.0;

		private const double ToastW = 330.0;

		private const double ToastH = 50.0;

		private const double VolW = 270.0;

		private const double VolH = 38.0;

		private const double VolTrack = 150.0;

		public static readonly System.Windows.Media.FontFamily UiFont = new System.Windows.Media.FontFamily("Segoe UI Variable Display, Segoe UI");

		public static readonly System.Windows.Media.FontFamily IconFont = new System.Windows.Media.FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets");

		private static readonly CultureInfo Ar = new CultureInfo("ar-EG");

		private readonly Config cfg;

		private Border pill;

		private Grid host;

		private Grid compactIdle;

		private Grid compactMedia;

		private Grid expandedMedia;

		private Grid expandedIdle;

		private Grid toastLayer;

		private Grid trayLayer;

		private Grid volumeLayer;

		private Border volFill;

		private TextBlock volGlyph;

		private TextBlock volPct;

		private bool volShowing;

		private DispatcherTimer volTimer;

		private WrapPanel trayGrid;

		private TextBlock trayCount;

		private TextBlock trayEmpty;

		private bool trayOpen;

		private bool menuOpen;

		private double trayH = 200.0;

		private TextBlock clockText;

		private TextBlock mediaClock;

		private TextBlock bigClock;

		private TextBlock bigDate;

		private TextBlock titleText;

		private TextBlock artistText;

		private TextBlock playGlyph;

		private TextBlock toastTitle;

		private TextBlock toastSub;

		private ImageBrush artSmallBrush = new ImageBrush
		{
			Stretch = Stretch.UniformToFill
		};

		private ImageBrush artBigBrush = new ImageBrush
		{
			Stretch = Stretch.UniformToFill
		};

		private Border artSmall;

		private Border artBig;

		private Border toastIcon;

		private DropShadowEffect artShadow;

		private System.Windows.Shapes.Rectangle progressFill;

		private Grid progressTrack;

		private System.Windows.Shapes.Rectangle[] bars = new System.Windows.Shapes.Rectangle[4];

		private SolidColorBrush pillBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(253, 0, 0, 0));

		private SolidColorBrush borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(34, byte.MaxValue, byte.MaxValue, byte.MaxValue));

		private DropShadowEffect shadow = new DropShadowEffect
		{
			BlurRadius = 22.0,
			ShadowDepth = 4.0,
			Direction = 270.0,
			Opacity = 0.5,
			Color = Colors.Black
		};

		private DispatcherTimer tick;

		private DispatcherTimer fast;

		private DispatcherTimer toastTimer;

		private GlobalSystemMediaTransportControlsSessionManager mgr;

		private GlobalSystemMediaTransportControlsSession session;

		private bool mediaAvailable;

		private bool playing;

		private bool open;

		private bool toasting;

		private bool polling;

		private bool occluded;

		private readonly IslandStages stages = new IslandStages();

		private readonly Stopwatch clock = Stopwatch.StartNew();

		private long outsideAt = -1L;

		private bool clickThrough;

		private Mon mon;

		private double winW = 500.0;

		private double winH = 440.0;

		private bool dockShown;

		private bool inLine;

		private bool fullscreen;

		private string lastTrackKey = "";

		private string mode = "";

		private FrameworkElement shownLayer;

		private IntPtr hwnd;

		private readonly uint selfPid = (uint)Process.GetCurrentProcess().Id;

		private int tickCount;

		private int fastCount;

		private double dpi = 1.0;

		private DateTime dockLeftAt = DateTime.MinValue;

		private HintLine hint;

		private bool lastTrackHasArt;

		public Audio Audio;

		private AudioApp sourceApp;

		private MacSlider sourceVol;

		private string pickedSource = "";

		private Border prevBtn;

		private Border playBtn;

		private Border nextBtn;

		private Border back10;

		private Border forward10;

		private Border shuffleBtn;

		private Border repeatBtn;

		private Border sourceChip;

		private TextBlock shuffleGlyph;

		private TextBlock repeatGlyph;

		private TextBlock sourceText;

		private TextBlock posText;

		private TextBlock durText;

		private Grid seekArea;

		private Grid times;

		private TimeSpan trackStart;

		private double trackTotal;

		private double trackPos;

		private DateTime seekHold = DateTime.MinValue;

		private bool shuffleOn;

		private MediaPlaybackAutoRepeatMode repeatMode;

		private Grid noticeLayer;

		private Border noticeIcon;

		private Border noticeBar;

		private TextBlock noticeApp;

		private TextBlock noticeTitle;

		private TextBlock noticeBody;

		private Notice notice;

		private long noticeUntil;

		private readonly List<Notice> pending = new List<Notice>();

		public event Action OpenControlCenterRequested;

		public event Action OpenSettingsRequested;

		public Island(Config cfg)
		{
			this.cfg = cfg;
			base.Title = "MacStyle Island";
			base.WindowStyle = WindowStyle.None;
			base.AllowsTransparency = true;
			base.Background = System.Windows.Media.Brushes.Transparent;
			base.Topmost = true;
			base.ShowInTaskbar = false;
			base.ShowActivated = false;
			base.ResizeMode = ResizeMode.NoResize;
			base.Width = 500.0;
			base.Height = 440.0;
			base.Left = 0.0;
			base.Top = 0.0;
			host = new Grid();
			pill = new Border
			{
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(0.0, 3.0, 0.0, 0.0),
				Width = 128.0,
				Height = 32.0,
				Background = pillBrush,
				BorderBrush = borderBrush,
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(16.0),
				Child = host,
				Effect = shadow
			};
			Border border = pill;
			SizeChangedEventHandler value = delegate
			{
				double num2 = Math.Min(pill.ActualHeight / 2.0, 30.0);
				pill.CornerRadius = new CornerRadius(num2);
				host.Clip = new RectangleGeometry(new Rect(0.0, 0.0, Math.Max(0.0, pill.ActualWidth - 2.0), Math.Max(0.0, pill.ActualHeight - 2.0)), Math.Max(0.0, num2 - 1.0), Math.Max(0.0, num2 - 1.0));
			};
			border.SizeChanged += value;
			pill.MouseLeftButtonUp += delegate
			{
				if (!open && mode != "notice")
				{
					open = true;
					RefreshVolume();
					Refresh();
				}
			};
			System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu
			{
				FlowDirection = System.Windows.FlowDirection.RightToLeft
			};
			System.Windows.Controls.MenuItem menuItem = new System.Windows.Controls.MenuItem
			{
				Header = "البرامج اللي في الخلفية"
			};
			menuItem.Click += delegate
			{
				OpenTray();
			};
			contextMenu.Items.Add(menuItem);
			System.Windows.Controls.MenuItem menuItem2 = new System.Windows.Controls.MenuItem
			{
				Header = "مركز التحكم (الواي فاي والصوت)"
			};
			menuItem2.Click += delegate
			{
				if (this.OpenControlCenterRequested != null)
				{
					this.OpenControlCenterRequested();
				}
			};
			contextMenu.Items.Add(menuItem2);
			System.Windows.Controls.MenuItem menuItem3 = new System.Windows.Controls.MenuItem
			{
				Header = "إعدادات MacStyle"
			};
			menuItem3.Click += delegate
			{
				if (this.OpenSettingsRequested != null)
				{
					this.OpenSettingsRequested();
				}
			};
			System.Windows.Controls.MenuItem menuItem4 = new System.Windows.Controls.MenuItem
			{
				Header = "إعادة تشغيل الدوك"
			};
			menuItem4.Click += delegate
			{
				if (Dock.Current != null)
				{
					Dock.Current.Reload();
				}
			};
			System.Windows.Controls.MenuItem menuItem5 = new System.Windows.Controls.MenuItem
			{
				Header = "قفل MacStyle"
			};
			menuItem5.Click += delegate
			{
				System.Windows.Application.Current.Shutdown();
			};
			System.Windows.Controls.MenuItem menuItem6 = new System.Windows.Controls.MenuItem
			{
				Header = "إشعارات الآيلاند",
				IsCheckable = true
			};
			menuItem6.Click += delegate
			{
				cfg.NotifyEnabled = menuItem6.IsChecked;
				cfg.Save();
			};
			contextMenu.Opened += delegate
			{
				menuItem6.IsChecked = cfg.NotifyEnabled;
				menuOpen = true;
			};
			contextMenu.Closed += delegate
			{
				menuOpen = false;
			};
			contextMenu.Items.Add(menuItem6);
			contextMenu.Items.Add(menuItem3);
			contextMenu.Items.Add(menuItem4);
			contextMenu.Items.Add(new Separator());
			contextMenu.Items.Add(menuItem5);
			pill.ContextMenu = contextMenu;
			BuildCompactIdle();
			BuildCompactMedia();
			BuildExpandedMedia();
			BuildExpandedIdle();
			BuildToast();
			BuildTray();
			BuildVolume();
			BuildNotice();
			FrameworkElement[] array = new FrameworkElement[8] { compactIdle, compactMedia, expandedMedia, expandedIdle, toastLayer, trayLayer, volumeLayer, noticeLayer };
			foreach (FrameworkElement frameworkElement in array)
			{
				frameworkElement.Opacity = 0.0;
				frameworkElement.IsHitTestVisible = false;
				host.Children.Add(frameworkElement);
			}
			base.Content = new Grid
			{
				Children = { (UIElement)pill }
			};
			toastTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(2800.0)
			};
			toastTimer.Tick += delegate
			{
				toastTimer.Stop();
				toasting = false;
				Refresh();
			};
			tick = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1.0)
			};
			tick.Tick += delegate
			{
				OnTick();
			};
			fast = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(30.0)
			};
			fast.Tick += delegate
			{
				OnFast();
			};
			base.SourceInitialized += delegate
			{
				hwnd = new WindowInteropHelper(this).Handle;
				Native.MakeOverlay(hwnd, clickThrough: false);
			};
			base.Loaded += delegate
			{
				hint = new HintLine();
				hint.Show();
				Place();
				ApplyConfig();
				UpdateClock();
				Refresh();
				tick.Start();
				fast.Start();
				OnTick();
			};
		}

		public void ApplyConfig()
		{
			System.Windows.Media.Color accentColor = cfg.AccentColor;
			LinearGradientBrush fill = ColorUtil.Gradient(accentColor, 90.0);
			System.Windows.Shapes.Rectangle[] array = bars;
			foreach (System.Windows.Shapes.Rectangle rectangle in array)
			{
				rectangle.Fill = fill;
			}
			progressFill.Fill = ColorUtil.Gradient(accentColor, 0.0);
			toastIcon.Background = ColorUtil.Gradient(accentColor, 45.0);
			artShadow.Color = ColorUtil.Mix(accentColor, Colors.Black, 0.3);
			if (!lastTrackHasArt)
			{
				LinearGradientBrush background = ColorUtil.Gradient(accentColor, 45.0);
				artSmall.Background = background;
				artBig.Background = background;
			}
			UpdateClock();
			UpdateVisibility();
			mode = "";
			Refresh();
		}

		private void Place()
		{
			mon = Screens.Primary();
			dpi = mon.Dpi;
			winW = Math.Min(500.0, mon.W);
			winH = Math.Min(440.0, mon.H);
			int num = (int)Math.Round(winW * dpi);
			Screens.Place(hwnd, mon.CenterX - num / 2, mon.Px.T, num, (int)Math.Round(winH * dpi));
			hint.Place(mon);
		}

		private void UpdateVisibility()
		{
			bool flag = cfg.IslandEnabled && !fullscreen;
			base.Visibility = ((!flag) ? Visibility.Hidden : Visibility.Visible);
		}

		public static TextBlock Text(double size, FontWeight weight, byte alpha)
		{
			TextBlock textBlock = new TextBlock();
			textBlock.FontFamily = UiFont;
			textBlock.FontSize = size;
			textBlock.FontWeight = weight;
			textBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
			return textBlock;
		}

		private void BuildCompactIdle()
		{
			compactIdle = new Grid();
			clockText = Text(13.5, FontWeights.SemiBold, byte.MaxValue);
			clockText.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			compactIdle.Children.Add(clockText);
		}

		private void BuildCompactMedia()
		{
			compactMedia = new Grid
			{
				Margin = new Thickness(6.0, 0.0, 12.0, 0.0)
			};
			compactMedia.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			compactMedia.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			compactMedia.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			artSmall = new Border
			{
				Width = 22.0,
				Height = 22.0,
				CornerRadius = new CornerRadius(6.0),
				Background = artSmallBrush,
				VerticalAlignment = VerticalAlignment.Center
			};
			compactMedia.Children.Add(artSmall);
			mediaClock = Text(12.5, FontWeights.SemiBold, 200);
			mediaClock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			Grid.SetColumn(mediaClock, 1);
			compactMedia.Children.Add(mediaClock);
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			StackPanel stackPanel2 = stackPanel;
			double[] array = new double[4] { 0.42, 0.58, 0.36, 0.5 };
			for (int i = 0; i < 4; i++)
			{
				System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
				rectangle.Width = 3.0;
				rectangle.Height = 15.0;
				rectangle.RadiusX = 1.5;
				rectangle.RadiusY = 1.5;
				rectangle.Margin = new Thickness(1.5, 0.0, 1.5, 0.0);
				rectangle.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
				rectangle.RenderTransform = new ScaleTransform(1.0, 0.3);
				rectangle.Tag = array[i];
				System.Windows.Shapes.Rectangle rectangle2 = rectangle;
				bars[i] = rectangle2;
				stackPanel2.Children.Add(rectangle2);
			}
			Grid.SetColumn(stackPanel2, 2);
			compactMedia.Children.Add(stackPanel2);
		}

		private Border IconButton(string glyph, double size, Action onClick, out TextBlock tb)
		{
			tb = new TextBlock
			{
				Text = glyph,
				FontFamily = IconFont,
				FontSize = size,
				Foreground = System.Windows.Media.Brushes.White,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			Border b = new Border
			{
				Width = size + 18.0,
				Height = size + 14.0,
				CornerRadius = new CornerRadius((size + 14.0) / 2.0),
				Background = System.Windows.Media.Brushes.Transparent,
				Child = tb,
				Margin = new Thickness(4.0, 0.0, 4.0, 0.0)
			};
			SolidColorBrush hoverBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			b.MouseEnter += delegate
			{
				b.Background = hoverBrush;
			};
			b.MouseLeave += delegate
			{
				b.Background = System.Windows.Media.Brushes.Transparent;
			};
			b.MouseLeftButtonDown += delegate
			{
				b.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
				b.RenderTransform = new ScaleTransform(0.88, 0.88);
			};
			b.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				b.RenderTransform = null;
				onClick();
				e.Handled = true;
			};
			return b;
		}

		private void BuildExpandedMedia()
		{
			expandedMedia = new Grid
			{
				Margin = new Thickness(16.0, 14.0, 16.0, 12.0)
			};
			expandedMedia.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			expandedMedia.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			expandedMedia.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			expandedMedia.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(1.0, GridUnitType.Star)
			});
			expandedMedia.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			artShadow = new DropShadowEffect
			{
				BlurRadius = 16.0,
				ShadowDepth = 2.0,
				Opacity = 0.5,
				Direction = 270.0
			};
			artBig = new Border
			{
				Width = 84.0,
				Height = 84.0,
				CornerRadius = new CornerRadius(16.0),
				Background = artBigBrush,
				VerticalAlignment = VerticalAlignment.Center,
				Effect = artShadow
			};
			expandedMedia.Children.Add(artBig);
			StackPanel stackPanel = new StackPanel();
			stackPanel.Margin = new Thickness(16.0, 0.0, 0.0, 0.0);
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			Grid.SetColumn(stackPanel, 1);
			titleText = Text(15.0, FontWeights.SemiBold, byte.MaxValue);
			titleText.Margin = new Thickness(0.0, 0.0, 22.0, 0.0);
			artistText = Text(12.5, FontWeights.Normal, 165);
			artistText.Margin = new Thickness(0.0, 1.0, 22.0, 0.0);
			stackPanel.Children.Add(titleText);
			stackPanel.Children.Add(artistText);
			progressTrack = new Grid
			{
				Height = 4.0,
				VerticalAlignment = VerticalAlignment.Center
			};
			progressTrack.Children.Add(new Border
			{
				CornerRadius = new CornerRadius(2.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(55, byte.MaxValue, byte.MaxValue, byte.MaxValue))
			});
			progressFill = new System.Windows.Shapes.Rectangle
			{
				RadiusX = 2.0,
				RadiusY = 2.0,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				Width = 0.0
			};
			progressTrack.Children.Add(progressFill);
			seekArea = new Grid
			{
				Height = 16.0,
				Margin = new Thickness(0.0, 5.0, 0.0, 0.0),
				Background = System.Windows.Media.Brushes.Transparent,
				Cursor = System.Windows.Input.Cursors.Hand
			};
			seekArea.Children.Add(progressTrack);
			Action<System.Windows.Input.MouseEventArgs> seekFrom = delegate(System.Windows.Input.MouseEventArgs e)
			{
				ShowPosition(Math.Max(0.0, Math.Min(1.0, e.GetPosition(seekArea).X / Math.Max(1.0, seekArea.ActualWidth))) * trackTotal);
			};
			seekArea.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
			{
				seekArea.CaptureMouse();
				seekFrom(e);
				e.Handled = true;
			};
			seekArea.MouseMove += delegate(object s, System.Windows.Input.MouseEventArgs e)
			{
				if (seekArea.IsMouseCaptured)
				{
					seekFrom(e);
				}
			};
			seekArea.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				if (seekArea.IsMouseCaptured)
				{
					seekArea.ReleaseMouseCapture();
					SeekTo(trackPos);
					e.Handled = true;
				}
			};
			stackPanel.Children.Add(seekArea);
			times = new Grid();
			posText = Text(10.5, FontWeights.Normal, 150);
			durText = Text(10.5, FontWeights.Normal, 150);
			durText.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			times.Children.Add(posText);
			times.Children.Add(durText);
			stackPanel.Children.Add(times);
			expandedMedia.Children.Add(stackPanel);
			StackPanel stackPanel2 = new StackPanel();
			stackPanel2.Orientation = System.Windows.Controls.Orientation.Horizontal;
			stackPanel2.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			stackPanel2.VerticalAlignment = VerticalAlignment.Center;
			Grid.SetRow(stackPanel2, 1);
			Grid.SetColumnSpan(stackPanel2, 2);
			TextBlock tb;
			shuffleBtn = IconButton("\ue8b1", 14.0, delegate
			{
				MediaCommand(3);
			}, out shuffleGlyph);
			shuffleBtn.ToolTip = "عشوائي";
			prevBtn = IconButton("\ue892", 15.0, delegate
			{
				MediaCommand(0);
			}, out tb);
			back10 = IconButton("‎-10", 12.0, delegate
			{
				Skip(-10.0);
			}, out tb);
			TextButton(back10, tb, "رجوع 10 ثواني");
			playBtn = IconButton("\ue768", 19.0, delegate
			{
				MediaCommand(1);
			}, out playGlyph);
			forward10 = IconButton("‎+10", 12.0, delegate
			{
				Skip(10.0);
			}, out tb);
			TextButton(forward10, tb, "تقديم 10 ثواني");
			nextBtn = IconButton("\ue893", 15.0, delegate
			{
				MediaCommand(2);
			}, out tb);
			repeatBtn = IconButton("\ue8ee", 14.0, delegate
			{
				MediaCommand(4);
			}, out repeatGlyph);
			repeatBtn.ToolTip = "تكرار";
			Border[] array = new Border[7] { shuffleBtn, prevBtn, back10, playBtn, forward10, nextBtn, repeatBtn };
			foreach (Border element in array)
			{
				stackPanel2.Children.Add(element);
			}
			expandedMedia.Children.Add(stackPanel2);
			Grid grid = new Grid();
			Grid.SetRow(grid, 2);
			Grid.SetColumnSpan(grid, 2);
			StackPanel stackPanel3 = new StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Horizontal,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left
			};
			sourceText = Text(11.5, FontWeights.Medium, 220);
			sourceText.MaxWidth = 130.0;
			sourceChip = new Border
			{
				CornerRadius = new CornerRadius(11.0),
				Padding = new Thickness(10.0, 3.0, 10.0, 4.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(34, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				VerticalAlignment = VerticalAlignment.Center,
				Child = sourceText
			};
			sourceChip.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				NextSource();
				e.Handled = true;
			};
			stackPanel3.Children.Add(sourceChip);
			Border border = IconButton("\ue8a7", 12.0, delegate
			{
				if (session != null)
				{
					OpenApp(session.SourceAppUserModelId, titleText.Text);
				}
			}, out tb);
			border.ToolTip = "افتح البرنامج";
			stackPanel3.Children.Add(border);
			grid.Children.Add(stackPanel3);
			sourceVol = new MacSlider("\ue995", 18.0)
			{
				Width = 170.0,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				Visibility = Visibility.Collapsed,
				ToolTip = "صوت البرنامج نفسه (دوس على الأيقونة للكتم)"
			};
			sourceVol.Changed = delegate(double v)
			{
				if (sourceApp != null)
				{
					Audio.SetApp(sourceApp, (float)v);
					sourceApp.Muted = false;
					ShowSourceVolume();
				}
			};
			sourceVol.GlyphClick = delegate
			{
				if (sourceApp != null)
				{
					Audio.MuteApp(sourceApp, !sourceApp.Muted);
					ShowSourceVolume();
				}
			};
			grid.Children.Add(sourceVol);
			expandedMedia.Children.Add(grid);
			TextBlock tb2;
			Border border2 = IconButton("\ue71d", 12.0, delegate
			{
				OpenTray();
			}, out tb2);
			border2.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			border2.VerticalAlignment = VerticalAlignment.Top;
			border2.Margin = new Thickness(0.0, -8.0, -10.0, 0.0);
			border2.ToolTip = "البرامج اللي في الخلفية";
			Grid.SetColumn(border2, 1);
			expandedMedia.Children.Add(border2);
		}

		private static void TextButton(Border b, TextBlock tb, string tip)
		{
			tb.FontFamily = UiFont;
			tb.FontWeight = FontWeights.SemiBold;
			b.Width = 34.0;
			b.ToolTip = tip;
		}

		private static void Enable(Border b, bool on)
		{
			b.IsHitTestVisible = on;
			b.Opacity = (on ? 1.0 : 0.3);
		}

		private static void Show(FrameworkElement b, bool on)
		{
			b.Visibility = ((!on) ? Visibility.Collapsed : Visibility.Visible);
		}

		private void ShowPosition(double seconds)
		{
			trackPos = Math.Max(0.0, Math.Min(trackTotal, seconds));
			progressFill.Width = ((trackTotal > 0.0) ? (progressTrack.ActualWidth * trackPos / trackTotal) : 0.0);
			posText.Text = MediaSource.Time(trackPos);
			durText.Text = MediaSource.Time(trackTotal);
		}

		private void Skip(double by)
		{
			ShowPosition(trackPos + by);
			SeekTo(trackPos);
		}

		private async void SeekTo(double seconds)
		{
			// Players report the new position late; do not let the next poll pull the bar back.
			seekHold = DateTime.Now.AddMilliseconds(1800.0);
			GlobalSystemMediaTransportControlsSession s = session;
			if (s == null)
			{
				return;
			}
			try
			{
				await s.TryChangePlaybackPositionAsync((trackStart + TimeSpan.FromSeconds(seconds)).Ticks);
			}
			catch
			{
			}
		}

		private void NextSource()
		{
			try
			{
				List<string> list = mgr.GetSessions().Select((GlobalSystemMediaTransportControlsSession x) => x.SourceAppUserModelId).ToList();
				if (list.Count < 2)
				{
					return;
				}
				pickedSource = list[(list.IndexOf((session != null) ? session.SourceAppUserModelId : "") + 1) % list.Count];
			}
			catch
			{
				return;
			}
			seekHold = DateTime.MinValue;
			sourceApp = null;
			if (!polling)
			{
				PollMedia();
			}
			RefreshVolume();
		}

		private void RefreshVolume()
		{
			if (Audio == null || session == null || !mediaAvailable || Mouse.Captured != null)
			{
				return;
			}
			string id = session.SourceAppUserModelId;
			sourceApp = Audio.Apps().FirstOrDefault((AudioApp x) => MediaSource.Owns(x.Exe, id));
			if (sourceApp == null && Dock.Current != null)
			{
				WinInfo win = Dock.Current.EnumerateAppWindows().FirstOrDefault((WinInfo x) => string.Equals(x.Aumid, id, StringComparison.OrdinalIgnoreCase));
				if (win != null)
				{
					sourceApp = Audio.Apps().FirstOrDefault((AudioApp x) => string.Equals(x.Exe, win.Exe, StringComparison.OrdinalIgnoreCase));
				}
			}
			Show(sourceVol, sourceApp != null);
			if (sourceApp != null)
			{
				ShowSourceVolume();
			}
		}

		private void ShowSourceVolume()
		{
			sourceVol.Value = (sourceApp.Muted ? 0f : sourceApp.Level);
			sourceVol.SetGlyph((sourceApp.Muted || sourceApp.Level <= 0.001f) ? "\ue74f" : "\ue995");
		}

		private static void OpenApp(string aumid, string hintTitle)
		{
			if (string.IsNullOrEmpty(aumid))
			{
				return;
			}
			try
			{
				List<WinInfo> list = ((Dock.Current == null) ? new List<WinInfo>() : Dock.Current.EnumerateAppWindows().Where((WinInfo x) => string.Equals(x.Aumid, aumid, StringComparison.OrdinalIgnoreCase) || MediaSource.Owns(x.Exe, aumid)).ToList());
				WinInfo win = list.FirstOrDefault((WinInfo x) => !string.IsNullOrEmpty(hintTitle) && ShellApi.Title(x.H).IndexOf(hintTitle, StringComparison.OrdinalIgnoreCase) >= 0) ?? list.FirstOrDefault();
				if (win != null)
				{
					Dock.Activate(win.H);
				}
				else if (ShellApi.DisplayName("shell:AppsFolder\\" + aumid) != null)
				{
					Process.Start("explorer.exe", "\"shell:AppsFolder\\" + aumid + "\"");
				}
			}
			catch (Exception ex)
			{
				Sys.Log("open app: " + ex.Message);
			}
		}

		private void BuildNotice()
		{
			noticeLayer = new Grid
			{
				FlowDirection = System.Windows.FlowDirection.RightToLeft,
				Background = System.Windows.Media.Brushes.Transparent,
				Cursor = System.Windows.Input.Cursors.Hand
			};
			noticeLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			noticeLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			noticeLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			noticeBar = new Border
			{
				IsHitTestVisible = false
			};
			Grid.SetColumnSpan(noticeBar, 3);
			noticeLayer.Children.Add(noticeBar);
			noticeIcon = new Border
			{
				Width = 42.0,
				Height = 42.0,
				CornerRadius = new CornerRadius(12.0),
				Margin = new Thickness(16.0, 0.0, 0.0, 0.0),
				VerticalAlignment = VerticalAlignment.Center
			};
			RenderOptions.SetBitmapScalingMode(noticeIcon, BitmapScalingMode.HighQuality);
			noticeLayer.Children.Add(noticeIcon);
			StackPanel stackPanel = new StackPanel
			{
				Margin = new Thickness(12.0, 0.0, 6.0, 0.0),
				VerticalAlignment = VerticalAlignment.Center
			};
			Grid.SetColumn(stackPanel, 1);
			noticeApp = Text(10.5, FontWeights.SemiBold, 190);
			noticeTitle = Text(13.5, FontWeights.SemiBold, byte.MaxValue);
			noticeBody = Text(12.0, FontWeights.Normal, 190);
			noticeBody.TextWrapping = TextWrapping.Wrap;
			noticeBody.MaxHeight = 33.0;
			stackPanel.Children.Add(noticeApp);
			stackPanel.Children.Add(noticeTitle);
			stackPanel.Children.Add(noticeBody);
			noticeLayer.Children.Add(stackPanel);
			Border border = IconButton("\ue711", 10.0, NextNotice, out var _);
			border.Margin = new Thickness(0.0, 0.0, 10.0, 0.0);
			border.ToolTip = "اقفل الإشعار";
			Grid.SetColumn(border, 2);
			noticeLayer.Children.Add(border);
			noticeLayer.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				e.Handled = true;
				if (notice != null)
				{
					OpenApp(notice.Aumid, "");
					NextNotice();
				}
			};
		}

		public void ShowNotice(Notice n)
		{
			if (!cfg.IslandEnabled || fullscreen)
			{
				return;
			}
			if (notice == null || notice.Aumid == n.Aumid)
			{
				SetNotice(n);
				return;
			}
			pending.RemoveAll((Notice x) => x.Aumid == n.Aumid);
			pending.Add(n);
		}

		private void NextNotice()
		{
			Notice n = pending.FirstOrDefault();
			if (n != null)
			{
				pending.RemoveAt(0);
			}
			SetNotice(n);
		}

		private void SetNotice(Notice n)
		{
			notice = n;
			if (n != null)
			{
				System.Windows.Media.Color c = cfg.NotifyAccent;
				noticeBar.Background = new LinearGradientBrush(System.Windows.Media.Color.FromArgb(120, c.R, c.G, c.B), System.Windows.Media.Color.FromArgb(0, c.R, c.G, c.B), 0.0);
				noticeApp.Foreground = new SolidColorBrush(ColorUtil.Mix(c, Colors.White, 0.45));
				noticeIcon.Background = ((n.Icon != null) ? null : ColorUtil.Gradient(c, 45.0));
				noticeIcon.Child = ((n.Icon != null) ? (UIElement)new System.Windows.Controls.Image
				{
					Source = n.Icon,
					Stretch = Stretch.Uniform
				} : (UIElement)new TextBlock
				{
					Text = "\uea8f",
					FontFamily = IconFont,
					FontSize = 18.0,
					Foreground = System.Windows.Media.Brushes.White,
					HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
				SetAutoText(noticeApp, n.App);
				SetAutoText(noticeTitle, n.Title);
				SetAutoText(noticeBody, n.Body);
				Show(noticeBody, n.Body != "");
				noticeLayer.LayoutTransform = new ScaleTransform(cfg.NotifyScale, cfg.NotifyScale);
				noticeUntil = clock.ElapsedMilliseconds + cfg.NotifySeconds * 1000;
			}
			mode = "";
			Refresh();
		}

		private static void SetAutoText(TextBlock tb, string text)
		{
			tb.Text = text;
			tb.FlowDirection = (text.Any((char ch) => ch >= '\u0600' && ch <= '\u06ff') ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight);
			tb.TextAlignment = ((tb.FlowDirection == System.Windows.FlowDirection.RightToLeft) ? TextAlignment.Left : TextAlignment.Right);
		}

		private void BuildExpandedIdle()
		{
			expandedIdle = new Grid();
			StackPanel stackPanel = new StackPanel();
			stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			StackPanel stackPanel2 = stackPanel;
			bigClock = Text(30.0, FontWeights.Light, byte.MaxValue);
			bigClock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			bigDate = Text(12.5, FontWeights.Normal, 170);
			bigDate.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			bigDate.FlowDirection = System.Windows.FlowDirection.RightToLeft;
			stackPanel2.Children.Add(bigClock);
			stackPanel2.Children.Add(bigDate);
			Border chip = new Border
			{
				Margin = new Thickness(0.0, 8.0, 0.0, 0.0),
				Padding = new Thickness(12.0, 4.0, 12.0, 5.0),
				CornerRadius = new CornerRadius(11.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(34, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				Cursor = System.Windows.Input.Cursors.Hand,
				Child = new TextBlock
				{
					Text = "البرامج اللي في الخلفية  ‹",
					FontFamily = UiFont,
					FontSize = 11.5,
					Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
					FlowDirection = System.Windows.FlowDirection.RightToLeft
				}
			};
			chip.MouseEnter += delegate
			{
				chip.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(60, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			};
			chip.MouseLeave += delegate
			{
				chip.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(34, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			};
			chip.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				OpenTray();
				e.Handled = true;
			};
			stackPanel2.Children.Add(chip);
			expandedIdle.Children.Add(stackPanel2);
		}

		private void BuildTray()
		{
			trayLayer = new Grid
			{
				Margin = new Thickness(16.0, 12.0, 16.0, 12.0),
				FlowDirection = System.Windows.FlowDirection.RightToLeft
			};
			trayLayer.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			trayLayer.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(1.0, GridUnitType.Star)
			});
			trayLayer.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			Grid grid = new Grid();
			grid.Margin = new Thickness(4.0, 0.0, 4.0, 6.0);
			Grid grid2 = grid;
			grid2.Children.Add(Text(14.0, FontWeights.SemiBold, byte.MaxValue));
			((TextBlock)grid2.Children[0]).Text = "شغالين في الخلفية";
			trayCount = Text(12.0, FontWeights.SemiBold, 150);
			trayCount.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			grid2.Children.Add(trayCount);
			trayLayer.Children.Add(grid2);
			ScrollViewer scrollViewer = new ScrollViewer();
			scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			ScrollViewer scrollViewer2 = scrollViewer;
			Grid grid3 = new Grid();
			trayGrid = new WrapPanel();
			trayEmpty = Text(12.5, FontWeights.Normal, 150);
			trayEmpty.Text = "مفيش برامج شغالة في الخلفية دلوقتي";
			trayEmpty.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			trayEmpty.Margin = new Thickness(0.0, 14.0, 0.0, 14.0);
			grid3.Children.Add(trayGrid);
			grid3.Children.Add(trayEmpty);
			scrollViewer2.Content = grid3;
			Grid.SetRow(scrollViewer2, 1);
			trayLayer.Children.Add(scrollViewer2);
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
			stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			stackPanel.Margin = new Thickness(0.0, 8.0, 0.0, 0.0);
			StackPanel stackPanel2 = stackPanel;
			Border border = TrayAction("\ue701", "الواي فاي والصوت", null);
			border.MouseLeftButtonUp += delegate
			{
				if (this.OpenControlCenterRequested != null)
				{
					this.OpenControlCenterRequested();
				}
			};
			stackPanel2.Children.Add(border);
			stackPanel2.Children.Add(TrayAction("\ue7e7", "الإشعارات", "ms-actioncenter:"));
			Grid.SetRow(stackPanel2, 2);
			trayLayer.Children.Add(stackPanel2);
		}

		private Border TrayAction(string glyph, string label, string uri)
		{
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
			StackPanel stackPanel2 = stackPanel;
			stackPanel2.Children.Add(new TextBlock
			{
				Text = glyph,
				FontFamily = IconFont,
				FontSize = 13.0,
				Foreground = System.Windows.Media.Brushes.White,
				VerticalAlignment = VerticalAlignment.Center
			});
			TextBlock textBlock = Text(12.0, FontWeights.Medium, 230);
			textBlock.Text = label;
			textBlock.Margin = new Thickness(7.0, 0.0, 0.0, 0.0);
			stackPanel2.Children.Add(textBlock);
			SolidColorBrush normal = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			SolidColorBrush hover = new SolidColorBrush(System.Windows.Media.Color.FromArgb(58, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Border b = new Border
			{
				Child = stackPanel2,
				Background = normal,
				CornerRadius = new CornerRadius(14.0),
				Padding = new Thickness(13.0, 6.0, 13.0, 7.0),
				Margin = new Thickness(4.0, 0.0, 4.0, 0.0),
				Cursor = System.Windows.Input.Cursors.Hand
			};
			b.MouseEnter += delegate
			{
				b.Background = hover;
			};
			b.MouseLeave += delegate
			{
				b.Background = normal;
			};
			b.MouseLeftButtonUp += delegate
			{
				if (uri != null)
				{
					try
					{
						Process.Start(new ProcessStartInfo(uri)
						{
							UseShellExecute = true
						});
					}
					catch
					{
					}
				}
				CloseTray();
			};
			return b;
		}

		private FrameworkElement TrayTile(TrayApp app)
		{
			StackPanel stackPanel = new StackPanel();
			stackPanel.Width = 74.0;
			stackPanel.Margin = new Thickness(0.0, 2.0, 0.0, 6.0);
			stackPanel.Cursor = System.Windows.Input.Cursors.Hand;
			stackPanel.Background = System.Windows.Media.Brushes.Transparent;
			stackPanel.ToolTip = app.Name;
			StackPanel stackPanel2 = stackPanel;
			SolidColorBrush normal = new SolidColorBrush(System.Windows.Media.Color.FromArgb(22, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			SolidColorBrush hover = new SolidColorBrush(System.Windows.Media.Color.FromArgb(55, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Border box = new Border
			{
				Width = 48.0,
				Height = 48.0,
				CornerRadius = new CornerRadius(13.0),
				Background = normal,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center
			};
			if (app.Icon != null)
			{
				box.Child = new System.Windows.Controls.Image
				{
					Source = app.Icon,
					Width = 28.0,
					Height = 28.0,
					Stretch = Stretch.Uniform
				};
			}
			else
			{
				box.Child = new TextBlock
				{
					Text = "\ue737",
					FontFamily = IconFont,
					FontSize = 20.0,
					Foreground = System.Windows.Media.Brushes.White,
					HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};
			}
			RenderOptions.SetBitmapScalingMode(box, BitmapScalingMode.HighQuality);
			stackPanel2.Children.Add(box);
			TextBlock textBlock = Text(10.5, FontWeights.Normal, 210);
			textBlock.Text = app.Name;
			textBlock.TextAlignment = TextAlignment.Center;
			textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock.MaxWidth = 70.0;
			textBlock.Margin = new Thickness(0.0, 5.0, 0.0, 0.0);
			textBlock.FlowDirection = System.Windows.FlowDirection.LeftToRight;
			stackPanel2.Children.Add(textBlock);
			stackPanel2.MouseEnter += delegate
			{
				box.Background = hover;
			};
			stackPanel2.MouseLeave += delegate
			{
				box.Background = normal;
			};
			stackPanel2.MouseLeftButtonDown += delegate
			{
				box.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
				box.RenderTransform = new ScaleTransform(0.9, 0.9);
			};
			stackPanel2.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				box.RenderTransform = null;
				TrayApps.Open(app);
				CloseTray();
				e.Handled = true;
			};
			System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();
			contextMenu.FlowDirection = System.Windows.FlowDirection.RightToLeft;
			System.Windows.Controls.ContextMenu contextMenu2 = contextMenu;
			System.Windows.Controls.MenuItem menuItem = new System.Windows.Controls.MenuItem();
			menuItem.Header = "افتح " + app.Name;
			System.Windows.Controls.MenuItem menuItem2 = menuItem;
			menuItem2.Click += delegate
			{
				TrayApps.Open(app);
				CloseTray();
			};
			System.Windows.Controls.MenuItem menuItem3 = new System.Windows.Controls.MenuItem();
			menuItem3.Header = "افتح مكان البرنامج";
			System.Windows.Controls.MenuItem menuItem4 = menuItem3;
			menuItem4.Click += delegate
			{
				TrayApps.ShowInFolder(app);
				CloseTray();
			};
			System.Windows.Controls.MenuItem menuItem5 = new System.Windows.Controls.MenuItem();
			menuItem5.Header = "اقفل البرنامج";
			System.Windows.Controls.MenuItem menuItem6 = menuItem5;
			menuItem6.Click += delegate
			{
				TrayApps.Quit(app);
				ShowToast("اتقفل", app.Name);
			};
			contextMenu2.Items.Add(menuItem2);
			contextMenu2.Items.Add(menuItem4);
			contextMenu2.Items.Add(new Separator());
			contextMenu2.Items.Add(menuItem6);
			contextMenu2.Opened += delegate
			{
				menuOpen = true;
			};
			contextMenu2.Closed += delegate
			{
				menuOpen = false;
			};
			stackPanel2.ContextMenu = contextMenu2;
			return stackPanel2;
		}

		public void OpenTray()
		{
			List<TrayApp> list;
			try
			{
				list = TrayApps.GetRunning();
			}
			catch
			{
				list = new List<TrayApp>();
			}
			trayGrid.Children.Clear();
			foreach (TrayApp item in list)
			{
				trayGrid.Children.Add(TrayTile(item));
			}
			trayEmpty.Visibility = ((list.Count != 0) ? Visibility.Collapsed : Visibility.Visible);
			trayCount.Text = ((list.Count > 0) ? list.Count.ToString() : "");
			int val = Math.Max(1, (int)Math.Ceiling((double)list.Count / 5.0));
			double num = ((list.Count == 0) ? 50 : (Math.Min(3, val) * 86));
			trayH = 54.0 + num + 46.0 + 8.0;
			trayOpen = true;
			open = true;
			mode = "";
			Refresh();
		}

		private void CloseTray()
		{
			trayOpen = false;
			mode = "";
			Refresh();
		}

		private void BuildVolume()
		{
			volumeLayer = new Grid
			{
				Margin = new Thickness(16.0, 0.0, 16.0, 0.0),
				FlowDirection = System.Windows.FlowDirection.LeftToRight
			};
			volumeLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(26.0)
			});
			volumeLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			volumeLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(42.0)
			});
			volGlyph = new TextBlock
			{
				Text = "\ue995",
				FontFamily = IconFont,
				FontSize = 15.0,
				Foreground = System.Windows.Media.Brushes.White,
				VerticalAlignment = VerticalAlignment.Center
			};
			volumeLayer.Children.Add(volGlyph);
			Grid grid = new Grid();
			grid.Width = 150.0;
			grid.Height = 6.0;
			grid.VerticalAlignment = VerticalAlignment.Center;
			grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			grid.Margin = new Thickness(4.0, 0.0, 0.0, 0.0);
			Grid grid2 = grid;
			grid2.Children.Add(new Border
			{
				CornerRadius = new CornerRadius(3.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(60, byte.MaxValue, byte.MaxValue, byte.MaxValue))
			});
			volFill = new Border
			{
				CornerRadius = new CornerRadius(3.0),
				Background = System.Windows.Media.Brushes.White,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				Width = 0.0
			};
			grid2.Children.Add(volFill);
			Grid.SetColumn(grid2, 1);
			volumeLayer.Children.Add(grid2);
			volPct = Text(12.0, FontWeights.SemiBold, 200);
			volPct.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			volPct.VerticalAlignment = VerticalAlignment.Center;
			Grid.SetColumn(volPct, 2);
			volumeLayer.Children.Add(volPct);
			volTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(1600.0)
			};
			volTimer.Tick += delegate
			{
				volTimer.Stop();
				volShowing = false;
				Refresh();
			};
		}

		public void ShowVolume(float level, bool muted)
		{
			if (cfg.VolumeHud && cfg.IslandEnabled)
			{
				double num = (muted ? 0f : Math.Max(0f, Math.Min(1f, level)));
				volGlyph.Text = ((muted || num <= 0.001) ? "\ue74f" : ((num < 0.34) ? "\ue993" : ((num < 0.67) ? "\ue994" : "\ue995")));
				volPct.Text = (muted ? "صامت" : Math.Round(num * 100.0).ToString(CultureInfo.InvariantCulture));
				volFill.Background = ColorUtil.Gradient(cfg.AccentColor, 0.0);
				volFill.BeginAnimation(FrameworkElement.WidthProperty, new DoubleAnimation(150.0 * num, TimeSpan.FromMilliseconds(130.0))
				{
					EasingFunction = new QuadraticEase
					{
						EasingMode = EasingMode.EaseOut
					}
				});
				volShowing = true;
				volTimer.Stop();
				volTimer.Start();
				if (mode != "volume")
				{
					mode = "";
					Refresh();
				}
			}
		}

		private void BuildToast()
		{
			toastLayer = new Grid
			{
				Margin = new Thickness(10.0, 0.0, 18.0, 0.0),
				FlowDirection = System.Windows.FlowDirection.RightToLeft
			};
			toastLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			toastLayer.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			toastIcon = new Border
			{
				Width = 32.0,
				Height = 32.0,
				CornerRadius = new CornerRadius(16.0),
				Child = new TextBlock
				{
					Text = "\ue722",
					FontFamily = IconFont,
					FontSize = 15.0,
					Foreground = System.Windows.Media.Brushes.White,
					HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				},
				VerticalAlignment = VerticalAlignment.Center
			};
			toastLayer.Children.Add(toastIcon);
			StackPanel stackPanel = new StackPanel();
			stackPanel.Margin = new Thickness(10.0, 0.0, 0.0, 0.0);
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			StackPanel stackPanel2 = stackPanel;
			Grid.SetColumn(stackPanel2, 1);
			toastTitle = Text(13.5, FontWeights.SemiBold, byte.MaxValue);
			toastSub = Text(11.5, FontWeights.Normal, 160);
			toastSub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
			toastSub.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			stackPanel2.Children.Add(toastTitle);
			stackPanel2.Children.Add(toastSub);
			toastLayer.Children.Add(stackPanel2);
		}

		private void Refresh()
		{
			string text = IslandStages.Mode(volShowing, open, trayOpen, mediaAvailable, notice != null, toasting, cfg.IslandAutoLine && occluded, stages.Clock, stages.Near, playing);
			if (!(text == mode))
			{
				mode = text;
				SetLineLook(text == "line" || text == "hidden");
				pill.Margin = new Thickness(0.0, (text == "clock") ? 0 : 3, 0.0, 0.0);
				pill.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation((text == "hidden") ? 0 : 1, TimeSpan.FromMilliseconds(160.0)));
				switch (text)
				{
				case "hidden":
				case "line":
					AnimateSize(84.0, 5.0);
					ShowLayer(null);
					break;
				case "clock":
					AnimateSize(92.0, 28.0);
					ShowLayer(compactIdle);
					break;
				case "notice":
					AnimateSize(360.0 * cfg.NotifyScale, ((notice.Body == "") ? 62.0 : 86.0) * cfg.NotifyScale);
					ShowLayer(noticeLayer);
					break;
				case "toast":
					AnimateSize(330.0, 50.0);
					ShowLayer(toastLayer);
					break;
				case "volume":
					AnimateSize(270.0, 38.0);
					ShowLayer(volumeLayer);
					break;
				case "tray":
					AnimateSize(424.0, trayH);
					ShowLayer(trayLayer);
					break;
				case "hoverMedia":
					AnimateSize(440.0, 200.0);
					ShowLayer(expandedMedia);
					break;
				case "hoverIdle":
					AnimateSize(290.0, 108.0);
					ShowLayer(expandedIdle);
					break;
				case "media":
					AnimateSize(240.0, 32.0);
					ShowLayer(compactMedia);
					break;
				default:
					AnimateSize(128.0, 32.0);
					ShowLayer(compactIdle);
					break;
				}
			}
		}

		private void AnimateSize(double w, double h)
		{
			w = Math.Min(w, winW - 24.0);
			h = Math.Min(h, winH - 30.0);
			Duration duration = new Duration(TimeSpan.FromMilliseconds(520.0));
			BackEase backEase = new BackEase();
			backEase.Amplitude = 0.42;
			backEase.EasingMode = EasingMode.EaseOut;
			IEasingFunction easingFunction = backEase;
			pill.BeginAnimation(FrameworkElement.WidthProperty, new DoubleAnimation(w, duration)
			{
				EasingFunction = easingFunction
			});
			pill.BeginAnimation(FrameworkElement.HeightProperty, new DoubleAnimation(h, duration)
			{
				EasingFunction = easingFunction
			});
		}

		private void ShowLayer(FrameworkElement layer)
		{
			if (shownLayer != layer)
			{
				if (shownLayer != null)
				{
					shownLayer.IsHitTestVisible = false;
					shownLayer.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(110.0)));
				}
				shownLayer = layer;
				if (layer != null)
				{
					layer.IsHitTestVisible = true;
					layer.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(260.0))
					{
						BeginTime = TimeSpan.FromMilliseconds(120.0),
						EasingFunction = new QuadraticEase
						{
							EasingMode = EasingMode.EaseOut
						}
					});
					TranslateTransform translateTransform = (TranslateTransform)(layer.RenderTransform = new TranslateTransform(0.0, 6.0));
					translateTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(6.0, 0.0, TimeSpan.FromMilliseconds(320.0))
					{
						BeginTime = TimeSpan.FromMilliseconds(100.0),
						EasingFunction = new CubicEase
						{
							EasingMode = EasingMode.EaseOut
						}
					});
				}
			}
		}

		private void SetLineLook(bool line)
		{
			if (line != inLine)
			{
				inLine = line;
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(320.0);
				pillBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(line ? Colors.White : System.Windows.Media.Color.FromArgb(253, 0, 0, 0), timeSpan));
				borderBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(line ? System.Windows.Media.Color.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue) : System.Windows.Media.Color.FromArgb(34, byte.MaxValue, byte.MaxValue, byte.MaxValue), timeSpan));
				shadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(line ? 0.25 : 0.5, timeSpan));
				pill.BorderThickness = new Thickness((!line) ? 1 : 0);
			}
		}

		private void SetBars(bool on)
		{
			System.Windows.Shapes.Rectangle[] array = bars;
			foreach (System.Windows.Shapes.Rectangle rectangle in array)
			{
				ScaleTransform scaleTransform = (ScaleTransform)rectangle.RenderTransform;
				if (on)
				{
					scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.25, 1.0, TimeSpan.FromSeconds((double)rectangle.Tag))
					{
						AutoReverse = true,
						RepeatBehavior = RepeatBehavior.Forever,
						EasingFunction = new SineEase
						{
							EasingMode = EasingMode.EaseInOut
						}
					});
				}
				else
				{
					scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.25, TimeSpan.FromMilliseconds(250.0)));
				}
			}
		}

		private void UpdateClock()
		{
			DateTime now = DateTime.Now;
			string text = now.ToString(cfg.Clock24 ? "H:mm" : "h:mm", CultureInfo.InvariantCulture);
			clockText.Text = text;
			mediaClock.Text = text;
			bigClock.Text = text;
			bigDate.Text = now.ToString("dddd، d MMMM", Ar);
		}

		private void OnTick()
		{
			tickCount++;
			Place();
			UpdateClock();
			if (open && tickCount % 3 == 0)
			{
				RefreshVolume();
			}
			if (!polling && cfg.IslandEnabled)
			{
				PollMedia();
			}
			if (tickCount % 2 == 0)
			{
				bool flag = ForegroundIsFullscreen();
				if (flag != fullscreen)
				{
					fullscreen = flag;
					UpdateVisibility();
				}
				if (base.Visibility == Visibility.Visible)
				{
					Native.TopMost(hwnd);
				}
			}
		}

		private void OnFast()
		{
			if (!Native.GetCursorPos(out var p))
			{
				return;
			}
			long now = clock.ElapsedMilliseconds;
			bool inside = mon.Contains(p);
			double dx = (p.X - mon.CenterX) / dpi;
			double dy = (p.Y - mon.Px.T) / dpi;
			if (fastCount++ % 8 == 0)
			{
				occluded = AppCovers((int)(mon.CenterX - 160.0 * dpi), mon.Px.T, (int)(mon.CenterX + 160.0 * dpi), (int)(mon.Px.T + 46.0 * dpi));
			}
			bool onPill = inside && Math.Abs(dx) <= pill.ActualWidth / 2.0 && dy <= pill.Margin.Top + pill.ActualHeight;
			stages.Update(now, inside && IslandStages.IsNear(dx, dy, stages.Near), inside && p.Y <= mon.Px.T && Math.Abs(dx) <= 64.0, Native.MouseButtonDown(), onPill);
			if (!open || onPill || menuOpen || Mouse.Captured != null)
			{
				outsideAt = -1L;
			}
			else if (outsideAt < 0)
			{
				outsideAt = now;
			}
			else if (now - outsideAt >= 380)
			{
				open = false;
				trayOpen = false;
			}
			if (notice != null)
			{
				if (open || onPill)
				{
					noticeUntil = Math.Max(noticeUntil, now + 1500);
				}
				else if (now >= noticeUntil)
				{
					NextNotice();
				}
			}
			Refresh();
			bool through = !(onPill && IslandStages.Clickable(mode)) && Mouse.Captured == null;
			if (through != clickThrough)
			{
				clickThrough = through;
				Native.SetClickThrough(hwnd, through);
			}
			if (Dock.Current != null)
			{
				dockShown = Dock.Current.IsShown;
			}
			else
			{
				bool flag3 = inside && (mon.Px.B - p.Y) / dpi < 110.0 && Math.Abs(dx) < 420.0;
				if (inside && p.Y >= mon.Px.B - 1)
				{
					dockShown = true;
				}
				else if (dockShown && !flag3)
				{
					if (dockLeftAt == DateTime.MinValue)
					{
						dockLeftAt = DateTime.Now;
					}
					if ((DateTime.Now - dockLeftAt).TotalMilliseconds > 900.0)
					{
						dockShown = false;
						dockLeftAt = DateTime.MinValue;
					}
				}
				if (flag3)
				{
					dockLeftAt = DateTime.MinValue;
				}
			}
			if (hint != null)
			{
				hint.SetShown(cfg.DockEnabled && cfg.DockLine && cfg.DockAutoHide && !dockShown && !fullscreen);
			}
		}

		// Any real app window under the area counts, not just the foreground one: a tab behind a small dialog must stay clickable too.
		private bool AppCovers(int l, int t, int r, int b)
		{
			bool hit = false;
			Native.EnumWindows(delegate(IntPtr h, IntPtr lp)
			{
				if (!Native.IsWindowVisible(h) || Native.IsIconic(h) || (Native.GetWindowLong(h, -20) & 0x80000A0) != 0)
				{
					return true;
				}
				if (!Native.GetWindowRect(h, out var r2) || r2.L >= r || r2.R <= l || r2.T >= b || r2.B <= t)
				{
					return true;
				}
				switch (Native.ClassOf(h))
				{
				case "Progman":
				case "WorkerW":
				case "Shell_TrayWnd":
				case "Shell_SecondaryTrayWnd":
				case "Tauri Window":
					return true;
				}
				Native.GetWindowPid(h, out var pid);
				hit = pid != selfPid && Native.GetWindowTextLength(h) > 0 && !ShellApi.Cloaked(h);
				return !hit;
			}, IntPtr.Zero);
			return hit;
		}

		private bool ForegroundIsFullscreen()
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

		private async void PollMedia()
		{
			polling = true;
			try
			{
				if (mgr == null)
				{
					mgr = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
				}
				IReadOnlyList<GlobalSystemMediaTransportControlsSession> all = mgr.GetSessions();
				session = all.FirstOrDefault((GlobalSystemMediaTransportControlsSession x) => x.SourceAppUserModelId == pickedSource) ?? mgr.GetCurrentSession();
				if (session == null)
				{
					mediaAvailable = false;
					playing = false;
				}
				else
				{
					GlobalSystemMediaTransportControlsSessionPlaybackInfo info = session.GetPlaybackInfo();
					bool nowPlaying = info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
					GlobalSystemMediaTransportControlsSessionMediaProperties props = await session.TryGetMediaPropertiesAsync();
					string title = ((props != null) ? props.Title : "");
					string artist = ((props != null) ? props.Artist : "");
					mediaAvailable = !string.IsNullOrEmpty(title);
					titleText.Text = title;
					artistText.Text = (string.IsNullOrEmpty(artist) ? AppName(session.SourceAppUserModelId) : artist);
					sourceText.Text = AppName(session.SourceAppUserModelId) + ((all.Count > 1) ? "  \u21c4" : "");
					sourceChip.ToolTip = ((all.Count > 1) ? "المصدر اللي بعده" : null);
					sourceChip.Cursor = ((all.Count > 1) ? System.Windows.Input.Cursors.Hand : null);
					string key = session.SourceAppUserModelId + "|" + title + "|" + artist;
					if (key != lastTrackKey)
					{
						lastTrackKey = key;
						BitmapImage img = null;
						try
						{
							if (props.Thumbnail != null)
							{
								img = await LoadThumb(props.Thumbnail);
							}
						}
						catch
						{
						}
						lastTrackHasArt = img != null;
						if (img != null)
						{
							artSmallBrush.ImageSource = img;
							artBigBrush.ImageSource = img;
							artSmall.Background = artSmallBrush;
							artBig.Background = artBigBrush;
						}
						else
						{
							LinearGradientBrush background = ColorUtil.Gradient(cfg.AccentColor, 45.0);
							artSmall.Background = background;
							artBig.Background = background;
						}
					}
					GlobalSystemMediaTransportControlsSessionTimelineProperties tl = session.GetTimelineProperties();
					double pos = (tl.Position - tl.StartTime).TotalSeconds;
					if (nowPlaying)
					{
						pos += (DateTimeOffset.Now - tl.LastUpdatedTime).TotalSeconds * (info.PlaybackRate ?? 1.0);
					}
					trackStart = tl.StartTime;
					trackTotal = (tl.EndTime - tl.StartTime).TotalSeconds;
					if (!seekArea.IsMouseCaptured && DateTime.Now >= seekHold)
					{
						ShowPosition(pos);
					}
					GlobalSystemMediaTransportControlsSessionPlaybackControls can = info.Controls;
					bool seek = can.IsPlaybackPositionEnabled && trackTotal > 0.0;
					Enable(prevBtn, can.IsPreviousEnabled);
					Enable(playBtn, can.IsPlayPauseToggleEnabled || can.IsPlayEnabled || can.IsPauseEnabled);
					Enable(nextBtn, can.IsNextEnabled);
					Show(back10, seek);
					Show(forward10, seek);
					seekArea.IsHitTestVisible = seek;
					Show(times, trackTotal > 0.0);
					Show(shuffleBtn, can.IsShuffleEnabled);
					Show(repeatBtn, can.IsRepeatEnabled);
					shuffleOn = info.IsShuffleActive ?? false;
					repeatMode = info.AutoRepeatMode ?? MediaPlaybackAutoRepeatMode.None;
					SolidColorBrush onBrush = new SolidColorBrush(ColorUtil.Mix(cfg.AccentColor, Colors.White, 0.2));
					shuffleGlyph.Foreground = (shuffleOn ? onBrush : System.Windows.Media.Brushes.White);
					repeatGlyph.Foreground = ((repeatMode != MediaPlaybackAutoRepeatMode.None) ? onBrush : System.Windows.Media.Brushes.White);
					repeatGlyph.Text = ((repeatMode == MediaPlaybackAutoRepeatMode.Track) ? "\ue8ed" : "\ue8ee");
					if (nowPlaying != playing)
					{
						SetBars(nowPlaying);
					}
					playing = nowPlaying;
					playGlyph.Text = (playing ? "\ue769" : "\ue768");
				}
			}
			catch
			{
				session = null;
				mediaAvailable = false;
				playing = false;
			}
			finally
			{
				polling = false;
			}
			Refresh();
		}

		internal static string AppName(string aumid)
		{
			if (string.IsNullOrEmpty(aumid))
			{
				return "";
			}
			string text = aumid;
			int num = text.LastIndexOf('!');
			if (num >= 0)
			{
				text = text.Substring(0, num);
			}
			if (text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(0, text.Length - 4);
			}
			int num2 = text.LastIndexOf('.');
			if (num2 >= 0 && num2 < text.Length - 1)
			{
				text = text.Substring(num2 + 1);
			}
			int num3 = text.IndexOf('_');
			if (num3 > 0)
			{
				text = text.Substring(0, num3);
			}
			return text;
		}

		private static async Task<BitmapImage> LoadThumb(IRandomAccessStreamReference reference)
		{
			IRandomAccessStreamWithContentType ras = await reference.OpenReadAsync();
			uint size = (uint)ras.Size;
			DataReader reader = new DataReader(ras.GetInputStreamAt(0uL));
			byte[] buf = new byte[await reader.LoadAsync(size).ToTask()];
			reader.ReadBytes(buf);
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.CacheOption = BitmapCacheOption.OnLoad;
			bi.DecodePixelWidth = 200;
			bi.StreamSource = new MemoryStream(buf);
			bi.EndInit();
			bi.Freeze();
			return bi;
		}

		private async void MediaCommand(int which)
		{
			GlobalSystemMediaTransportControlsSession s = session;
			if (s == null)
			{
				return;
			}
			try
			{
				switch (which)
				{
				case 0:
					await s.TrySkipPreviousAsync();
					break;
				case 1:
					await s.TryTogglePlayPauseAsync();
					break;
				case 3:
					await s.TryChangeShuffleActiveAsync(!shuffleOn);
					break;
				case 4:
					await s.TryChangeAutoRepeatModeAsync((repeatMode == MediaPlaybackAutoRepeatMode.None) ? MediaPlaybackAutoRepeatMode.List : ((repeatMode == MediaPlaybackAutoRepeatMode.List) ? MediaPlaybackAutoRepeatMode.Track : MediaPlaybackAutoRepeatMode.None));
					break;
				default:
					await s.TrySkipNextAsync();
					break;
				}
			}
			catch
			{
			}
			await Task.Delay(250);
			if (!polling)
			{
				PollMedia();
			}
		}

		public void ShowToast(string title, string sub)
		{
			toastTitle.Text = title;
			toastSub.Text = sub;
			toasting = true;
			toastTimer.Stop();
			toastTimer.Start();
			mode = "";
			Refresh();
		}
	}
	internal class HintLine : Window
	{
		public IntPtr Handle;

		private bool shown;

		private Border line;

		public HintLine()
		{
			base.WindowStyle = WindowStyle.None;
			base.AllowsTransparency = true;
			base.Background = System.Windows.Media.Brushes.Transparent;
			base.Topmost = true;
			base.ShowInTaskbar = false;
			base.ShowActivated = false;
			base.ResizeMode = ResizeMode.NoResize;
			base.Width = 200.0;
			base.Height = 16.0;
			base.Left = 0.0;
			base.Top = 0.0;
			line = new Border
			{
				Width = 134.0,
				Height = 5.0,
				CornerRadius = new CornerRadius(2.5),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(205, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				Opacity = 0.0,
				Effect = new DropShadowEffect
				{
					BlurRadius = 8.0,
					ShadowDepth = 0.0,
					Opacity = 0.6,
					Color = Colors.Black
				}
			};
			base.Content = line;
			EventHandler value = delegate
			{
				Handle = new WindowInteropHelper(this).Handle;
				Native.MakeOverlay(Handle, clickThrough: true);
			};
			base.SourceInitialized += value;
		}

		public void Place(Mon mon)
		{
			int num = (int)Math.Round(200.0 * mon.Dpi);
			int num2 = (int)Math.Round(16.0 * mon.Dpi);
			Screens.Place(Handle, mon.CenterX - num / 2, mon.Px.B - num2, num, num2);
		}

		public void SetShown(bool on)
		{
			if (on != shown)
			{
				shown = on;
				CubicEase cubicEase = new CubicEase();
				cubicEase.EasingMode = EasingMode.EaseOut;
				CubicEase easingFunction = cubicEase;
				line.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(on ? 1 : 0, TimeSpan.FromMilliseconds(on ? 380 : 140))
				{
					EasingFunction = easingFunction
				});
				ScaleTransform scaleTransform = new ScaleTransform(on ? 0.4 : 1.0, 1.0);
				line.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
				line.RenderTransform = scaleTransform;
				scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(on ? 0.4 : 1.0, on ? 1.0 : 0.4, TimeSpan.FromMilliseconds(on ? 420 : 140))
				{
					EasingFunction = easingFunction
				});
				if (on)
				{
					Native.TopMost(Handle);
				}
			}
		}
	}
}
