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
	internal class AppEntry
	{
		public string Name;

		public string Id;

		public string Lower;

		public ImageSource Icon;

		public bool IconLoaded;

		public double Usage;

		public readonly List<System.Windows.Controls.Image> Images = new List<System.Windows.Controls.Image>();
	}
	internal static class AppIndex
	{
		private static readonly string[] Junk = new string[12]
		{
			"uninstall", "readme", "read me", "documentation", "manual", "help", "release notes", "website", "on the web", "license",
			"faq", "إلغاء تثبيت"
		};

		private static string UsageFile => System.IO.Path.Combine(Brand.DataDir, "launches.txt");

		public static List<AppEntry> Enumerate()
		{
			List<AppEntry> list = new List<AppEntry>();
			try
			{
				dynamic val = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
				dynamic val2 = val.NameSpace("shell:AppsFolder");
				foreach (dynamic item in val2.Items())
				{
					string text = item.Name;
					string text2 = item.Path;
					if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
					{
						string ln = text.ToLowerInvariant();
						string text3 = text2.ToLowerInvariant();
						if (!Junk.Any((string j) => ln.Contains(j)) && !text3.EndsWith(".url") && !text3.EndsWith(".html") && !text3.EndsWith(".htm") && !text3.EndsWith(".chm") && !text3.EndsWith(".txt") && !text3.EndsWith(".pdf") && !text3.StartsWith("http"))
						{
							list.Add(new AppEntry
							{
								Name = text.Trim(),
								Id = text2,
								Lower = ln
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				Sys.Log("apps: " + ex.Message);
			}
			list.Sort((AppEntry a, AppEntry b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
			return list;
		}

		public static Dictionary<string, int> Ours()
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			try
			{
				if (File.Exists(UsageFile))
				{
					string[] array = File.ReadAllLines(UsageFile, Encoding.UTF8);
					foreach (string text in array)
					{
						string[] array2 = text.Split('\t');
						if (array2.Length == 2 && int.TryParse(array2[1], out var result))
						{
							dictionary[array2[0]] = result;
						}
					}
				}
			}
			catch
			{
			}
			return dictionary;
		}

		public static void Count(string id)
		{
			try
			{
				Dictionary<string, int> dictionary = Ours();
				dictionary.TryGetValue(id, out var value);
				dictionary[id] = value + 1;
				Directory.CreateDirectory(Brand.DataDir);
				File.WriteAllLines(UsageFile, dictionary.Select((KeyValuePair<string, int> kv) => kv.Key + "\t" + kv.Value), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			}
			catch
			{
			}
		}

		private static string Rot13(string s)
		{
			char[] array = s.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				char c = array[i];
				if (c >= 'a' && c <= 'z')
				{
					array[i] = (char)(97 + (c - 97 + 13) % 26);
				}
				else if (c >= 'A' && c <= 'Z')
				{
					array[i] = (char)(65 + (c - 65 + 13) % 26);
				}
			}
			return new string(array);
		}

		public static void ScoreUsage(List<AppEntry> apps)
		{
			Dictionary<string, AppEntry> dictionary = new Dictionary<string, AppEntry>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, AppEntry> dictionary2 = new Dictionary<string, AppEntry>(StringComparer.OrdinalIgnoreCase);
			foreach (AppEntry app in apps)
			{
				app.Usage = 0.0;
				dictionary[app.Id] = app;
				string text = SafeFile(app.Id);
				if (text != null && !dictionary2.ContainsKey(text))
				{
					dictionary2[text] = app;
				}
			}
			string[] array = new string[2] { "{CEBFF5CD-ACE2-4F4F-9178-9926F41749EA}", "{F4E57C4B-2036-45F0-A9AB-443BCFE33D9F}" };
			foreach (string text2 in array)
			{
				try
				{
					using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\UserAssist\\" + text2 + "\\Count"))
					{
						if (registryKey == null)
						{
							continue;
						}
						string[] valueNames = registryKey.GetValueNames();
						foreach (string text3 in valueNames)
						{
							if (!(registryKey.GetValue(text3) is byte[] array2) || array2.Length < 72)
							{
								continue;
							}
							int num = BitConverter.ToInt32(array2, 4);
							if (num <= 0)
							{
								continue;
							}
							long num2 = BitConverter.ToInt64(array2, 60);
							double num3 = 365.0;
							try
							{
								if (num2 > 0)
								{
									num3 = Math.Max(0.0, (DateTime.Now - DateTime.FromFileTime(num2)).TotalDays);
								}
							}
							catch
							{
							}
							double num4 = (double)num * ((num3 < 3.0) ? 1.6 : ((num3 < 14.0) ? 1.25 : ((num3 < 60.0) ? 1.0 : 0.6)));
							string text4 = Rot13(text3);
							if (!dictionary.TryGetValue(text4, out var value))
							{
								string text5 = SafeFile(text4);
								if (text5 != null)
								{
									dictionary2.TryGetValue(text5, out value);
								}
							}
							if (value != null)
							{
								value.Usage += num4;
							}
						}
					}
				}
				catch
				{
				}
			}
			foreach (KeyValuePair<string, int> item in Ours())
			{
				if (dictionary.TryGetValue(item.Key, out var value2))
				{
					value2.Usage += item.Value * 6;
				}
			}
		}

		private static string SafeFile(string id)
		{
			try
			{
				int num = Math.Max(id.LastIndexOf('\\'), id.LastIndexOf('/'));
				string text = ((num >= 0) ? id.Substring(num + 1) : id);
				if (text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
				{
					return System.IO.Path.GetFileNameWithoutExtension(text);
				}
			}
			catch
			{
			}
			return null;
		}

		public static ImageSource LoadIcon(AppEntry a)
		{
			ImageSource imageSource = null;
			if (a.Id.Contains("!"))
			{
				imageSource = ShellApi.PackagedLogo(a.Id);
			}
			if (imageSource == null)
			{
				imageSource = ShellApi.Icon("shell:AppsFolder\\" + a.Id, 256, useCache: true);
			}
			return imageSource;
		}

		public static void Launch(AppEntry a)
		{
			try
			{
				Process.Start("explorer.exe", "shell:AppsFolder\\" + a.Id);
			}
			catch (Exception ex)
			{
				Sys.Log("launch: " + ex.Message);
			}
			Task.Run(delegate
			{
				Count(a.Id);
			});
		}
	}
	internal class ResultItem
	{
		public string Title;

		public string Sub;

		public string Glyph;

		public string Kind;

		public ImageSource Icon;

		public Action Run;

		public bool LtrSub;
	}
	internal static class QuickSearch
	{
		private static readonly string[][] Settings = new string[24][]
		{
			new string[3] { "واي فاي wifi wi-fi شبكة network انترنت", "الواي فاي", "ms-settings:network-wifi" },
			new string[3] { "بلوتوث bluetooth", "البلوتوث", "ms-settings:bluetooth" },
			new string[3] { "صوت sound سماعات audio", "الصوت", "ms-settings:sound" },
			new string[3] { "شاشة display عرض resolution دقة سطوع brightness", "الشاشة", "ms-settings:display" },
			new string[3] { "خلفية background wallpaper", "الخلفية", "ms-settings:personalization-background" },
			new string[3] { "ألوان الوان colors color theme ثيم", "الألوان", "ms-settings:colors" },
			new string[3] { "تحديث تحديثات update windows update", "تحديثات ويندوز", "ms-settings:windowsupdate" },
			new string[3] { "برامج apps تطبيقات حذف uninstall", "البرامج المتسطبة", "ms-settings:appsfeatures" },
			new string[3] { "طاقة power بطارية battery sleep سكون", "الطاقة والبطارية", "ms-settings:powersleep" },
			new string[3] { "إشعارات اشعارات notifications", "الإشعارات", "ms-settings:notifications" },
			new string[3] { "لغة language كيبورد keyboard", "اللغة والكيبورد", "ms-settings:regionlanguage" },
			new string[3] { "ماوس mouse تاتش touchpad", "الماوس", "ms-settings:mousetouchpad" },
			new string[3] { "تخزين storage مساحة هارد disk", "مساحة التخزين", "ms-settings:storagesense" },
			new string[3] { "خصوصية privacy كاميرا camera مايك microphone", "الخصوصية", "ms-settings:privacy" },
			new string[3] { "حساب account accounts باسورد password", "الحسابات", "ms-settings:yourinfo" },
			new string[3] { "وقت ساعة تاريخ time date clock", "الوقت والتاريخ", "ms-settings:dateandtime" },
			new string[3] { "طابعة printer printers", "الطابعات", "ms-settings:printers" },
			new string[3] { "ألعاب العاب game gaming", "الألعاب", "ms-settings:gaming-gamebar" },
			new string[3] { "إعدادات اعدادات settings", "إعدادات ويندوز", "ms-settings:" },
			new string[3] { "مدير المهام task manager taskmgr", "مدير المهام", "taskmgr" },
			new string[3] { "لوحة التحكم control panel", "لوحة التحكم", "control" },
			new string[3] { "سلة المحذوفات recycle bin trash", "سلة المحذوفات", "shell:RecycleBinFolder" },
			new string[3] { "التنزيلات downloads تنزيلات", "التنزيلات", "shell:Downloads" },
			new string[3] { "المستندات documents", "المستندات", "shell:Personal" }
		};

		private static readonly Regex MathChars = new Regex("^[0-9\\.\\,\\s\\+\\-\\*/\\(\\)%×÷]+$");

		private static readonly string[] SysDirs = new string[13]
		{
			"\\program files", "\\windows\\", "\\windowsapps", "\\programdata\\", "\\appdata\\", "\\$recycle.bin", "\\node_modules\\", "\\.git\\", "\\site-packages\\", "\\__pycache__\\",
			"\\venv\\", "\\.venv\\", "\\.cache\\"
		};

		private static readonly string[] SysExt = new string[15]
		{
			".pyc", ".dll", ".sys", ".mui", ".cat", ".manifest", ".pri", ".etl", ".dat", ".tmp",
			".log", ".ini", ".db", ".pak", ".bin"
		};

		public static List<ResultItem> SettingsFor(string q)
		{
			List<ResultItem> list = new List<ResultItem>();
			string lq = q.ToLowerInvariant();
			string[][] settings = Settings;
			foreach (string[] array in settings)
			{
				if (array[0].Split(' ').Any((string w) => w.StartsWith(lq) || (lq.Length >= 3 && w.Contains(lq))) || array[1].Contains(q))
				{
					string target = array[2];
					list.Add(new ResultItem
					{
						Title = array[1],
						Sub = "إعداد",
						Glyph = (target.StartsWith("ms-settings") ? "\ue713" : "\ue8b7"),
						Kind = "setting",
						Run = delegate
						{
							Open(target);
						}
					});
					if (list.Count >= 3)
					{
						break;
					}
				}
			}
			return list;
		}

		public static void Open(string target)
		{
			try
			{
				if (target.StartsWith("shell:"))
				{
					Process.Start("explorer.exe", target);
					return;
				}
				ProcessStartInfo processStartInfo = new ProcessStartInfo(target);
				processStartInfo.UseShellExecute = true;
				Process.Start(processStartInfo);
			}
			catch (Exception ex)
			{
				Sys.Log("open: " + ex.Message);
			}
		}

		public static string Calc(string q)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in q)
			{
				stringBuilder.Append((c >= '٠' && c <= '٩') ? ((char)(48 + c - 1632)) : c);
			}
			string text = stringBuilder.ToString().Replace('×', '*').Replace('÷', '/')
				.Replace(',', '.')
				.Trim();
			if (!MathChars.IsMatch(text) || !text.Any(char.IsDigit) || !text.Any((char value) => "+-*/%".IndexOf(value) >= 0))
			{
				return null;
			}
			if ("+-*/%".IndexOf(text[text.Length - 1]) >= 0)
			{
				return null;
			}
			try
			{
				double num = Convert.ToDouble(new DataTable().Compute(text.Replace("%", "/100"), ""), CultureInfo.InvariantCulture);
				if (double.IsNaN(num) || double.IsInfinity(num))
				{
					return null;
				}
				return num.ToString((Math.Abs(num) >= 1000000000000.0) ? "G10" : "#,0.##########", CultureInfo.InvariantCulture);
			}
			catch
			{
				return null;
			}
		}

		private static bool SystemPath(string p)
		{
			string l = p.ToLowerInvariant();
			if (SysDirs.Any((string d) => l.Contains(d)))
			{
				return true;
			}
			string extension = System.IO.Path.GetExtension(l);
			return SysExt.Contains(extension);
		}

		public static List<string> Files(string q)
		{
			List<string> list = new List<string>();
			try
			{
				string[] array = q.Replace("'", " ").Replace("\"", " ").Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 0)
				{
					return list;
				}
				string text = string.Join(" AND ", array.Select((string w) => "\"" + w + "*\""));
				string cmdText = "SELECT TOP 40 System.ItemPathDisplay FROM SystemIndex WHERE SCOPE='file:' AND CONTAINS(System.ItemNameDisplay, '" + text + "') AND System.ItemType <> '.lnk' ORDER BY System.DateModified DESC";
				using (OleDbConnection oleDbConnection = new OleDbConnection("Provider=Search.CollatorDSO;Extended Properties='Application=Windows';"))
				{
					oleDbConnection.Open();
					using (OleDbCommand oleDbCommand = new OleDbCommand(cmdText, oleDbConnection))
					{
						using (OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader())
						{
							while (oleDbDataReader.Read() && list.Count < 6)
							{
								string text2 = oleDbDataReader[0] as string;
								if (!string.IsNullOrEmpty(text2) && !SystemPath(text2))
								{
									list.Add(text2);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Sys.Log("file search: " + ex.Message);
			}
			return list;
		}
	}
	internal static class SmoothScroll
	{
		public static void Attach(ScrollViewer sv)
		{
			double target = 0.0;
			bool running = false;
			EventHandler tick = null;
			tick = delegate
			{
				double verticalOffset = sv.VerticalOffset;
				double num = verticalOffset + (target - verticalOffset) * 0.2;
				if (Math.Abs(target - num) < 0.6)
				{
					num = target;
					CompositionTarget.Rendering -= tick;
					running = false;
				}
				sv.ScrollToVerticalOffset(num);
			};
			sv.PreviewMouseWheel += delegate(object s, MouseWheelEventArgs e)
			{
				if (!running)
				{
					target = sv.VerticalOffset;
				}
				target = Math.Max(0.0, Math.Min(sv.ScrollableHeight, target - (double)e.Delta * 0.9));
				if (!running)
				{
					running = true;
					CompositionTarget.Rendering += tick;
				}
				e.Handled = true;
			};
		}
	}
	internal class Launcher : Window
	{
		private const double W = 700.0;

		private const double BodyH = 480.0;

		private const string ArKeys = "ضصثقفغعهخحجدشسيبلاتنمكطئءؤرىةوزظ";

		private const string EnKeys = "qwertyuiop[]asdfghjkl;'zxcvnm,./";

		public static Launcher Current;

		private readonly Config cfg;

		private readonly Border frame;

		private readonly ScaleTransform zoom = new ScaleTransform(1.0, 1.0);

		private readonly TranslateTransform slide = new TranslateTransform();

		private readonly System.Windows.Controls.TextBox search = new System.Windows.Controls.TextBox();

		private readonly TextBlock hint;

		private readonly Grid home = new Grid();

		private readonly Grid resultsPage = new Grid();

		private readonly StackPanel resultsList = new StackPanel();

		private readonly WrapPanel allGrid = new WrapPanel();

		private readonly WrapPanel frequentRow = new WrapPanel();

		private readonly ScrollViewer homeScroll;

		private readonly StackPanel powerRow = new StackPanel
		{
			Orientation = System.Windows.Controls.Orientation.Horizontal
		};

		private TextBlock greet;

		private List<AppEntry> apps = new List<AppEntry>();

		private List<ResultItem> results = new List<ResultItem>();

		private int selected;

		private bool closing;

		private bool loading;

		private DateTime indexedAt = DateTime.MinValue;

		private readonly DispatcherTimer fileTimer;

		private readonly DispatcherTimer iconTimer;

		private int searchToken;

		private Native.LowLevelKeyboardProc mouseProc;

		private IntPtr mouseHook;

		private readonly Queue<AppEntry> iconQueue = new Queue<AppEntry>();

		private static readonly string[][] Aliases = new string[44][]
		{
			new string[2] { "واتس", "whatsapp" },
			new string[2] { "واتساب", "whatsapp" },
			new string[2] { "كروم", "chrome" },
			new string[2] { "جوجل", "chrome" },
			new string[2] { "بريف", "brave" },
			new string[2] { "اكسل", "excel" },
			new string[2] { "إكسل", "excel" },
			new string[2] { "وورد", "word" },
			new string[2] { "ورد", "word" },
			new string[2] { "باوربوينت", "powerpoint" },
			new string[2] { "بوربوينت", "powerpoint" },
			new string[2] { "فوتوشوب", "photoshop" },
			new string[2] { "تليجرام", "telegram" },
			new string[2] { "تلجرام", "telegram" },
			new string[2] { "فيلمورا", "filmora" },
			new string[2] { "كاب كات", "capcut" },
			new string[2] { "كابكات", "capcut" },
			new string[2] { "المفكرة", "notepad" },
			new string[2] { "مفكرة", "notepad" },
			new string[2] { "نوت باد", "notepad" },
			new string[2] { "الحاسبة", "calculator" },
			new string[2] { "حاسبة", "calculator" },
			new string[2] { "الرسام", "paint" },
			new string[2] { "رسام", "paint" },
			new string[2] { "ستيم", "steam" },
			new string[2] { "ديسكورد", "discord" },
			new string[2] { "فالورانت", "valorant" },
			new string[2] { "سبوتيفاي", "spotify" },
			new string[2] { "زووم", "zoom" },
			new string[2] { "اوداسيتي", "audacity" },
			new string[2] { "في اس كود", "visual studio code" },
			new string[2] { "فيجوال", "visual studio" },
			new string[2] { "بايثون", "python" },
			new string[2] { "الملفات", "file explorer" },
			new string[2] { "مستكشف", "file explorer" },
			new string[2] { "كلود", "claude" },
			new string[2] { "ريوت", "riot" },
			new string[2] { "متجر", "microsoft store" },
			new string[2] { "الأذان", "أذان" },
			new string[2] { "اوتلوك", "outlook" },
			new string[2] { "هاند بريك", "handbrake" },
			new string[2] { "وينرار", "winrar" },
			new string[2] { "في ال سي", "vlc" },
			new string[2] { "مدير المهام", "task manager" }
		};

		[DllImport("user32.dll")]
		private static extern bool LockWorkStation();

		[DllImport("powrprof.dll")]
		private static extern bool SetSuspendState(bool hibernate, bool force, bool disableWake);

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint a, uint b, bool attach);

		[DllImport("user32.dll")]
		private static extern bool BringWindowToTop(IntPtr h);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		public Launcher(Config cfg)
		{
			Current = this;
			this.cfg = cfg;
			base.Title = "MacStyle Launcher";
			base.WindowStyle = WindowStyle.None;
			base.AllowsTransparency = true;
			base.Background = System.Windows.Media.Brushes.Transparent;
			base.Topmost = true;
			base.ShowInTaskbar = false;
			base.ResizeMode = ResizeMode.NoResize;
			base.SizeToContent = SizeToContent.Height;
			base.Width = 748.0;
			base.FlowDirection = System.Windows.FlowDirection.RightToLeft;
			base.FontFamily = Island.UiFont;
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
						Height = new GridLength(480.0)
					},
					new RowDefinition
					{
						Height = GridLength.Auto
					}
				}
			};
			Grid grid2 = new Grid
			{
				Height = 48.0,
				Margin = new Thickness(0.0, 0.0, 0.0, 12.0)
			};
			Border element = new Border
			{
				CornerRadius = new CornerRadius(15.0),
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(24, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderThickness = new Thickness(1.0)
			};
			grid2.Children.Add(element);
			TextBlock textBlock = CC.G("\ue721", 17.0, new SolidColorBrush(System.Windows.Media.Color.FromArgb(190, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
			textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			textBlock.Margin = new Thickness(16.0, 0.0, 0.0, 0.0);
			grid2.Children.Add(textBlock);
			search.Background = System.Windows.Media.Brushes.Transparent;
			search.BorderThickness = new Thickness(0.0);
			search.Foreground = System.Windows.Media.Brushes.White;
			search.CaretBrush = System.Windows.Media.Brushes.White;
			search.FontSize = 17.0;
			search.FontFamily = Island.UiFont;
			search.VerticalContentAlignment = VerticalAlignment.Center;
			search.Margin = new Thickness(44.0, 0.0, 14.0, 0.0);
			search.SelectionBrush = UI.Accent;
			NumberSubstitution.SetSubstitution(search, NumberSubstitutionMethod.European);
			hint = CC.T("دو\u0651ر على برنامج، ملف، إعداد، أو احسب حاجة…", 15.0, FontWeights.Normal, 120);
			hint.VerticalAlignment = VerticalAlignment.Center;
			hint.Margin = new Thickness(46.0, 0.0, 14.0, 0.0);
			hint.IsHitTestVisible = false;
			grid2.Children.Add(hint);
			grid2.Children.Add(search);
			grid.Children.Add(grid2);
			StackPanel stackPanel = new StackPanel
			{
				Children = 
				{
					(UIElement)Shortcuts(),
					(UIElement)SectionTitle("الأكثر استخداما\u064b")
				}
			};
			frequentRow.Margin = new Thickness(0.0, 0.0, 0.0, 6.0);
			stackPanel.Children.Add(frequentRow);
			stackPanel.Children.Add(SectionTitle("كل البرامج"));
			stackPanel.Children.Add(allGrid);
			homeScroll = new ScrollViewer
			{
				VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
				Content = stackPanel,
				CanContentScroll = false
			};
			SmoothScroll.Attach(homeScroll);
			home.Children.Add(homeScroll);
			ScrollViewer scrollViewer = new ScrollViewer
			{
				VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
				Content = resultsList
			};
			SmoothScroll.Attach(scrollViewer);
			resultsPage.Children.Add(scrollViewer);
			resultsPage.Visibility = Visibility.Collapsed;
			Grid element2 = new Grid
			{
				Children = 
				{
					(UIElement)home,
					(UIElement)resultsPage
				}
			};
			Grid.SetRow(element2, 1);
			grid.Children.Add(element2);
			Grid grid3 = new Grid
			{
				Margin = new Thickness(0.0, 12.0, 0.0, 0.0)
			};
			Border element3 = new Border
			{
				Height = 1.0,
				Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(28, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(0.0, -6.0, 0.0, 0.0)
			};
			grid3.Children.Add(element3);
			StackPanel stackPanel2 = new StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Horizontal,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				Margin = new Thickness(0.0, 6.0, 0.0, 0.0)
			};
			stackPanel2.Children.Add(Avatar());
			StackPanel stackPanel3 = new StackPanel
			{
				Margin = new Thickness(10.0, 0.0, 0.0, 0.0),
				VerticalAlignment = VerticalAlignment.Center
			};
			greet = CC.T("", 12.0, FontWeights.Normal, 160);
			stackPanel3.Children.Add(greet);
			stackPanel3.Children.Add(CC.T(Environment.UserName, 14.0, FontWeights.SemiBold, byte.MaxValue));
			stackPanel2.Children.Add(stackPanel3);
			grid3.Children.Add(stackPanel2);
			powerRow.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			powerRow.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
			powerRow.Children.Add(PowerButton("\ue72e", "قفل", confirm: false, delegate
			{
				LockWorkStation();
			}));
			powerRow.Children.Add(PowerButton("\ue748", "خروج", confirm: true, delegate
			{
				Process.Start(new ProcessStartInfo("shutdown", "/l")
				{
					CreateNoWindow = true,
					UseShellExecute = false
				});
			}));
			powerRow.Children.Add(PowerButton("\ue708", "سكون", confirm: false, delegate
			{
				SetSuspendState(hibernate: false, force: false, disableWake: false);
			}));
			powerRow.Children.Add(PowerButton("\ue777", "ريستارت", confirm: true, delegate
			{
				Process.Start(new ProcessStartInfo("shutdown", "/r /t 0")
				{
					CreateNoWindow = true,
					UseShellExecute = false
				});
			}));
			powerRow.Children.Add(PowerButton("\ue7e8", "إيقاف", confirm: true, delegate
			{
				Process.Start(new ProcessStartInfo("shutdown", "/s /t 0")
				{
					CreateNoWindow = true,
					UseShellExecute = false
				});
			}));
			grid3.Children.Add(powerRow);
			Grid.SetRow(grid3, 2);
			grid.Children.Add(grid3);
			frame = new Border
			{
				Margin = new Thickness(24.0, 20.0, 24.0, 28.0),
				CornerRadius = new CornerRadius(26.0),
				Padding = new Thickness(18.0, 18.0, 18.0, 14.0),
				BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(46, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderThickness = new Thickness(1.0),
				Effect = new DropShadowEffect
				{
					BlurRadius = 34.0,
					ShadowDepth = 10.0,
					Direction = 270.0,
					Opacity = 0.55
				},
				Child = grid,
				RenderTransformOrigin = new System.Windows.Point(0.5, 1.0)
			};
			TransformGroup renderTransform = new TransformGroup
			{
				Children = 
				{
					(Transform)zoom,
					(Transform)slide
				}
			};
			frame.RenderTransform = renderTransform;
			base.Content = frame;
			ApplyColors();
			search.TextChanged += delegate
			{
				OnSearch();
			};
			base.PreviewKeyDown += OnKey;
			base.Deactivated += delegate
			{
				Hide2();
			};
			base.SourceInitialized += delegate
			{
				IntPtr handle = new WindowInteropHelper(this).Handle;
				Native.SetWindowLong(handle, -20, Native.GetWindowLong(handle, -20) | 0x80);
			};
			fileTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(140.0)
			};
			fileTimer.Tick += delegate
			{
				fileTimer.Stop();
				SearchFiles();
			};
			iconTimer = new DispatcherTimer(DispatcherPriority.Background)
			{
				Interval = TimeSpan.FromMilliseconds(15.0)
			};
			iconTimer.Tick += delegate
			{
				LoadSomeIcons();
			};
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				Reindex(prewarm: true);
			}, DispatcherPriority.ApplicationIdle);
		}

		private void ApplyColors()
		{
			System.Windows.Media.Color accentColor = cfg.AccentColor;
			System.Windows.Media.Color color = ColorUtil.Mix(System.Windows.Media.Color.FromRgb(36, 32, 46), accentColor, 0.1);
			System.Windows.Media.Color color2 = ColorUtil.Mix(System.Windows.Media.Color.FromRgb(20, 17, 28), accentColor, 0.07);
			frame.Background = new LinearGradientBrush(System.Windows.Media.Color.FromArgb(247, color.R, color.G, color.B), System.Windows.Media.Color.FromArgb(247, color2.R, color2.G, color2.B), 90.0);
		}

		private void Reindex(bool prewarm)
		{
			if (loading)
			{
				return;
			}
			loading = true;
			Thread thread = new Thread((ThreadStart)delegate
			{
				List<AppEntry> list = null;
				try
				{
					list = AppIndex.Enumerate();
					AppIndex.ScoreUsage(list);
				}
				catch
				{
				}
				base.Dispatcher.BeginInvoke((Action)delegate
				{
					loading = false;
					if (list != null && list.Count != 0)
					{
						Dictionary<string, AppEntry> dictionary = apps.ToDictionary((AppEntry x) => x.Id, (AppEntry x) => x, StringComparer.OrdinalIgnoreCase);
						foreach (AppEntry item in list)
						{
							if (dictionary.TryGetValue(item.Id, out var value) && value.IconLoaded)
							{
								item.Icon = value.Icon;
								item.IconLoaded = true;
							}
						}
						apps = list;
						indexedAt = DateTime.Now;
						BuildGrids();
						if (prewarm && !base.IsVisible)
						{
							base.ShowActivated = false;
							base.Left = -20000.0;
							base.Opacity = 0.0;
							Show();
							Hide();
							base.ShowActivated = true;
						}
					}
				});
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();
		}

		private void BuildGrids()
		{
			frequentRow.Children.Clear();
			foreach (AppEntry item in (from x in apps
				where x.Usage > 0.0
				orderby x.Usage descending
				select x).Take(7))
			{
				frequentRow.Children.Add(Tile(item, 52.0, 92.0));
			}
			if (frequentRow.Children.Count == 0)
			{
				frequentRow.Children.Add(Muted("البرامج اللي بتفتحها كتير هتظهر هنا"));
			}
			allGrid.Children.Clear();
			foreach (AppEntry app in apps)
			{
				allGrid.Children.Add(Tile(app, 40.0, 92.0));
			}
			iconQueue.Clear();
			foreach (AppEntry item2 in apps.OrderByDescending((AppEntry x) => x.Usage))
			{
				if (!item2.IconLoaded)
				{
					iconQueue.Enqueue(item2);
				}
			}
			iconTimer.Start();
		}

		private void LoadSomeIcons()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (iconQueue.Count > 0 && stopwatch.ElapsedMilliseconds < 10)
			{
				AppEntry appEntry = iconQueue.Dequeue();
				if (appEntry.IconLoaded)
				{
					continue;
				}
				appEntry.Icon = AppIndex.LoadIcon(appEntry);
				appEntry.IconLoaded = true;
				foreach (System.Windows.Controls.Image image in appEntry.Images)
				{
					image.Source = appEntry.Icon;
				}
			}
			if (iconQueue.Count == 0)
			{
				iconTimer.Stop();
			}
		}

		private static ImageSource IconOf(AppEntry a)
		{
			if (!a.IconLoaded)
			{
				a.Icon = AppIndex.LoadIcon(a);
				a.IconLoaded = true;
			}
			return a.Icon;
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
			ApplyColors();
			search.Text = "";
			ShowHome();
			homeScroll.ScrollToVerticalOffset(0.0);
			int hour = DateTime.Now.Hour;
			greet.Text = ((hour >= 5 && hour < 12) ? "صباح الخير" : ((hour < 17 && hour >= 12) ? "يومك سعيد" : "مساء الخير"));
			foreach (Border child in powerRow.Children)
			{
				ResetPower(child);
			}
			Mon mon = Screens.Primary();
			frame.LayoutTransform = Transform.Identity;
			frame.Measure(new System.Windows.Size(748.0, double.PositiveInfinity));
			double fit = Math.Min(1.0, Math.Min(mon.W / 748.0, (mon.H - 78.0) / frame.DesiredSize.Height));
			double height = frame.DesiredSize.Height * fit;
			frame.LayoutTransform = new ScaleTransform(fit, fit);
			base.Width = 748.0 * fit;
			base.Opacity = 0.0;
			Show();
			Screens.Center(new WindowInteropHelper(this).Handle, mon, 748.0 * fit, Math.Max(8.0, mon.H - height - 70.0));
			ForceForeground();
			search.Focus();
			Keyboard.Focus(search);
			HookMouse(on: true);
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(260.0);
			QuinticEase quinticEase = new QuinticEase();
			quinticEase.EasingMode = EasingMode.EaseOut;
			QuinticEase easingFunction = quinticEase;
			BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(150.0)));
			zoom.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.94, 1.0, timeSpan)
			{
				EasingFunction = easingFunction
			});
			zoom.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.94, 1.0, timeSpan)
			{
				EasingFunction = easingFunction
			});
			slide.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(18.0, 0.0, timeSpan)
			{
				EasingFunction = easingFunction
			});
			if ((DateTime.Now - indexedAt).TotalMinutes > 3.0)
			{
				Reindex(prewarm: false);
				return;
			}
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				AppIndex.ScoreUsage(apps);
				RefreshFrequent();
			}, DispatcherPriority.Background);
		}

		private void RefreshFrequent()
		{
			frequentRow.Children.Clear();
			foreach (AppEntry item in (from x in apps
				where x.Usage > 0.0
				orderby x.Usage descending
				select x).Take(7))
			{
				frequentRow.Children.Add(Tile(item, 52.0, 92.0));
			}
			if (frequentRow.Children.Count == 0)
			{
				frequentRow.Children.Add(Muted("البرامج اللي بتفتحها كتير هتظهر هنا"));
			}
		}

		public void Hide2()
		{
			if (!base.IsVisible || closing)
			{
				return;
			}
			closing = true;
			HookMouse(on: false);
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(140.0);
			DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, timeSpan);
			doubleAnimation.Completed += delegate
			{
				if (closing)
				{
					Hide();
					closing = false;
				}
			};
			BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
			zoom.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.96, timeSpan));
			zoom.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.96, timeSpan));
			slide.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(10.0, timeSpan));
		}

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
					IntPtr handle = new WindowInteropHelper(this).Handle;
					if (Native.GetWindowRect(handle, out var r))
					{
						double num4 = 1.0;
						PresentationSource presentationSource = PresentationSource.FromVisual(this);
						if (presentationSource != null && presentationSource.CompositionTarget != null)
						{
							num4 = presentationSource.CompositionTarget.TransformToDevice.M11;
						}
						int num5 = (int)(22.0 * num4);
						if ((num2 < r.L + num5 || num2 > r.R - num5 || num3 < r.T + num5 || num3 > r.B - num5) && !IsDockClick(num2, num3))
						{
							base.Dispatcher.BeginInvoke(new Action(Hide2));
						}
					}
				}
			}
			catch
			{
			}
			return Native.CallNextHookEx(mouseHook, code, wParam, lParam);
		}

		private bool IsDockClick(int x, int y)
		{
			IntPtr intPtr = Native.FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "MacStyle Dock");
			if (intPtr != IntPtr.Zero && Native.GetWindowRect(intPtr, out var r) && x >= r.L && x <= r.R && y >= r.T)
			{
				return y <= r.B;
			}
			return false;
		}

		private void OnKey(object s, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				if (search.Text != "")
				{
					search.Text = "";
				}
				else
				{
					Hide2();
				}
				e.Handled = true;
				return;
			}
			if (resultsPage.Visibility == Visibility.Visible && results.Count > 0)
			{
				if (e.Key == Key.Down)
				{
					Select(Math.Min(results.Count - 1, selected + 1));
					e.Handled = true;
					return;
				}
				if (e.Key == Key.Up)
				{
					Select(Math.Max(0, selected - 1));
					e.Handled = true;
					return;
				}
				if (e.Key == Key.Return)
				{
					RunResult(results[selected]);
					e.Handled = true;
					return;
				}
			}
			else if (e.Key == Key.Return && frequentRow.Children.Count > 0)
			{
				e.Handled = true;
			}
			if (!search.IsKeyboardFocused)
			{
				search.Focus();
			}
		}

		private void ShowHome()
		{
			resultsPage.Visibility = Visibility.Collapsed;
			home.Visibility = Visibility.Visible;
			hint.Visibility = Visibility.Visible;
		}

		private void OnSearch()
		{
			string text = search.Text.Trim();
			hint.Visibility = ((search.Text.Length != 0) ? Visibility.Collapsed : Visibility.Visible);
			System.Windows.FlowDirection flowDirection = ((search.Text.Any((char ch) => ch >= '\u0600' && ch <= 'ۿ') || search.Text.Length == 0) ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight);
			if (search.FlowDirection != flowDirection)
			{
				search.FlowDirection = flowDirection;
				search.TextAlignment = ((flowDirection == System.Windows.FlowDirection.LeftToRight) ? TextAlignment.Right : TextAlignment.Left);
			}
			searchToken++;
			fileTimer.Stop();
			if (text.Length == 0)
			{
				ShowHome();
				return;
			}
			home.Visibility = Visibility.Collapsed;
			resultsPage.Visibility = Visibility.Visible;
			results = new List<ResultItem>();
			string text2 = QuickSearch.Calc(text);
			if (text2 != null)
			{
				string c = text2;
				results.Add(new ResultItem
				{
					Title = "= " + c,
					Sub = "اضغط Enter عشان تنسخ الناتج",
					Glyph = "\ue8ef",
					Kind = "calc",
					Run = delegate
					{
						try
						{
							System.Windows.Clipboard.SetText(c);
						}
						catch
						{
						}
					}
				});
			}
			foreach (AppEntry item in RankApps(text).Take(7))
			{
				AppEntry app = item;
				results.Add(new ResultItem
				{
					Title = item.Name,
					Sub = "برنامج",
					Icon = IconOf(item),
					Kind = "app",
					Run = delegate
					{
						AppIndex.Launch(app);
					}
				});
			}
			results.AddRange(QuickSearch.SettingsFor(text));
			string web = text;
			results.Add(new ResultItem
			{
				Title = "دو\u0651ر على \"" + text + "\" في جوجل",
				Sub = "المتصفح",
				Glyph = "\ue774",
				Kind = "web",
				Run = delegate
				{
					QuickSearch.Open("https://www.google.com/search?q=" + Uri.EscapeDataString(web));
				}
			});
			selected = 0;
			Render();
			if (text.Length >= 2)
			{
				fileTimer.Start();
			}
		}

		private static string FromArabicLayout(string q)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = q.Replace("لا", "b");
			foreach (char c in text)
			{
				int num = "ضصثقفغعهخحجدشسيبلاتنمكطئءؤرىةوزظ".IndexOf(c);
				if (num >= 0)
				{
					stringBuilder.Append("qwertyuiop[]asdfghjkl;'zxcvnm,./"[num]);
				}
				else if (c == 'أ' || c == 'إ' || c == 'آ')
				{
					stringBuilder.Append('h');
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		private IEnumerable<AppEntry> RankApps(string q)
		{
			List<KeyValuePair<string, double>> list = new List<KeyValuePair<string, double>>();
			list.Add(new KeyValuePair<string, double>(q.ToLowerInvariant(), 0.0));
			List<KeyValuePair<string, double>> list2 = list;
			if (q.Any((char ch) => ch >= '\u0600' && ch <= 'ۿ'))
			{
				string[][] aliases = Aliases;
				foreach (string[] array in aliases)
				{
					if (array[0].StartsWith(q) || q.StartsWith(array[0]))
					{
						list2.Add(new KeyValuePair<string, double>(array[1], -2.0));
					}
				}
				if (q.Length >= 2)
				{
					list2.Add(new KeyValuePair<string, double>(FromArabicLayout(q), -8.0));
				}
			}
			List<KeyValuePair<double, AppEntry>> list3 = new List<KeyValuePair<double, AppEntry>>();
			foreach (AppEntry app in apps)
			{
				double num2 = 0.0;
				string lower = app.Lower;
				foreach (KeyValuePair<string, double> item in list2)
				{
					string lq = item.Key;
					if (lq.Length != 0)
					{
						double num3 = 0.0;
						if (lower.StartsWith(lq))
						{
							num3 = 100.0;
						}
						else if (lower.Split(' ', '-', '.', '_').Any((string w) => w.StartsWith(lq)))
						{
							num3 = 80.0;
						}
						else if (Initials(lower).StartsWith(lq) && lq.Length >= 2)
						{
							num3 = 70.0;
						}
						else if (lower.Contains(lq))
						{
							num3 = 55.0;
						}
						if (num3 > 0.0)
						{
							num2 = Math.Max(num2, num3 + item.Value);
						}
					}
				}
				if (!(num2 <= 0.0))
				{
					num2 += Math.Min(25.0, Math.Log(1.0 + app.Usage) * 4.0);
					list3.Add(new KeyValuePair<double, AppEntry>(num2, app));
				}
			}
			return from k in list3
				orderby k.Key descending, k.Value.Name.Length
				select k.Value;
		}

		private static string Initials(string n)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = n.Split(new char[4] { ' ', '-', '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				stringBuilder.Append(text[0]);
			}
			return stringBuilder.ToString();
		}

		private static bool Subsequence(string n, string q)
		{
			int num = 0;
			foreach (char c in n)
			{
				if (num < q.Length && c == q[num])
				{
					num++;
				}
			}
			return num == q.Length;
		}

		private void SearchFiles()
		{
			string q = search.Text.Trim();
			int token = searchToken;
			Task.Run(() => QuickSearch.Files(q)).ContinueWith((Task<List<string>> t) => base.Dispatcher.BeginInvoke((Action)delegate
			{
				if (token == searchToken && t.Result.Count != 0)
				{
					int num = results.FindIndex((ResultItem r) => r.Kind == "web");
					if (num < 0)
					{
						num = results.Count;
					}
					List<ResultItem> collection = t.Result.Select((string p) => new ResultItem
					{
						Title = System.IO.Path.GetFileName(p),
						Sub = System.IO.Path.GetDirectoryName(p),
						LtrSub = true,
						Icon = ShellApi.Icon(p, 48, useCache: true),
						Kind = "file",
						Run = delegate
						{
							QuickSearch.Open(p);
						}
					}).ToList();
					results.InsertRange(num, collection);
					Render();
				}
			}));
		}

		private void Render()
		{
			resultsList.Children.Clear();
			string text = null;
			for (int i = 0; i < results.Count; i++)
			{
				ResultItem resultItem = results[i];
				string text2 = ((resultItem.Kind == "app") ? "البرامج" : ((resultItem.Kind == "file") ? "الملفات" : ((resultItem.Kind == "setting") ? "الإعدادات" : null)));
				if (text2 != null && text2 != text)
				{
					TextBlock textBlock = SectionTitle(text2);
					textBlock.Margin = new Thickness(6.0, (i != 0) ? 8 : 0, 0.0, 4.0);
					resultsList.Children.Add(textBlock);
				}
				text = text2;
				resultsList.Children.Add(ResultRow(resultItem, i));
			}
			Select(selected);
		}

		private void Select(int i)
		{
			selected = i;
			foreach (object child in resultsList.Children)
			{
				if (child is Border border && border.Tag is int)
				{
					border.Background = (((int)border.Tag == i) ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(70, UI.Accent.Color.R, UI.Accent.Color.G, UI.Accent.Color.B)) : System.Windows.Media.Brushes.Transparent);
					if ((int)border.Tag == i)
					{
						border.BringIntoView();
					}
				}
			}
		}

		private void RunResult(ResultItem r)
		{
			Hide2();
			try
			{
				r.Run();
			}
			catch (Exception ex)
			{
				Sys.Log("run: " + ex.Message);
			}
		}

		private Border ResultRow(ResultItem r, int index)
		{
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(44.0)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			FrameworkElement element;
			if (r.Icon != null)
			{
				System.Windows.Controls.Image image = new System.Windows.Controls.Image();
				image.Source = r.Icon;
				image.Width = 30.0;
				image.Height = 30.0;
				System.Windows.Controls.Image image2 = image;
				RenderOptions.SetBitmapScalingMode(image2, BitmapScalingMode.HighQuality);
				element = image2;
			}
			else
			{
				Border border = new Border();
				border.Width = 30.0;
				border.Height = 30.0;
				border.CornerRadius = new CornerRadius(9.0);
				border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				border.Child = CC.G(r.Glyph ?? "\ue721", 14.0, System.Windows.Media.Brushes.White);
				element = border;
			}
			grid.Children.Add(element);
			StackPanel stackPanel = new StackPanel();
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			StackPanel stackPanel2 = stackPanel;
			TextBlock element2 = CC.T(r.Title, 14.0, FontWeights.SemiBold, byte.MaxValue);
			stackPanel2.Children.Add(element2);
			TextBlock textBlock = CC.T(r.Sub, 11.5, FontWeights.Normal, 150);
			if (r.LtrSub)
			{
				textBlock.FlowDirection = System.Windows.FlowDirection.LeftToRight;
				textBlock.TextAlignment = TextAlignment.Right;
				textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
			}
			stackPanel2.Children.Add(textBlock);
			Grid.SetColumn(stackPanel2, 1);
			grid.Children.Add(stackPanel2);
			Border border2 = new Border();
			border2.Child = grid;
			border2.CornerRadius = new CornerRadius(12.0);
			border2.Padding = new Thickness(8.0, 6.0, 10.0, 6.0);
			border2.Margin = new Thickness(0.0, 1.0, 0.0, 1.0);
			border2.Tag = index;
			border2.Cursor = System.Windows.Input.Cursors.Hand;
			border2.Background = System.Windows.Media.Brushes.Transparent;
			Border border3 = border2;
			border3.MouseEnter += delegate
			{
				Select(index);
			};
			border3.MouseLeftButtonUp += delegate
			{
				RunResult(r);
			};
			return border3;
		}

		private FrameworkElement Tile(AppEntry a, double icon, double w)
		{
			Border bg = new Border
			{
				Width = w,
				CornerRadius = new CornerRadius(16.0),
				Padding = new Thickness(4.0, 9.0, 4.0, 7.0),
				Margin = new Thickness(1.0, 2.0, 1.0, 2.0),
				Background = System.Windows.Media.Brushes.Transparent,
				Cursor = System.Windows.Input.Cursors.Hand
			};
			StackPanel stackPanel = new StackPanel();
			System.Windows.Controls.Image image = new System.Windows.Controls.Image();
			image.Width = icon;
			image.Height = icon;
			image.Source = a.Icon;
			image.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			System.Windows.Controls.Image image2 = image;
			RenderOptions.SetBitmapScalingMode(image2, BitmapScalingMode.HighQuality);
			a.Images.Add(image2);
			stackPanel.Children.Add(image2);
			TextBlock textBlock = CC.T(a.Name, 11.5, FontWeights.Normal, 225);
			textBlock.TextAlignment = TextAlignment.Center;
			textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock.MaxWidth = w - 10.0;
			textBlock.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
			stackPanel.Children.Add(textBlock);
			bg.Child = stackPanel;
			SolidColorBrush hover = new SolidColorBrush(System.Windows.Media.Color.FromArgb(26, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			bg.MouseEnter += delegate
			{
				bg.Background = hover;
			};
			bg.MouseLeave += delegate
			{
				bg.Background = System.Windows.Media.Brushes.Transparent;
				bg.RenderTransform = null;
			};
			bg.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
			{
				bg.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
				bg.RenderTransform = new ScaleTransform(0.93, 0.93);
				e.Handled = true;
			};
			bg.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				if (bg.RenderTransform != null)
				{
					bg.RenderTransform = null;
					Hide2();
					AppIndex.Launch(a);
					e.Handled = true;
				}
			};
			return bg;
		}

		private FrameworkElement Shortcuts()
		{
			WrapPanel wp = new WrapPanel
			{
				Margin = new Thickness(0.0, 0.0, 0.0, 6.0)
			};
			Action<string, string, string> action = delegate(string glyph, string label, string target)
			{
				StackPanel stackPanel = new StackPanel
				{
					Orientation = System.Windows.Controls.Orientation.Horizontal
				};
				stackPanel.Children.Add(CC.G(glyph, 13.0, System.Windows.Media.Brushes.White));
				TextBlock textBlock = CC.T(label, 12.0, FontWeights.Medium, 230);
				textBlock.Margin = new Thickness(7.0, 0.0, 0.0, 0.0);
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				stackPanel.Children.Add(textBlock);
				SolidColorBrush normal = new SolidColorBrush(System.Windows.Media.Color.FromArgb(22, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				Border b = new Border
				{
					Child = stackPanel,
					CornerRadius = new CornerRadius(12.0),
					Padding = new Thickness(12.0, 7.0, 12.0, 8.0),
					Margin = new Thickness(0.0, 0.0, 7.0, 7.0),
					Background = normal,
					Cursor = System.Windows.Input.Cursors.Hand
				};
				b.MouseEnter += delegate
				{
					b.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(44, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				};
				b.MouseLeave += delegate
				{
					b.Background = normal;
				};
				b.MouseLeftButtonUp += delegate
				{
					Hide2();
					QuickSearch.Open(target);
				};
				wp.Children.Add(b);
			};
			action("\ue713", "الإعدادات", "ms-settings:");
			action("\uec50", "الملفات", "shell:MyComputerFolder");
			action("\ue896", "التنزيلات", "shell:Downloads");
			action("\ue9d9", "مدير المهام", "taskmgr");
			action("\ue80f", "لوحة التحكم", "control");
			action("\ue722", "لقطة شاشة", "ms-screenclip:");
			return wp;
		}

		private Border PowerButton(string glyph, string label, bool confirm, Action act)
		{
			TextBlock element = CC.G(glyph, 14.0, System.Windows.Media.Brushes.White);
			TextBlock t = CC.T(label, 11.5, FontWeights.Medium, 230);
			t.Margin = new Thickness(6.0, 0.0, 0.0, 0.0);
			t.VerticalAlignment = VerticalAlignment.Center;
			t.Visibility = Visibility.Collapsed;
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
			stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			StackPanel stackPanel2 = stackPanel;
			stackPanel2.Children.Add(element);
			stackPanel2.Children.Add(t);
			SolidColorBrush normal = new SolidColorBrush(System.Windows.Media.Color.FromArgb(24, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Border b = new Border
			{
				Child = stackPanel2,
				Height = 36.0,
				MinWidth = 36.0,
				CornerRadius = new CornerRadius(18.0),
				Padding = new Thickness(11.0, 0.0, 11.0, 0.0),
				Margin = new Thickness(6.0, 0.0, 0.0, 0.0),
				Background = normal,
				Cursor = System.Windows.Input.Cursors.Hand,
				ToolTip = label,
				Tag = label
			};
			bool armed = false;
			DispatcherTimer disarm = null;
			b.MouseEnter += delegate
			{
				if (!armed)
				{
					t.Text = label;
					t.Visibility = Visibility.Visible;
					b.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(46, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				}
			};
			b.MouseLeave += delegate
			{
				if (!armed)
				{
					t.Visibility = Visibility.Collapsed;
					b.Background = normal;
				}
			};
			b.MouseLeftButtonUp += delegate
			{
				if (confirm && !armed)
				{
					armed = true;
					t.Text = label + "؟ اضغط تاني";
					t.Visibility = Visibility.Visible;
					b.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 220, 60, 60));
					disarm = new DispatcherTimer
					{
						Interval = TimeSpan.FromSeconds(3.0)
					};
					disarm.Tick += delegate
					{
						disarm.Stop();
						armed = false;
						t.Visibility = Visibility.Collapsed;
						b.Background = normal;
					};
					disarm.Start();
				}
				else
				{
					if (disarm != null)
					{
						disarm.Stop();
					}
					armed = false;
					Hide2();
					DispatcherTimer delay = new DispatcherTimer
					{
						Interval = TimeSpan.FromMilliseconds(250.0)
					};
					delay.Tick += delegate
					{
						delay.Stop();
						try
						{
							act();
						}
						catch (Exception ex)
						{
							Sys.Log("power: " + ex.Message);
						}
					};
					delay.Start();
				}
			};
			return b;
		}

		private static void ResetPower(Border b)
		{
			b.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(24, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			if (b.Child is StackPanel stackPanel && stackPanel.Children.Count > 1)
			{
				stackPanel.Children[1].Visibility = Visibility.Collapsed;
			}
		}

		private FrameworkElement Avatar()
		{
			Grid grid = new Grid();
			grid.Width = 38.0;
			grid.Height = 38.0;
			Grid grid2 = grid;
			ImageSource imageSource = null;
			try
			{
				string value = WindowsIdentity.GetCurrent().User.Value;
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AccountPicture\\Users\\" + value))
				{
					string text = ((registryKey == null) ? null : ((registryKey.GetValue("Image96") ?? registryKey.GetValue("Image192")) as string));
					if (!string.IsNullOrEmpty(text) && File.Exists(text))
					{
						BitmapImage bitmapImage = new BitmapImage();
						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.UriSource = new Uri(text);
						bitmapImage.DecodePixelWidth = 96;
						bitmapImage.EndInit();
						bitmapImage.Freeze();
						imageSource = bitmapImage;
					}
				}
			}
			catch
			{
			}
			if (imageSource != null)
			{
				grid2.Children.Add(new Ellipse
				{
					Fill = new ImageBrush(imageSource)
					{
						Stretch = Stretch.UniformToFill
					}
				});
			}
			else
			{
				grid2.Children.Add(new Ellipse
				{
					Fill = ColorUtil.Gradient(cfg.AccentColor, 45.0)
				});
				string userName = Environment.UserName;
				TextBlock textBlock = CC.T(string.IsNullOrEmpty(userName) ? "?" : userName.Substring(0, 1).ToUpperInvariant(), 16.0, FontWeights.SemiBold, byte.MaxValue);
				textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				textBlock.TextAlignment = TextAlignment.Center;
				grid2.Children.Add(textBlock);
			}
			return grid2;
		}

		private static TextBlock SectionTitle(string s)
		{
			TextBlock textBlock = CC.T(s, 12.5, FontWeights.SemiBold, 200);
			textBlock.Margin = new Thickness(6.0, 6.0, 0.0, 6.0);
			return textBlock;
		}

		private static TextBlock Muted(string s)
		{
			TextBlock textBlock = CC.T(s, 12.0, FontWeights.Normal, 130);
			textBlock.Margin = new Thickness(8.0, 10.0, 0.0, 14.0);
			return textBlock;
		}
	}
}
