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
	internal static class CC
	{
		public static readonly System.Windows.Media.FontFamily Icons = Island.IconFont;

		public static System.Windows.Media.Brush CardBg = new SolidColorBrush(System.Windows.Media.Color.FromArgb(26, byte.MaxValue, byte.MaxValue, byte.MaxValue));

		public static System.Windows.Media.Brush CardHover = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, byte.MaxValue, byte.MaxValue, byte.MaxValue));

		public static System.Windows.Media.Brush Off = new SolidColorBrush(System.Windows.Media.Color.FromArgb(44, byte.MaxValue, byte.MaxValue, byte.MaxValue));

		public static TextBlock T(string text, double size, FontWeight w, byte alpha)
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.FontSize = size;
			textBlock.FontWeight = w;
			textBlock.FontFamily = Island.UiFont;
			textBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
			TextBlock textBlock2 = textBlock;
			if (!string.IsNullOrEmpty(text) && !text.Any((char ch) => ch >= '\u0600' && ch <= 'ۿ'))
			{
				textBlock2.FlowDirection = System.Windows.FlowDirection.LeftToRight;
				textBlock2.TextAlignment = TextAlignment.Right;
			}
			return textBlock2;
		}

		public static TextBlock G(string glyph, double size, System.Windows.Media.Brush fg)
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = glyph;
			textBlock.FontFamily = Icons;
			textBlock.FontSize = size;
			textBlock.Foreground = fg;
			textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			return textBlock;
		}

		public static Border Card(UIElement child)
		{
			Border border = new Border();
			border.Background = CardBg;
			border.CornerRadius = new CornerRadius(16.0);
			border.Padding = new Thickness(12.0, 10.0, 12.0, 10.0);
			border.Child = child;
			border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(18, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			border.BorderThickness = new Thickness(1.0);
			return border;
		}

		public static void Press(FrameworkElement e, Action click)
		{
			e.Cursor = System.Windows.Input.Cursors.Hand;
			e.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs a)
			{
				e.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
				e.RenderTransform = new ScaleTransform(0.95, 0.95);
				a.Handled = true;
			};
			e.MouseLeave += delegate
			{
				e.RenderTransform = null;
			};
			e.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs a)
			{
				if (e.RenderTransform != null)
				{
					e.RenderTransform = null;
					a.Handled = true;
					click();
				}
			};
		}
	}
	internal class MacSlider : Grid
	{
		private readonly Border fill;

		private readonly Border track;

		private readonly TextBlock glyph;

		private double value;

		private bool dragging;

		public Action<double> Changed;

		public Action<double> Committed;

		public Action GlyphClick;

		public double Value
		{
			get
			{
				return value;
			}
			set
			{
				if (!dragging)
				{
					this.value = Math.Max(0.0, Math.Min(1.0, value));
					Layout();
				}
			}
		}

		public MacSlider(string icon, double height)
		{
			base.FlowDirection = System.Windows.FlowDirection.LeftToRight;
			base.Height = height;
			base.Cursor = System.Windows.Input.Cursors.Hand;
			track = new Border
			{
				CornerRadius = new CornerRadius(height / 2.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, byte.MaxValue, byte.MaxValue, byte.MaxValue))
			};
			fill = new Border
			{
				CornerRadius = new CornerRadius(height / 2.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				Width = height
			};
			base.Children.Add(track);
			base.Children.Add(fill);
			if (icon != null)
			{
				glyph = CC.G(icon, height * 0.5, new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 40, 36, 48)));
				glyph.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
				glyph.Margin = new Thickness(height * 0.3, 0.0, 0.0, 0.0);
				glyph.Width = height * 0.5;
				base.Children.Add(glyph);
			}
			base.SizeChanged += delegate
			{
				Layout();
			};
			base.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
			{
				if (glyph != null && GlyphClick != null && e.GetPosition(this).X < base.Height)
				{
					GlyphClick();
					e.Handled = true;
				}
				else
				{
					dragging = true;
					CaptureMouse();
					From(e);
					e.Handled = true;
				}
			};
			base.MouseMove += delegate(object s, System.Windows.Input.MouseEventArgs e)
			{
				if (dragging)
				{
					From(e);
				}
			};
			base.MouseLeftButtonUp += delegate
			{
				if (dragging)
				{
					dragging = false;
					ReleaseMouseCapture();
					if (Committed != null)
					{
						Committed(value);
					}
				}
			};
		}

		private void From(System.Windows.Input.MouseEventArgs e)
		{
			double num = Math.Max(1.0, base.ActualWidth - base.Height);
			value = Math.Max(0.0, Math.Min(1.0, (e.GetPosition(this).X - base.Height / 2.0) / num));
			Layout();
			if (Changed != null)
			{
				Changed(value);
			}
		}

		public void SetGlyph(string g)
		{
			if (glyph != null)
			{
				glyph.Text = g;
			}
		}

		private void Layout()
		{
			if (!(base.ActualWidth <= 0.0))
			{
				fill.Width = base.Height + (base.ActualWidth - base.Height) * value;
			}
		}
	}
	internal class ControlCenter : Window
	{
		private const double W = 360.0;

		private readonly Config cfg;

		private readonly Audio audio;

		private readonly Border frame;

		private readonly StackPanel body = new StackPanel();

		private readonly ScaleTransform zoom = new ScaleTransform(1.0, 1.0);

		private MacSlider volSlider;

		private MacSlider brightSlider;

		private StackPanel devicePanel;

		private StackPanel appsPanel;

		private bool devicesOpen;

		private bool closing;

		private Radio wifiRadio;

		private Radio btRadio;

		private string page = "main";

		private int wifiConnecting;

		public static ControlCenter Current;

		private bool hasWifi;

		private bool withApps;

		private int brightness = -1;

		private DispatcherTimer outside;

		private Native.LowLevelKeyboardProc mouseProc;

		private IntPtr mouseHook;

		public event Action OpenSettingsRequested;

		public ControlCenter(Config cfg, Audio audio)
		{
			Current = this;
			this.cfg = cfg;
			this.audio = audio;
			base.Title = "MacStyle Control Center";
			base.WindowStyle = WindowStyle.None;
			base.AllowsTransparency = true;
			base.Background = System.Windows.Media.Brushes.Transparent;
			base.Topmost = true;
			base.ShowInTaskbar = false;
			base.ResizeMode = ResizeMode.NoResize;
			base.SizeToContent = SizeToContent.Height;
			base.Width = 400.0;
			base.FlowDirection = System.Windows.FlowDirection.RightToLeft;
			base.FontFamily = Island.UiFont;
			frame = new Border
			{
				Margin = new Thickness(20.0, 6.0, 20.0, 24.0),
				CornerRadius = new CornerRadius(22.0),
				Padding = new Thickness(12.0),
				BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(46, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderThickness = new Thickness(1.0),
				Effect = new DropShadowEffect
				{
					BlurRadius = 28.0,
					ShadowDepth = 8.0,
					Direction = 270.0,
					Opacity = 0.5
				},
				Child = new ScrollViewer
				{
					VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
					Content = body
				},
				RenderTransform = zoom,
				RenderTransformOrigin = new System.Windows.Point(0.5, 0.0)
			};
			base.Content = frame;
			ApplyColors();
			EventHandler value = delegate
			{
				Hide2();
			};
			base.Deactivated += value;
			base.KeyDown += delegate(object s, System.Windows.Input.KeyEventArgs e)
			{
				if (e.Key == Key.Escape)
				{
					Hide2();
				}
			};
			base.SourceInitialized += delegate
			{
				IntPtr handle = new WindowInteropHelper(this).Handle;
				Native.SetWindowLong(handle, -20, Native.GetWindowLong(handle, -20) | 0x80);
			};
			audio.Changed += delegate(float lv, bool mu, bool fromUi)
			{
				if (base.IsVisible && volSlider != null && !fromUi)
				{
					volSlider.Value = (mu ? 0f : lv);
					volSlider.SetGlyph(VolGlyph(lv, mu));
				}
			};
			base.Dispatcher.BeginInvoke(new Action(Prewarm), DispatcherPriority.ApplicationIdle);
			outside = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(60.0)
			};
			outside.Tick += delegate
			{
				CheckOutside();
			};
		}

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint a, uint b, bool attach);

		[DllImport("user32.dll")]
		private static extern bool BringWindowToTop(IntPtr h);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		private void ForceForeground()
		{
			IntPtr handle = new WindowInteropHelper(this).Handle;
			Activate();
			if (!(Native.GetForegroundWindow() == handle))
			{
				IntPtr foregroundWindow = Native.GetForegroundWindow();
				uint windowThreadProcessId = Native.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
				uint currentThreadId = GetCurrentThreadId();
				if (windowThreadProcessId != 0 && windowThreadProcessId != currentThreadId)
				{
					AttachThreadInput(currentThreadId, windowThreadProcessId, attach: true);
				}
				Native.SetForegroundWindow(handle);
				BringWindowToTop(handle);
				if (windowThreadProcessId != 0 && windowThreadProcessId != currentThreadId)
				{
					AttachThreadInput(currentThreadId, windowThreadProcessId, attach: false);
				}
				Activate();
			}
		}

		private void HookMouse(bool on)
		{
			if (on && mouseHook == IntPtr.Zero)
			{
				if (mouseProc == null)
				{
					mouseProc = OnMouse;
				}
				using (Process process = Process.GetCurrentProcess())
				{
					using (ProcessModule processModule = process.MainModule)
					{
						mouseHook = Native.SetWindowsHookEx(14, mouseProc, Native.GetModuleHandle(processModule.ModuleName), 0u);
						return;
					}
				}
			}
			if (!on && mouseHook != IntPtr.Zero)
			{
				Native.UnhookWindowsHookEx(mouseHook);
				mouseHook = IntPtr.Zero;
			}
		}

		private IntPtr OnMouse(int code, IntPtr wParam, IntPtr lParam)
		{
			try
			{
				int num = wParam.ToInt32();
				if (code >= 0 && (num == 513 || num == 516 || num == 519))
				{
					int num2 = Marshal.ReadInt32(lParam);
					int num3 = Marshal.ReadInt32(lParam, 4);
					if (Native.GetWindowRect(new WindowInteropHelper(this).Handle, out var r) && (num2 < r.L || num2 > r.R || num3 < r.T || num3 > r.B))
					{
						base.Dispatcher.BeginInvoke(new Action(Hide2));
					}
				}
			}
			catch
			{
			}
			return Native.CallNextHookEx(mouseHook, code, wParam, lParam);
		}

		private void CheckOutside()
		{
			if (!base.IsVisible || closing)
			{
				outside.Stop();
				HookMouse(on: false);
			}
		}

		private void ApplyColors()
		{
			System.Windows.Media.Color accentColor = cfg.AccentColor;
			System.Windows.Media.Color color = ColorUtil.Mix(System.Windows.Media.Color.FromRgb(38, 34, 48), accentColor, 0.1);
			System.Windows.Media.Color color2 = ColorUtil.Mix(System.Windows.Media.Color.FromRgb(22, 19, 30), accentColor, 0.06);
			frame.Background = new LinearGradientBrush(System.Windows.Media.Color.FromArgb(246, color.R, color.G, color.B), System.Windows.Media.Color.FromArgb(246, color2.R, color2.G, color2.B), 90.0);
		}

		public void Toggle()
		{
			if (base.IsVisible && !closing)
			{
				Hide2();
			}
			else
			{
				Open();
			}
		}

		public void Open()
		{
			closing = false;
			page = "main";
			withApps = false;
			ApplyColors();
			Build();
			Mon mon = Screens.Primary();
			base.MaxHeight = mon.WorkH - 44.0;
			base.Width = Math.Min(400.0, mon.W);
			base.Opacity = 0.0;
			Show();
			Screens.Center(new WindowInteropHelper(this).Handle, mon, Math.Min(400.0, mon.W), 44.0);
			ForceForeground();
			HookMouse(on: true);
			outside.Start();
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(230.0);
			CubicEase cubicEase = new CubicEase();
			cubicEase.EasingMode = EasingMode.EaseOut;
			CubicEase easingFunction = cubicEase;
			BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(160.0)));
			zoom.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.9, 1.0, timeSpan)
			{
				EasingFunction = easingFunction
			});
			zoom.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.86, 1.0, timeSpan)
			{
				EasingFunction = easingFunction
			});
			base.Dispatcher.BeginInvoke((Action)async delegate
			{
				withApps = true;
				if (base.IsVisible && page == "main")
				{
					Build();
				}
				if (brightness >= 0)
				{
					int b = await Task.Run(() => Brightness.Get());
					if (b >= 0)
					{
						brightness = b;
						if (brightSlider != null)
						{
							brightSlider.Value = (double)b / 100.0;
						}
					}
				}
			}, DispatcherPriority.Background);
		}

		private async void Prewarm()
		{
			try
			{
				await Task.Run(delegate
				{
					try
					{
						hasWifi = Wifi.Present;
					}
					catch
					{
					}
					try
					{
						brightness = Brightness.Get();
					}
					catch
					{
					}
				});
				wifiRadio = await Radios.Get(RadioKind.WiFi);
				btRadio = await Radios.Get(RadioKind.Bluetooth);
				if (!base.IsVisible)
				{
					base.ShowActivated = false;
					base.Left = -20000.0;
					base.Opacity = 0.0;
					Build();
					Show();
					Hide();
					base.ShowActivated = true;
				}
			}
			catch (Exception ex)
			{
				Sys.Log("cc prewarm: " + ex.Message);
			}
		}

		private void Hide2()
		{
			if (!base.IsVisible || closing)
			{
				return;
			}
			closing = true;
			DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(130.0));
			doubleAnimation.Completed += delegate
			{
				if (closing)
				{
					Hide();
					closing = false;
				}
			};
			BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
			zoom.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.94, TimeSpan.FromMilliseconds(130.0)));
			zoom.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.92, TimeSpan.FromMilliseconds(130.0)));
		}

		private static string VolGlyph(double lv, bool mu)
		{
			if (mu || lv <= 0.001)
			{
				return "\ue74f";
			}
			if (lv < 0.34)
			{
				return "\ue993";
			}
			if (lv < 0.67)
			{
				return "\ue994";
			}
			return "\ue995";
		}

		private async void LoadRadios()
		{
			wifiRadio = await Radios.Get(RadioKind.WiFi);
			btRadio = await Radios.Get(RadioKind.Bluetooth);
			if (base.IsVisible && page == "main")
			{
				Build();
			}
		}

		private void Build()
		{
			body.Children.Clear();
			if (page == "wifi")
			{
				BuildWifi();
				return;
			}
			Grid grid = new Grid();
			grid.Margin = new Thickness(0.0, 0.0, 0.0, 10.0);
			Grid grid2 = grid;
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.35, GridUnitType.Star)
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(10.0)
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			StackPanel stackPanel = new StackPanel();
			if (hasWifi)
			{
				bool flag = wifiRadio == null || wifiRadio.State == RadioState.On;
				string text = null;
				if (flag)
				{
					foreach (WifiNetwork item in SafeNetworks())
					{
						if (item.Connected)
						{
							text = item.Ssid;
							break;
						}
					}
				}
				stackPanel.Children.Add(NetRow("\ue701", "واي فاي", flag ? (text ?? "مش متصل") : "مقفول", flag, async delegate
				{
					if (wifiRadio == null)
					{
						Launch("ms-settings:network-wifi");
					}
					else
					{
						await Radios.Set(wifiRadio, wifiRadio.State != RadioState.On);
						Build();
					}
				}, delegate
				{
					page = "wifi";
					Wifi.Scan();
					Build();
					RefreshWifiSoon();
				}));
			}
			else
			{
				bool flag2 = false;
				try
				{
					flag2 = NetworkInterface.GetAllNetworkInterfaces().Any((NetworkInterface n) => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
				}
				catch
				{
				}
				bool flag3 = false;
				try
				{
					flag3 = NetworkInterface.GetIsNetworkAvailable();
				}
				catch
				{
				}
				stackPanel.Children.Add(NetRow("\ue839", "الإنترنت", flag2 ? "متصل بالسلك" : (flag3 ? "متصل" : "مش متصل"), flag2 || flag3, null, delegate
				{
					Launch("ms-settings:network");
				}));
			}
			if (btRadio != null)
			{
				bool on = btRadio.State == RadioState.On;
				stackPanel.Children.Add(Gap(8.0));
				stackPanel.Children.Add(NetRow("\ue702", "بلوتوث", on ? "شغال" : "مقفول", on, async delegate
				{
					await Radios.Set(btRadio, !on);
					Build();
				}, delegate
				{
					Launch("ms-settings:bluetooth");
				}));
			}
			else
			{
				stackPanel.Children.Add(Gap(8.0));
				stackPanel.Children.Add(NetRow("\ue713", "الإعدادات", "ويندوز", on: false, null, delegate
				{
					Launch("ms-settings:");
				}));
			}
			Border element = CC.Card(stackPanel);
			grid2.Children.Add(element);
			Grid grid3 = new Grid();
			grid3.RowDefinitions.Add(new RowDefinition());
			grid3.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(8.0)
			});
			grid3.RowDefinitions.Add(new RowDefinition());
			Border element2 = SmallTile("\ue722", "لقطة شاشة", delegate
			{
				Hide2();
				Delay(300, delegate
				{
					Launch("ms-screenclip:");
				});
			});
			Border element3 = SmallTile("\ue771", "MacStyle", delegate
			{
				Hide2();
				if (this.OpenSettingsRequested != null)
				{
					this.OpenSettingsRequested();
				}
			});
			Grid.SetRow(element3, 2);
			grid3.Children.Add(element2);
			grid3.Children.Add(element3);
			Grid.SetColumn(grid3, 2);
			grid2.Children.Add(grid3);
			body.Children.Add(grid2);
			int num = brightness;
			if (num >= 0)
			{
				StackPanel stackPanel2 = new StackPanel();
				stackPanel2.Children.Add(SectionTitle("الشاشة"));
				brightSlider = new MacSlider("\ue706", 26.0)
				{
					Value = (double)num / 100.0
				};
				brightSlider.Changed = delegate(double v)
				{
					Brightness.Set((int)Math.Round(v * 100.0));
				};
				stackPanel2.Children.Add(brightSlider);
				Border border = CC.Card(stackPanel2);
				border.Margin = new Thickness(0.0, 0.0, 0.0, 10.0);
				body.Children.Add(border);
			}
			StackPanel stackPanel3 = new StackPanel();
			stackPanel3.Children.Add(SectionTitle("الصوت"));
			if (audio.Available)
			{
				float level = audio.Level;
				bool muted = audio.Muted;
				volSlider = new MacSlider(VolGlyph(level, muted), 26.0)
				{
					Value = (muted ? 0f : level)
				};
				volSlider.Changed = delegate(double v)
				{
					audio.SetLevel((float)v, fromUi: true);
					volSlider.SetGlyph(VolGlyph(v, mu: false));
				};
				volSlider.GlyphClick = delegate
				{
					bool flag4 = !audio.Muted;
					audio.SetMuted(flag4, fromUi: true);
					volSlider.Value = (flag4 ? 0f : audio.Level);
					volSlider.SetGlyph(VolGlyph(audio.Level, flag4));
				};
				stackPanel3.Children.Add(volSlider);
				List<AudioDevice> list = audio.Devices();
				AudioDevice audioDevice = list.FirstOrDefault((AudioDevice d) => d.IsDefault);
				Grid grid4 = new Grid();
				grid4.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
				grid4.Background = System.Windows.Media.Brushes.Transparent;
				Grid grid5 = grid4;
				grid5.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = GridLength.Auto
				});
				grid5.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = new GridLength(1.0, GridUnitType.Star)
				});
				grid5.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = GridLength.Auto
				});
				TextBlock element4 = CC.G(DeviceGlyph((audioDevice == null) ? "" : audioDevice.Name), 14.0, System.Windows.Media.Brushes.White);
				grid5.Children.Add(element4);
				TextBlock textBlock = CC.T((audioDevice == null) ? "مفيش سماعات" : audioDevice.Name, 12.5, FontWeights.Medium, 230);
				textBlock.Margin = new Thickness(8.0, 0.0, 8.0, 0.0);
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				Grid.SetColumn(textBlock, 1);
				grid5.Children.Add(textBlock);
				TextBlock element5 = CC.G(devicesOpen ? "\ue70e" : "\ue70d", 11.0, new SolidColorBrush(System.Windows.Media.Color.FromArgb(170, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
				Grid.SetColumn(element5, 2);
				grid5.Children.Add(element5);
				if (list.Count > 1)
				{
					CC.Press(grid5, delegate
					{
						devicesOpen = !devicesOpen;
						Build();
					});
				}
				stackPanel3.Children.Add(grid5);
				devicePanel = new StackPanel
				{
					Visibility = ((!devicesOpen) ? Visibility.Collapsed : Visibility.Visible),
					Margin = new Thickness(0.0, 6.0, 0.0, 0.0)
				};
				foreach (AudioDevice item2 in list)
				{
					AudioDevice dd = item2;
					Grid grid6 = ListRow(DeviceGlyph(item2.Name), item2.Name, item2.IsDefault ? "\ue73e" : null, item2.IsDefault);
					CC.Press(grid6, delegate
					{
						audio.SetDefault(dd.Id);
						devicesOpen = false;
						Build();
					});
					devicePanel.Children.Add(grid6);
				}
				stackPanel3.Children.Add(devicePanel);
			}
			else
			{
				stackPanel3.Children.Add(CC.T("مفيش سماعات متوصلة", 12.5, FontWeights.Normal, 170));
			}
			Border border2 = CC.Card(stackPanel3);
			border2.Margin = new Thickness(0.0, 0.0, 0.0, 10.0);
			body.Children.Add(border2);
			List<AudioApp> list2 = ((withApps && audio.Available) ? audio.Apps() : new List<AudioApp>());
			if (list2.Count > 0)
			{
				StackPanel stackPanel4 = new StackPanel();
				stackPanel4.Children.Add(SectionTitle("صوت كل برنامج"));
				foreach (AudioApp item3 in list2.Take(7))
				{
					AudioApp a = item3;
					Grid grid7 = new Grid();
					grid7.Margin = new Thickness(0.0, 4.0, 0.0, 4.0);
					Grid grid8 = grid7;
					grid8.ColumnDefinitions.Add(new ColumnDefinition
					{
						Width = new GridLength(26.0)
					});
					grid8.ColumnDefinitions.Add(new ColumnDefinition
					{
						Width = new GridLength(96.0)
					});
					grid8.ColumnDefinitions.Add(new ColumnDefinition
					{
						Width = new GridLength(1.0, GridUnitType.Star)
					});
					grid8.ColumnDefinitions.Add(new ColumnDefinition
					{
						Width = new GridLength(36.0)
					});
					FrameworkElement element6;
					if (a.Icon != null)
					{
						System.Windows.Controls.Image image = new System.Windows.Controls.Image();
						image.Source = a.Icon;
						image.Width = 20.0;
						image.Height = 20.0;
						System.Windows.Controls.Image image2 = image;
						RenderOptions.SetBitmapScalingMode(image2, BitmapScalingMode.HighQuality);
						element6 = image2;
					}
					else
					{
						element6 = CC.G((a.Exe == "#system") ? "\ue7f5" : "\ue8fc", 15.0, System.Windows.Media.Brushes.White);
					}
					grid8.Children.Add(element6);
					TextBlock textBlock2 = CC.T(a.Name, 12.0, FontWeights.Medium, 225);
					textBlock2.VerticalAlignment = VerticalAlignment.Center;
					textBlock2.Margin = new Thickness(6.0, 0.0, 6.0, 0.0);
					Grid.SetColumn(textBlock2, 1);
					grid8.Children.Add(textBlock2);
					TextBlock pct = CC.T(Math.Round((a.Muted ? 0f : a.Level) * 100f) + "%", 11.0, FontWeights.SemiBold, 160);
					pct.VerticalAlignment = VerticalAlignment.Center;
					pct.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					pct.Margin = new Thickness(6.0, 0.0, 0.0, 0.0);
					pct.FlowDirection = System.Windows.FlowDirection.LeftToRight;
					MacSlider macSlider = new MacSlider(null, 14.0);
					macSlider.Value = (a.Muted ? 0f : a.Level);
					macSlider.VerticalAlignment = VerticalAlignment.Center;
					MacSlider macSlider2 = macSlider;
					macSlider2.Changed = delegate(double v)
					{
						Audio.SetApp(a, (float)v);
						pct.Text = Math.Round(v * 100.0) + "%";
					};
					Grid.SetColumn(macSlider2, 2);
					grid8.Children.Add(macSlider2);
					Grid.SetColumn(pct, 3);
					grid8.Children.Add(pct);
					stackPanel4.Children.Add(grid8);
				}
				body.Children.Add(CC.Card(stackPanel4));
			}
			TextBlock textBlock3 = CC.T("إعدادات الصوت", 11.5, FontWeights.Medium, 150);
			textBlock3.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock3.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
			CC.Press(textBlock3, delegate
			{
				Launch("ms-settings:sound");
			});
			body.Children.Add(textBlock3);
		}

		private List<WifiNetwork> SafeNetworks()
		{
			try
			{
				return Wifi.Networks();
			}
			catch
			{
				return new List<WifiNetwork>();
			}
		}

		private void RefreshWifiSoon()
		{
			Delay(2500, delegate
			{
				if (base.IsVisible && page == "wifi")
				{
					Build();
				}
			});
		}

		private void BuildWifi()
		{
			Grid grid = new Grid();
			grid.Margin = new Thickness(2.0, 2.0, 2.0, 10.0);
			Grid grid2 = grid;
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			Border border = new Border();
			border.Width = 28.0;
			border.Height = 28.0;
			border.CornerRadius = new CornerRadius(14.0);
			border.Background = CC.CardBg;
			border.Child = CC.G("\ue72a", 12.0, System.Windows.Media.Brushes.White);
			Border border2 = border;
			CC.Press(border2, delegate
			{
				page = "main";
				Build();
			});
			grid2.Children.Add(border2);
			TextBlock textBlock = CC.T("واي فاي", 16.0, FontWeights.SemiBold, byte.MaxValue);
			textBlock.Margin = new Thickness(10.0, 0.0, 0.0, 0.0);
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			Grid.SetColumn(textBlock, 1);
			grid2.Children.Add(textBlock);
			bool flag = wifiRadio == null || wifiRadio.State == RadioState.On;
			FrameworkElement element = UI.Toggle(flag, async delegate(bool v)
			{
				if (wifiRadio != null)
				{
					await Radios.Set(wifiRadio, v);
					Delay(1500, delegate
					{
						if (page == "wifi")
						{
							Build();
						}
					});
				}
			});
			Grid.SetColumn(element, 2);
			grid2.Children.Add(element);
			body.Children.Add(grid2);
			StackPanel stackPanel = new StackPanel();
			List<WifiNetwork> list = (flag ? SafeNetworks() : new List<WifiNetwork>());
			if (!flag)
			{
				stackPanel.Children.Add(Center("الواي فاي مقفول"));
			}
			else if (list.Count == 0)
			{
				stackPanel.Children.Add(Center("بندو\u0651ر على شبكات..."));
			}
			foreach (WifiNetwork item in list.Take(12))
			{
				WifiNetwork w = item;
				StackPanel stackPanel2 = new StackPanel();
				string glyph = ((w.Signal > 75) ? "\ue701" : ((w.Signal > 50) ? "\ue874" : ((w.Signal > 25) ? "\ue873" : "\ue872")));
				Grid grid3 = ListRow(glyph, w.Ssid, w.Connected ? "\ue73e" : (w.Secured ? "\ue72e" : null), w.Connected);
				stackPanel2.Children.Add(grid3);
				StackPanel pwBox = new StackPanel
				{
					Visibility = Visibility.Collapsed,
					Margin = new Thickness(30.0, 4.0, 4.0, 8.0)
				};
				PasswordBox pb = new PasswordBox
				{
					Height = 30.0,
					FontSize = 13.0,
					Padding = new Thickness(8.0, 5.0, 8.0, 5.0),
					Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
					Foreground = System.Windows.Media.Brushes.White,
					BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(60, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
					CaretBrush = System.Windows.Media.Brushes.White,
					FlowDirection = System.Windows.FlowDirection.LeftToRight
				};
				TextBlock status = CC.T("", 11.5, FontWeights.Medium, 200);
				status.Margin = new Thickness(0.0, 4.0, 0.0, 0.0);
				Border border3 = UI.Button("اتصال", UI.Accent, System.Windows.Media.Brushes.White, delegate
				{
					DoConnect(w, pb.Password, status);
				});
				border3.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
				border3.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
				pb.KeyDown += delegate(object s, System.Windows.Input.KeyEventArgs e)
				{
					if (e.Key == Key.Return)
					{
						DoConnect(w, pb.Password, status);
					}
				};
				pwBox.Children.Add(CC.T("الباسورد", 11.5, FontWeights.Medium, 170));
				pwBox.Children.Add(pb);
				pwBox.Children.Add(border3);
				pwBox.Children.Add(status);
				stackPanel2.Children.Add(pwBox);
				CC.Press(grid3, delegate
				{
					if (w.Connected)
					{
						Wifi.Disconnect();
						Delay(1200, delegate
						{
							if (page == "wifi")
							{
								Build();
							}
						});
					}
					else if (w.HasProfile || !w.Secured)
					{
						DoConnect(w, null, null);
					}
					else
					{
						pwBox.Visibility = ((pwBox.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible);
						if (pwBox.Visibility == Visibility.Visible)
						{
							pb.Focus();
						}
					}
				});
				stackPanel.Children.Add(stackPanel2);
			}
			ScrollViewer scrollViewer = new ScrollViewer();
			scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			scrollViewer.MaxHeight = 420.0;
			scrollViewer.Content = stackPanel;
			ScrollViewer child = scrollViewer;
			body.Children.Add(CC.Card(child));
			TextBlock textBlock2 = CC.T("إعدادات الواي فاي", 11.5, FontWeights.Medium, 150);
			textBlock2.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock2.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
			CC.Press(textBlock2, delegate
			{
				Launch("ms-settings:network-wifi");
			});
			body.Children.Add(textBlock2);
		}

		private void DoConnect(WifiNetwork w, string password, TextBlock status)
		{
			if (status != null)
			{
				status.Text = "بنتصل...";
			}
			int token = ++wifiConnecting;
			Task.Run(() => Wifi.Connect(w, password)).ContinueWith((Task<string> t) => base.Dispatcher.BeginInvoke((Action)delegate
			{
				if (token == wifiConnecting)
				{
					string text = ((t.Exception == null) ? t.Result : "مقدرتش أتصل");
					if (text != null)
					{
						if (status != null)
						{
							status.Text = text;
						}
					}
					else
					{
						if (status != null)
						{
							status.Text = "اتصلنا ✓";
						}
						Delay(2500, delegate
						{
							if (base.IsVisible && page == "wifi")
							{
								Build();
							}
						});
					}
				}
			}));
		}

		private Grid NetRow(string glyph, string title, string sub, bool on, Action toggle, Action open)
		{
			Grid grid = new Grid();
			grid.Background = System.Windows.Media.Brushes.Transparent;
			Grid grid2 = grid;
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			Border border = new Border();
			border.Width = 32.0;
			border.Height = 32.0;
			border.CornerRadius = new CornerRadius(16.0);
			border.Background = (on ? UI.Accent : CC.Off);
			border.Child = CC.G(glyph, 14.0, System.Windows.Media.Brushes.White);
			Border border2 = border;
			grid2.Children.Add(border2);
			StackPanel stackPanel = new StackPanel();
			stackPanel.Margin = new Thickness(9.0, 0.0, 0.0, 0.0);
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			StackPanel stackPanel2 = stackPanel;
			stackPanel2.Children.Add(CC.T(title, 13.0, FontWeights.SemiBold, byte.MaxValue));
			TextBlock element = CC.T(sub, 11.5, FontWeights.Normal, 165);
			stackPanel2.Children.Add(element);
			Grid.SetColumn(stackPanel2, 1);
			grid2.Children.Add(stackPanel2);
			if (toggle != null)
			{
				CC.Press(border2, toggle);
			}
			if (open != null)
			{
				CC.Press(stackPanel2, open);
			}
			if (toggle == null && open != null)
			{
				CC.Press(border2, open);
			}
			return grid2;
		}

		private Border SmallTile(string glyph, string label, Action click)
		{
			StackPanel stackPanel = new StackPanel();
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			StackPanel stackPanel2 = stackPanel;
			stackPanel2.Children.Add(CC.G(glyph, 18.0, System.Windows.Media.Brushes.White));
			TextBlock textBlock = CC.T(label, 11.5, FontWeights.Medium, 225);
			textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock.Margin = new Thickness(0.0, 5.0, 0.0, 0.0);
			stackPanel2.Children.Add(textBlock);
			Border b = CC.Card(stackPanel2);
			b.MouseEnter += delegate
			{
				b.Background = CC.CardHover;
			};
			b.MouseLeave += delegate
			{
				b.Background = CC.CardBg;
			};
			CC.Press(b, click);
			return b;
		}

		private Grid ListRow(string glyph, string text, string right, bool active)
		{
			Grid g = new Grid
			{
				Background = System.Windows.Media.Brushes.Transparent,
				Margin = new Thickness(0.0, 2.0, 0.0, 2.0),
				MinHeight = 32.0
			};
			g.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(30.0)
			});
			g.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			g.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			Border border = new Border();
			border.Width = 24.0;
			border.Height = 24.0;
			border.CornerRadius = new CornerRadius(12.0);
			border.Background = (active ? UI.Accent : CC.Off);
			border.Child = CC.G(glyph, 11.0, System.Windows.Media.Brushes.White);
			Border element = border;
			g.Children.Add(element);
			TextBlock textBlock = CC.T(text, 12.5, active ? FontWeights.SemiBold : FontWeights.Normal, 235);
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			textBlock.Margin = new Thickness(6.0, 0.0, 6.0, 0.0);
			Grid.SetColumn(textBlock, 1);
			g.Children.Add(textBlock);
			if (right != null)
			{
				TextBlock element2 = CC.G(right, 12.0, new SolidColorBrush(System.Windows.Media.Color.FromArgb(190, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
				Grid.SetColumn(element2, 2);
				g.Children.Add(element2);
			}
			g.MouseEnter += delegate
			{
				g.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(18, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			};
			g.MouseLeave += delegate
			{
				g.Background = System.Windows.Media.Brushes.Transparent;
			};
			return g;
		}

		private static string DeviceGlyph(string name)
		{
			string text = (name ?? "").ToLowerInvariant();
			if (text.Contains("headphone") || text.Contains("headset") || text.Contains("airpods") || text.Contains("buds") || text.Contains("سماعة"))
			{
				return "\ue7f6";
			}
			if (text.Contains("hdmi") || text.Contains("display") || text.Contains("monitor") || text.Contains("tv"))
			{
				return "\ue7f4";
			}
			return "\ue7f5";
		}

		private static TextBlock SectionTitle(string s)
		{
			TextBlock textBlock = CC.T(s, 12.5, FontWeights.SemiBold, 235);
			textBlock.Margin = new Thickness(2.0, 0.0, 0.0, 8.0);
			return textBlock;
		}

		private static TextBlock Center(string s)
		{
			TextBlock textBlock = CC.T(s, 12.5, FontWeights.Normal, 165);
			textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock.Margin = new Thickness(0.0, 14.0, 0.0, 14.0);
			return textBlock;
		}

		private static FrameworkElement Gap(double h)
		{
			Border border = new Border();
			border.Height = h;
			return border;
		}

		private void Launch(string uri)
		{
			Hide2();
			try
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo(uri);
				processStartInfo.UseShellExecute = true;
				Process.Start(processStartInfo);
			}
			catch
			{
			}
		}

		private void Delay(int ms, Action a)
		{
			DispatcherTimer t = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(ms)
			};
			t.Tick += delegate
			{
				t.Stop();
				a();
			};
			t.Start();
		}
	}
}
