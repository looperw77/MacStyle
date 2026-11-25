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
	internal static class UI
	{
		public static readonly SolidColorBrush Accent = new SolidColorBrush(System.Windows.Media.Color.FromRgb(167, 110, byte.MaxValue));

		public static readonly SolidColorBrush Card = new SolidColorBrush(System.Windows.Media.Color.FromRgb(27, 23, 35));

		public static readonly SolidColorBrush Off = new SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 52, 70));

		public static readonly SolidColorBrush Muted = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, byte.MaxValue, byte.MaxValue, byte.MaxValue));

		public static TextBlock T(string text, double size, FontWeight w, System.Windows.Media.Brush fg)
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.FontSize = size;
			textBlock.FontWeight = w;
			textBlock.Foreground = fg;
			textBlock.FontFamily = Island.UiFont;
			textBlock.TextWrapping = TextWrapping.Wrap;
			return textBlock;
		}

		public static FrameworkElement Toggle(bool initial, Action<bool> changed)
		{
			Border track = new Border
			{
				Width = 44.0,
				Height = 24.0,
				CornerRadius = new CornerRadius(12.0),
				Background = (initial ? Accent : Off),
				Cursor = System.Windows.Input.Cursors.Hand,
				FlowDirection = System.Windows.FlowDirection.LeftToRight,
				VerticalAlignment = VerticalAlignment.Center
			};
			Ellipse ellipse = new Ellipse();
			ellipse.Width = 18.0;
			ellipse.Height = 18.0;
			ellipse.Fill = System.Windows.Media.Brushes.White;
			ellipse.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			ellipse.Margin = new Thickness(3.0, 0.0, 0.0, 0.0);
			ellipse.Effect = new DropShadowEffect
			{
				BlurRadius = 4.0,
				ShadowDepth = 1.0,
				Opacity = 0.3
			};
			Ellipse ellipse2 = ellipse;
			TranslateTransform tt = new TranslateTransform(initial ? 20 : 0, 0.0);
			ellipse2.RenderTransform = tt;
			track.Child = ellipse2;
			track.MouseLeftButtonUp += delegate
			{
				initial = !initial;
				tt.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(initial ? 20 : 0, TimeSpan.FromMilliseconds(220.0))
				{
					EasingFunction = new BackEase
					{
						Amplitude = 0.4,
						EasingMode = EasingMode.EaseOut
					}
				});
				track.Background = (initial ? Accent : Off);
				changed(initial);
			};
			return track;
		}

		public static FrameworkElement Slider(double min, double max, double value, Func<double, string> fmt, Action<double> changed, Action<double> committed)
		{
			Grid grid = new Grid();
			grid.Width = 230.0;
			grid.Height = 30.0;
			grid.FlowDirection = System.Windows.FlowDirection.LeftToRight;
			grid.Background = System.Windows.Media.Brushes.Transparent;
			grid.Cursor = System.Windows.Input.Cursors.Hand;
			Grid grid2 = grid;
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(46.0)
			});
			Grid area = new Grid
			{
				Background = System.Windows.Media.Brushes.Transparent
			};
			Border border = new Border();
			border.Height = 4.0;
			border.CornerRadius = new CornerRadius(2.0);
			border.Background = Off;
			border.VerticalAlignment = VerticalAlignment.Center;
			Border element = border;
			Border fill = new Border
			{
				Height = 4.0,
				CornerRadius = new CornerRadius(2.0),
				Background = Accent,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center
			};
			Ellipse thumb = new Ellipse
			{
				Width = 18.0,
				Height = 18.0,
				Fill = System.Windows.Media.Brushes.White,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Effect = new DropShadowEffect
				{
					BlurRadius = 6.0,
					ShadowDepth = 1.0,
					Opacity = 0.35
				}
			};
			area.Children.Add(element);
			area.Children.Add(fill);
			area.Children.Add(thumb);
			grid2.Children.Add(area);
			TextBlock label = T(fmt(value), 12.0, FontWeights.SemiBold, Muted);
			label.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			label.VerticalAlignment = VerticalAlignment.Center;
			Grid.SetColumn(label, 1);
			grid2.Children.Add(label);
			double v = value;
			Action layout = delegate
			{
				double num = Math.Max(0.0, area.ActualWidth - 18.0);
				double num2 = (v - min) / (max - min);
				thumb.Margin = new Thickness(num * num2, 0.0, 0.0, 0.0);
				fill.Width = Math.Max(0.0, num * num2 + 9.0);
				label.Text = fmt(v);
			};
			area.SizeChanged += delegate
			{
				layout();
			};
			Action<System.Windows.Input.MouseEventArgs> setFrom = delegate(System.Windows.Input.MouseEventArgs e)
			{
				double num = Math.Max(1.0, area.ActualWidth - 18.0);
				double num2 = Math.Max(0.0, Math.Min(1.0, (e.GetPosition(area).X - 9.0) / num));
				v = min + (max - min) * num2;
				layout();
				changed(v);
			};
			area.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
			{
				area.CaptureMouse();
				setFrom(e);
			};
			area.MouseMove += delegate(object s, System.Windows.Input.MouseEventArgs e)
			{
				if (area.IsMouseCaptured)
				{
					setFrom(e);
				}
			};
			area.MouseLeftButtonUp += delegate
			{
				if (area.IsMouseCaptured)
				{
					area.ReleaseMouseCapture();
					committed(v);
				}
			};
			return grid2;
		}

		public static Border Button(string text, System.Windows.Media.Brush bg, System.Windows.Media.Brush fg, Action click)
		{
			Border b = new Border
			{
				CornerRadius = new CornerRadius(10.0),
				Background = bg,
				Padding = new Thickness(16.0, 8.0, 16.0, 8.0),
				Cursor = System.Windows.Input.Cursors.Hand,
				Child = T(text, 13.0, FontWeights.SemiBold, fg)
			};
			b.MouseEnter += delegate
			{
				b.Opacity = 0.85;
			};
			b.MouseLeave += delegate
			{
				b.Opacity = 1.0;
			};
			b.MouseLeftButtonUp += delegate
			{
				click();
			};
			return b;
		}

		public static FrameworkElement Logo(double size)
		{
			Grid grid = new Grid();
			grid.Width = size;
			grid.Height = size;
			Grid grid2 = grid;
			Border border = new Border();
			border.CornerRadius = new CornerRadius(size * 0.26);
			Border border2 = border;
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
			linearGradientBrush.StartPoint = new System.Windows.Point(0.0, 0.0);
			linearGradientBrush.EndPoint = new System.Windows.Point(1.0, 1.0);
			LinearGradientBrush linearGradientBrush2 = linearGradientBrush;
			linearGradientBrush2.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromRgb(124, 58, 237), 0.0));
			linearGradientBrush2.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromRgb(236, 72, 153), 1.0));
			border2.Background = linearGradientBrush2;
			grid2.Children.Add(border2);
			grid2.Children.Add(new Border
			{
				Width = size * 0.46,
				Height = size * 0.15,
				CornerRadius = new CornerRadius(size * 0.075),
				Background = System.Windows.Media.Brushes.Black,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(0.0, size * 0.16, 0.0, 0.0)
			});
			grid2.Children.Add(new Border
			{
				Width = size * 0.62,
				Height = size * 0.16,
				CornerRadius = new CornerRadius(size * 0.06),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(0.0, 0.0, 0.0, size * 0.15)
			});
			return grid2;
		}
	}
	internal class SettingsWindow : Window
	{
		private readonly Config cfg;

		private readonly Island island;

		private readonly StackPanel body;

		private readonly WrapPanel swatches = new WrapPanel();

		private TextBlock status;

		public SettingsWindow(Config cfg, Island island)
		{
			this.cfg = cfg;
			this.island = island;
			base.Title = "MacStyle";
			Fit(Screens.AtCursor());
			base.WindowStyle = WindowStyle.None;
			base.AllowsTransparency = true;
			base.Background = System.Windows.Media.Brushes.Transparent;
			base.ResizeMode = ResizeMode.NoResize;
			base.WindowStartupLocation = WindowStartupLocation.Manual;
			base.SourceInitialized += delegate
			{
				Center(Screens.AtCursor());
			};
			base.DpiChanged += delegate
			{
				Fit(Screens.AtCursor());
			};
			EventHandler displayChanged = delegate
			{
				Center(Screens.AtCursor());
			};
			SystemEvents.DisplaySettingsChanged += displayChanged;
			base.Closed += delegate
			{
				SystemEvents.DisplaySettingsChanged -= displayChanged;
			};
			base.FlowDirection = System.Windows.FlowDirection.RightToLeft;
			base.FontFamily = Island.UiFont;
			try
			{
				base.Icon = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			catch
			{
			}
			Border border = new Border
			{
				Margin = new Thickness(18.0),
				CornerRadius = new CornerRadius(20.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, 16, 13, 22)),
				BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderThickness = new Thickness(1.0),
				Effect = new DropShadowEffect
				{
					BlurRadius = 30.0,
					ShadowDepth = 6.0,
					Opacity = 0.55,
					Direction = 270.0
				}
			};
			Grid grid = new Grid
			{
				RowDefinitions = 
				{
					new RowDefinition
					{
						Height = GridLength.Auto
					},
					new RowDefinition
					{
						Height = new GridLength(1.0, GridUnitType.Star)
					}
				}
			};
			border.Child = grid;
			base.Content = border;
			Grid grid2 = new Grid
			{
				Margin = new Thickness(24.0, 20.0, 24.0, 10.0),
				Background = System.Windows.Media.Brushes.Transparent
			};
			grid2.MouseLeftButtonDown += delegate
			{
				try
				{
					DragMove();
				}
				catch
				{
				}
			};
			StackPanel stackPanel = new StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Horizontal
			};
			stackPanel.Children.Add(UI.Logo(46.0));
			StackPanel stackPanel2 = new StackPanel
			{
				Margin = new Thickness(14.0, 0.0, 0.0, 0.0),
				VerticalAlignment = VerticalAlignment.Center
			};
			stackPanel2.Children.Add(UI.T("MacStyle", 24.0, FontWeights.SemiBold, System.Windows.Media.Brushes.White));
			stackPanel2.Children.Add(UI.T("by Ahmed Kamel  ·  v2.0", 12.5, FontWeights.Normal, UI.Muted));
			stackPanel.Children.Add(stackPanel2);
			grid2.Children.Add(stackPanel);
			Border close = new Border
			{
				Width = 30.0,
				Height = 30.0,
				CornerRadius = new CornerRadius(15.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(25, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Cursor = System.Windows.Input.Cursors.Hand,
				Child = new TextBlock
				{
					Text = "\ue711",
					FontFamily = Island.IconFont,
					FontSize = 11.0,
					Foreground = System.Windows.Media.Brushes.White,
					HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				}
			};
			close.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
			{
				e.Handled = true;
			};
			close.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				e.Handled = true;
				Close();
			};
			close.MouseEnter += delegate
			{
				close.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 239, 68, 68));
			};
			close.MouseLeave += delegate
			{
				close.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(25, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			};
			base.KeyDown += delegate(object s, System.Windows.Input.KeyEventArgs e)
			{
				if (e.Key == Key.Escape)
				{
					Close();
				}
			};
			grid2.Children.Add(close);
			grid.Children.Add(grid2);
			ScrollViewer scrollViewer = new ScrollViewer
			{
				VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
				Margin = new Thickness(0.0, 0.0, 0.0, 16.0)
			};
			Grid.SetRow(scrollViewer, 1);
			body = new StackPanel
			{
				Margin = new Thickness(24.0, 4.0, 24.0, 8.0)
			};
			scrollViewer.Content = body;
			grid.Children.Add(scrollViewer);
			BuildColors();
			BuildIsland();
			BuildNotify();
			BuildDock();
			BuildPaste();
			BuildAbout();
			UI.Accent.Color = cfg.AccentColor;
		}

		private void Fit(Mon mon)
		{
			base.Width = Math.Min(600.0, mon.WorkW);
			base.Height = Math.Min(760.0, mon.WorkH);
		}

		private void Center(Mon mon)
		{
			Fit(mon);
			int num = (int)(base.Width * mon.Dpi);
			int num2 = (int)(base.Height * mon.Dpi);
			Screens.Place(new WindowInteropHelper(this).Handle, (mon.Work.L + mon.Work.R - num) / 2, (mon.Work.T + mon.Work.B - num2) / 2, num, num2);
		}

		private StackPanel Card(string glyph, string title)
		{
			Border border = new Border();
			border.Background = UI.Card;
			border.CornerRadius = new CornerRadius(16.0);
			border.Padding = new Thickness(18.0, 14.0, 18.0, 14.0);
			border.Margin = new Thickness(0.0, 0.0, 0.0, 12.0);
			Border border2 = border;
			StackPanel stackPanel = new StackPanel();
			StackPanel stackPanel2 = new StackPanel();
			stackPanel2.Orientation = System.Windows.Controls.Orientation.Horizontal;
			stackPanel2.Margin = new Thickness(0.0, 0.0, 0.0, 8.0);
			StackPanel stackPanel3 = stackPanel2;
			stackPanel3.Children.Add(new TextBlock
			{
				Text = glyph,
				FontFamily = Island.IconFont,
				FontSize = 15.0,
				Foreground = UI.Accent,
				VerticalAlignment = VerticalAlignment.Center
			});
			TextBlock textBlock = UI.T(title, 15.5, FontWeights.SemiBold, System.Windows.Media.Brushes.White);
			textBlock.Margin = new Thickness(10.0, 0.0, 0.0, 0.0);
			stackPanel3.Children.Add(textBlock);
			stackPanel.Children.Add(stackPanel3);
			border2.Child = stackPanel;
			body.Children.Add(border2);
			return stackPanel;
		}

		private void Row(StackPanel card, string title, string desc, FrameworkElement control)
		{
			Grid grid = new Grid();
			grid.Margin = new Thickness(0.0, 8.0, 0.0, 8.0);
			Grid grid2 = grid;
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			StackPanel stackPanel = new StackPanel();
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			stackPanel.Margin = new Thickness(0.0, 0.0, 14.0, 0.0);
			StackPanel stackPanel2 = stackPanel;
			stackPanel2.Children.Add(UI.T(title, 13.5, FontWeights.Medium, System.Windows.Media.Brushes.White));
			if (!string.IsNullOrEmpty(desc))
			{
				TextBlock textBlock = UI.T(desc, 11.5, FontWeights.Normal, UI.Muted);
				textBlock.Margin = new Thickness(0.0, 2.0, 0.0, 0.0);
				stackPanel2.Children.Add(textBlock);
			}
			grid2.Children.Add(stackPanel2);
			Grid.SetColumn(control, 1);
			grid2.Children.Add(control);
			card.Children.Add(grid2);
		}

		private void Changed()
		{
			cfg.Save();
			island.ApplyConfig();
			if (Dock.Current != null)
			{
				Dock.Current.ApplyConfig();
			}
		}

		private void BuildColors()
		{
			StackPanel stackPanel = Card("\ue790", "اللون");
			stackPanel.Children.Add(UI.T("بيتطبق على الـ Island والدوك و Files", 11.5, FontWeights.Normal, UI.Muted));
			swatches.Margin = new Thickness(0.0, 12.0, 0.0, 4.0);
			stackPanel.Children.Add(swatches);
			DrawSwatches();
		}

		private void DrawSwatches()
		{
			swatches.Children.Clear();
			string[][] colors = Brand.Colors;
			foreach (string[] array in colors)
			{
				string text = array[0];
				string hex = array[1];
				bool flag = string.Equals(hex, cfg.Accent, StringComparison.OrdinalIgnoreCase);
				System.Windows.Media.Color col = ColorUtil.FromHex(hex);
				StackPanel stackPanel = new StackPanel();
				stackPanel.Margin = new Thickness(0.0, 0.0, 9.0, 8.0);
				stackPanel.Cursor = System.Windows.Input.Cursors.Hand;
				stackPanel.Width = 44.0;
				StackPanel stackPanel2 = stackPanel;
				Border border = new Border();
				border.Width = 36.0;
				border.Height = 36.0;
				border.CornerRadius = new CornerRadius(18.0);
				border.BorderThickness = new Thickness(2.0);
				border.BorderBrush = (flag ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Transparent);
				border.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				Border border2 = border;
				border2.Child = new Border
				{
					Margin = new Thickness(3.0),
					CornerRadius = new CornerRadius(15.0),
					Background = ColorUtil.Gradient(col, 45.0)
				};
				stackPanel2.Children.Add(border2);
				TextBlock textBlock = UI.T(text, 10.5, flag ? FontWeights.SemiBold : FontWeights.Normal, flag ? System.Windows.Media.Brushes.White : UI.Muted);
				textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				textBlock.Margin = new Thickness(0.0, 4.0, 0.0, 0.0);
				stackPanel2.Children.Add(textBlock);
				stackPanel2.MouseLeftButtonUp += delegate
				{
					cfg.Accent = hex;
					UI.Accent.Color = col;
					Changed();
					DrawSwatches();
					if (FilesApp.Installed && !FilesApp.Apply(cfg))
					{
						SetStatus("لون Files هيتغير لما تقفله وتفتحه تاني من هنا");
					}
				};
				swatches.Children.Add(stackPanel2);
			}
		}

		private void BuildIsland()
		{
			StackPanel card = Card("\ue7f4", "الـ Dynamic Island");
			Row(card, "تشغيل الـ Island", "الحباية اللي فوق في النص: الساعة، الأغاني، والتنبيهات", UI.Toggle(cfg.IslandEnabled, delegate(bool v)
			{
				cfg.IslandEnabled = v;
				Changed();
			}));
			Row(card, "مستخبية جوه البرامج", "قرّب بالماوس يظهر خط أبيض، الزقه في حافة الشاشة من فوق تظهر الساعة، ودوس عليها تتفتح", UI.Toggle(cfg.IslandAutoLine, delegate(bool v)
			{
				cfg.IslandAutoLine = v;
				Changed();
			}));
			Row(card, "ساعة 24 ساعة", "", UI.Toggle(cfg.Clock24, delegate(bool v)
			{
				cfg.Clock24 = v;
				Changed();
			}));
			Row(card, "الصوت يظهر في الـ Island", "لما تعل\u0651ي أو توط\u0651ي الصوت من الكيبورد يظهر فوق بدل بتاع ويندوز", UI.Toggle(cfg.VolumeHud, delegate(bool v)
			{
				cfg.VolumeHud = v;
				Changed();
			}));
			Row(card, "مركز التحكم (Win + A)", "الواي فاي والبلوتوث والصوت وصوت كل برنامج، بشكل الماك", UI.Button("افتحه", new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, byte.MaxValue, byte.MaxValue, byte.MaxValue)), System.Windows.Media.Brushes.White, delegate
			{
				if (ControlCenter.Current != null)
				{
					ControlCenter.Current.Open();
				}
			}));
		}

		private void BuildNotify()
		{
			StackPanel card = Card("\uea8f", "إشعارات ويندوز في الـ Island");
			Row(card, "إظهار الإشعارات", "تقدر تقفلها وتشغّلها كمان بكليك يمين على الـ Island. إشعار ويندوز العادي بيفضل زي ما هو", UI.Toggle(cfg.NotifyEnabled, delegate(bool v)
			{
				cfg.NotifyEnabled = v;
				Changed();
			}));
			if (!Notifier.HasAccess)
			{
				Row(card, "ويندوز مانع الوصول للإشعارات", "افتح الخصوصية واسمح للبرامج تقرا الإشعارات", UI.Button("افتح الخصوصية", UI.Accent, System.Windows.Media.Brushes.White, delegate
				{
					Process.Start(new ProcessStartInfo("ms-settings:privacy-notifications")
					{
						UseShellExecute = true
					});
				}));
			}
			Row(card, "المدة", "الإشعار بيستنى طول ما الماوس عليه", UI.Slider(2.0, 15.0, cfg.NotifySeconds, (double x) => Math.Round(x) + "s", delegate
			{
			}, delegate(double x)
			{
				cfg.NotifySeconds = (int)Math.Round(x);
				cfg.Save();
			}));
			Row(card, "الحجم", "", UI.Slider(0.8, 1.3, cfg.NotifyScale, (double x) => Math.Round(x * 100.0) + "%", delegate
			{
			}, delegate(double x)
			{
				cfg.NotifyScale = Math.Round(x, 2);
				cfg.Save();
			}));
			WrapPanel colors = new WrapPanel
			{
				Width = 230.0
			};
			Action draw = null;
			draw = delegate
			{
				colors.Children.Clear();
				List<string[]> list = new List<string[]> { new string[2] { "زي لون البرنامج", "" } };
				list.AddRange(Brand.Colors);
				foreach (string[] item in list)
				{
					string hex = item[1];
					Border dot = new Border
					{
						Width = 22.0,
						Height = 22.0,
						CornerRadius = new CornerRadius(11.0),
						Margin = new Thickness(0.0, 2.0, 4.0, 2.0),
						BorderThickness = new Thickness(2.0),
						BorderBrush = ((hex == cfg.NotifyColor) ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Transparent),
						Background = ((hex == "") ? (System.Windows.Media.Brush)UI.Accent : new SolidColorBrush(ColorUtil.FromHex(hex))),
						Cursor = System.Windows.Input.Cursors.Hand,
						ToolTip = item[0],
						Child = ((hex == "") ? new TextBlock
						{
							Text = "A",
							FontSize = 11.0,
							FontWeight = FontWeights.Bold,
							Foreground = System.Windows.Media.Brushes.White,
							HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
							VerticalAlignment = VerticalAlignment.Center
						} : null)
					};
					dot.MouseLeftButtonUp += delegate
					{
						cfg.NotifyColor = hex;
						cfg.Save();
						draw();
					};
					colors.Children.Add(dot);
				}
			};
			draw();
			Row(card, "اللون", "", colors);
			Row(card, "جرّب الشكل", "", UI.Button("جرّب", new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, byte.MaxValue, byte.MaxValue, byte.MaxValue)), System.Windows.Media.Brushes.White, delegate
			{
				island.ShowNotice(new Notice
				{
					App = "MacStyle",
					Title = "إشعار تجريبي",
					Body = "كده هيبقى شكل الإشعارات بالمدة والحجم واللون اللي اخترتهم"
				});
			}));
			if (Notifier.Current == null)
			{
				return;
			}
			TextBlock textBlock = UI.T("البرامج اللي تظهر إشعاراتها", 12.5, FontWeights.SemiBold, UI.Muted);
			textBlock.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
			card.Children.Add(textBlock);
			foreach (KeyValuePair<string, string> app in Notifier.Current.Apps())
			{
				string id = app.Key;
				Row(card, app.Value, "", UI.Toggle(!NoticeFilter.IsMuted(cfg.NotifyMuted, id), delegate(bool v)
				{
					cfg.NotifyMuted = NoticeFilter.SetMuted(cfg.NotifyMuted, id, !v);
					cfg.Save();
				}));
			}
		}

		private void BuildDock()
		{
			StackPanel stackPanel = Card("\ue8a9", "الدوك");
			Row(stackPanel, "تشغيل الدوك", "دوك الماك بدل تاسك بار ويندوز (لو قفلته التاسك بار العادي يرجع)", UI.Toggle(cfg.DockEnabled, delegate(bool v)
			{
				cfg.DockEnabled = v;
				Changed();
			}));
			Row(stackPanel, "يستخبى تلقائيا\u064b", "يفضل مستخبي ويطلع لما تلزق الماوس في قعر الشاشة", UI.Toggle(cfg.DockAutoHide, delegate(bool v)
			{
				cfg.DockAutoHide = v;
				Changed();
			}));
			Row(stackPanel, "خط أبيض مكان الدوك", "يبان تحت لما الدوك مستخبي", UI.Toggle(cfg.DockLine, delegate(bool v)
			{
				cfg.DockLine = v;
				Changed();
			}));
			Row(stackPanel, "أنيميشن السحب لما تصغ\u0651ر برنامج", "البرنامج يتسحب لأيقونته في الدوك زي الماك (Genie)", UI.Toggle(cfg.Genie, delegate(bool v)
			{
				cfg.Genie = v;
				Changed();
				if (Genie.Current != null)
				{
					Genie.Current.Apply();
				}
			}));
			Row(stackPanel, "تكبير الأيقونات بالماوس", "قد إيه الأيقونة تكبر لما تقف عليها", UI.Slider(1.0, 2.0, cfg.DockMagnify, (double x) => Math.Round(x * 100.0) + "%", delegate(double x)
			{
				cfg.DockMagnify = x;
				if (Dock.Current != null)
				{
					Dock.Current.ApplyConfig();
				}
			}, delegate
			{
				cfg.Save();
			}));
			Row(stackPanel, "حجم الأيقونات", "", UI.Slider(32.0, 72.0, cfg.DockIconSize, (double x) => Math.Round(x) + "px", delegate
			{
			}, delegate(double x)
			{
				cfg.DockIconSize = (int)Math.Round(x);
				Changed();
			}));
			Row(stackPanel, "زرار الويندوز يفتح قائمة MacStyle", "بدل قائمة ابدأ بتاعة ويندوز (Win + E وباقي الاختصارات شغالة عادي)", UI.Toggle(cfg.WinKeyLauncher, delegate(bool v)
			{
				cfg.WinKeyLauncher = v;
				Changed();
			}));
			stackPanel.Children.Add(UI.T("اسحب أي أيقونة عشان ترتبها، واسحبها لفوق برا الدوك عشان تشيلها. كليك يمين عليها للتثبيت والإغلاق.", 11.5, FontWeights.Normal, UI.Muted));
		}

		private void BuildPaste()
		{
			StackPanel card = Card("\ue722", "لصق السكرين شوت");
			Row(card, "Ctrl + V في الفولدرات والديسكتوب", "يحفظ الصورة اللي في الكليببورد ملف في نفس المكان", UI.Toggle(cfg.PasteEnabled, delegate(bool v)
			{
				cfg.PasteEnabled = v;
				Changed();
			}));
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
			stackPanel.FlowDirection = System.Windows.FlowDirection.LeftToRight;
			StackPanel stackPanel2 = stackPanel;
			Border png = null;
			Border jpg = null;
			Action paint = delegate
			{
				png.Background = ((cfg.PasteFormat == "png") ? UI.Accent : UI.Off);
				jpg.Background = ((cfg.PasteFormat == "jpg") ? UI.Accent : UI.Off);
			};
			png = UI.Button("PNG", UI.Off, System.Windows.Media.Brushes.White, delegate
			{
				cfg.PasteFormat = "png";
				Changed();
				paint();
			});
			jpg = UI.Button("JPG", UI.Off, System.Windows.Media.Brushes.White, delegate
			{
				cfg.PasteFormat = "jpg";
				Changed();
				paint();
			});
			jpg.Margin = new Thickness(6.0, 0.0, 0.0, 0.0);
			stackPanel2.Children.Add(png);
			stackPanel2.Children.Add(jpg);
			paint();
			Row(card, "صيغة الصورة", "PNG أوضح، JPG أصغر في الحجم", stackPanel2);
		}

		private void BuildAbout()
		{
			StackPanel stackPanel = Card("\ue946", "عن البرنامج");
			stackPanel.Children.Add(UI.T("MacStyle v2.0 — صنع Ahmed Kamel", 12.5, FontWeights.Normal, System.Windows.Media.Brushes.White));
			stackPanel.Children.Add(UI.T("الدوك والـ Island من صنع MacStyle نفسه، و Finder على Files (مفتوح المصدر).", 11.5, FontWeights.Normal, UI.Muted));
			status = UI.T("", 12.0, FontWeights.SemiBold, UI.Accent);
			status.Margin = new Thickness(0.0, 8.0, 0.0, 0.0);
			stackPanel.Children.Add(status);
			Border border = UI.Button("إلغاء تثبيت MacStyle", new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 239, 68, 68)), new SolidColorBrush(System.Windows.Media.Color.FromRgb(252, 165, 165)), delegate
			{
				try
				{
					Process.Start(Assembly.GetExecutingAssembly().Location, "--uninstall");
				}
				catch
				{
				}
			});
			border.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			border.Margin = new Thickness(0.0, 10.0, 0.0, 2.0);
			stackPanel.Children.Add(border);
		}

		private void SetStatus(string s)
		{
			if (status != null)
			{
				status.Text = s;
			}
		}
	}
}
