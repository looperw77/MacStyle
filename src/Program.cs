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

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: CompilationRelaxations(8)]
[assembly: AssemblyTitle("MacStyle")]
[assembly: AssemblyProduct("MacStyle")]
[assembly: AssemblyCompany("Ahmed Kamel")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Ahmed Kamel")]
[assembly: AssemblyVersion("2.0.0.0")]

namespace MacStyle
{
	internal static class Uninstaller
	{
		public static void Run()
		{
			MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("متأكد إنك عايز تشيل MacStyle؟", "MacStyle", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, System.Windows.MessageBoxOptions.RightAlign | System.Windows.MessageBoxOptions.RtlReading);
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
			bool flag = false;
			if (Seelen.Installed)
			{
				flag = System.Windows.MessageBox.Show("تشيل كمان برنامج Seelen UI؟ (MacStyle مبقاش محتاجه، الدوك بقى جواه)", "MacStyle", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, System.Windows.MessageBoxOptions.RightAlign | System.Windows.MessageBoxOptions.RtlReading) == MessageBoxResult.Yes;
			}
			Program.QuitRunning();
			Taskbar.Restore();
			try
			{
				Dwm.WindowsMinAnimation = true;
			}
			catch
			{
			}
			Program.RemoveAutostart();
			try
			{
				Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MacStyle", throwOnMissingSubKey: false);
			}
			catch
			{
			}
			try
			{
				string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "MacStyle");
				if (Directory.Exists(path))
				{
					Directory.Delete(path, recursive: true);
				}
			}
			catch
			{
			}
			if (Seelen.Installed)
			{
				Sys.Kill("seelen-ui");
				Thread.Sleep(600);
				Seelen.RemoveTheme();
				if (flag)
				{
					Sys.Run("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"Get-AppxPackage Seelen.SeelenUI | Remove-AppxPackage\"", 180000);
				}
				else
				{
					Seelen.Start();
				}
			}
			try
			{
				if (Directory.Exists(Brand.DataDir))
				{
					Directory.Delete(Brand.DataDir, recursive: true);
				}
			}
			catch
			{
			}
			System.Windows.MessageBox.Show("اتشال MacStyle. شكرا\u064b إنك جربته \ud83d\udc9c", "MacStyle", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, System.Windows.MessageBoxOptions.RightAlign | System.Windows.MessageBoxOptions.RtlReading);
			string directoryName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			try
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/c timeout /t 2 /nobreak >nul & rmdir /s /q \"" + directoryName + "\"");
				processStartInfo.CreateNoWindow = true;
				processStartInfo.UseShellExecute = false;
				Process.Start(processStartInfo);
			}
			catch
			{
			}
		}
	}
	internal static class Program
	{
		private static readonly Stopwatch bootClock = Stopwatch.StartNew();

		public static void QuitRunning()
		{
			try
			{
				if (EventWaitHandle.TryOpenExisting("MacStyle_Quit", out var result))
				{
					result.Set();
					result.Dispose();
					Stopwatch stopwatch = Stopwatch.StartNew();
					while (stopwatch.ElapsedMilliseconds < 3000)
					{
						bool flag = false;
						Process[] processesByName = Process.GetProcessesByName("MacStyle");
						foreach (Process process in processesByName)
						{
							if (process.Id != Process.GetCurrentProcess().Id)
							{
								flag = true;
							}
						}
						if (flag)
						{
							Thread.Sleep(100);
							continue;
						}
						break;
					}
				}
			}
			catch
			{
			}
			Sys.Kill("MacStyle");
		}

		public static void RegisterAutostart()
		{
			string location = Assembly.GetExecutingAssembly().Location;
			string str = Environment.UserDomainName + "\\" + Environment.UserName;
			string contents = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\"><RegistrationInfo><Author>" + SecurityElement.Escape("Ahmed Kamel") + "</Author><Description>MacStyle (dock, Dynamic Island)</Description></RegistrationInfo><Triggers><LogonTrigger><Enabled>true</Enabled><UserId>" + SecurityElement.Escape(str) + "</UserId></LogonTrigger></Triggers><Principals><Principal id=\"Author\"><UserId>" + SecurityElement.Escape(str) + "</UserId><LogonType>InteractiveToken</LogonType><RunLevel>HighestAvailable</RunLevel></Principal></Principals><Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><AllowHardTerminate>true</AllowHardTerminate><StartWhenAvailable>true</StartWhenAvailable><IdleSettings><StopOnIdleEnd>false</StopOnIdleEnd><RestartOnIdle>false</RestartOnIdle></IdleSettings><AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled><Hidden>false</Hidden><ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Priority>4</Priority></Settings><Actions Context=\"Author\"><Exec><Command>" + SecurityElement.Escape(location) + "</Command></Exec></Actions></Task>";
			bool flag = false;
			try
			{
				string text = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "macstyle-task.xml");
				File.WriteAllText(text, contents, Encoding.Unicode);
				flag = Sys.Run("schtasks.exe", "/Create /F /TN \"MacStyle\" /XML \"" + text + "\"", 20000) == 0;
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
			catch
			{
			}
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run"))
				{
					if (flag)
					{
						if (registryKey.GetValue("MacStyle") != null)
						{
							registryKey.DeleteValue("MacStyle");
						}
					}
					else
					{
						registryKey.SetValue("MacStyle", "\"" + location + "\"");
					}
				}
			}
			catch
			{
			}
			Sys.Log("autostart: " + (flag ? "task" : "run key"));
		}

		public static void RemoveAutostart()
		{
			Sys.Run("schtasks.exe", "/Delete /F /TN \"MacStyle\"", 20000);
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true))
				{
					if (registryKey != null && registryKey.GetValue("MacStyle") != null)
					{
						registryKey.DeleteValue("MacStyle");
					}
				}
			}
			catch
			{
			}
		}

		[STAThread]
		private static int Main(string[] args)
		{
			bootClock.Start();
			// The exe carries no TargetFramework attribute, so WPF keeps per-monitor DPI off unless told otherwise.
			AppContext.SetSwitch("Switch.System.Windows.DoNotScaleForDpiChanges", false);
			AppContext.SetSwitch("Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier2OrGreater", false);
			string a = ((args.Length > 0) ? args[0].ToLowerInvariant() : "");
			List<string> list = new List<string>(args);
			if (a == "--setup")
			{
				Config config = Config.Load();
				foreach (string item in list)
				{
					if (item.StartsWith("accent="))
					{
						config.Accent = item.Substring(7).ToUpperInvariant();
					}
					if (item == "noisland")
					{
						config.IslandEnabled = false;
					}
					if (item == "nopaste")
					{
						config.PasteEnabled = false;
					}
					if (item == "nodock")
					{
						config.DockLine = false;
					}
				}
				config.DockEnabled = !list.Contains("nodock");
				config.Save();
				if (config.DockEnabled && Seelen.Installed && Seelen.WegEnabled())
				{
					bool flag = Sys.IsRunning("seelen-ui");
					Sys.Kill("seelen-ui");
					Thread.Sleep(700);
					Seelen.DisableDock();
					if (flag)
					{
						Seelen.Start();
					}
				}
				if (list.Contains("files"))
				{
					FilesApp.Apply(config);
				}
				return 0;
			}
			if (a == "--quit")
			{
				QuitRunning();
				return 0;
			}
			if (a == "--autostart")
			{
				RegisterAutostart();
				return 0;
			}
			if (a == "--uninstall")
			{
				new System.Windows.Application();
				Uninstaller.Run();
				return 0;
			}
			bool createdNew;
			Mutex obj = new Mutex(initiallyOwned: true, "MacStyle_SingleInstance_v1", out createdNew);
			if (!createdNew)
			{
				if (EventWaitHandle.TryOpenExisting("MacStyle_OpenSettings", out var result))
				{
					result.Set();
					result.Dispose();
				}
				return 0;
			}
			System.Windows.Application app = new System.Windows.Application
			{
				ShutdownMode = ShutdownMode.OnExplicitShutdown
			};
			Config cfg = Config.Load();
			UI.Accent.Color = cfg.AccentColor;
			Island island = new Island(cfg);
			SettingsWindow settings = null;
			Action openSettings = delegate
			{
				if (settings != null)
				{
					settings.Activate();
				}
				else
				{
					settings = new SettingsWindow(cfg, island);
					settings.Closed += delegate
					{
						settings = null;
					};
					settings.Show();
					settings.Activate();
				}
			};
			island.OpenSettingsRequested += openSettings;
			PasteTool paste = new PasteTool(cfg, delegate(string name)
			{
				island.Dispatcher.BeginInvoke((Action)delegate
				{
					island.ShowToast("اتحفظت الصورة", name);
				});
			});
			Thread thread = new Thread((ThreadStart)delegate
			{
				using (EventWaitHandle eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "MacStyle_OpenSettings"))
				{
					while (true)
					{
						eventWaitHandle.WaitOne();
						app.Dispatcher.BeginInvoke(openSettings);
					}
				}
			});
			thread.IsBackground = true;
			thread.Start();
			Thread thread2 = new Thread((ThreadStart)delegate
			{
				using (EventWaitHandle eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "MacStyle_Quit"))
				{
					eventWaitHandle.WaitOne();
					app.Dispatcher.BeginInvoke((Action)delegate
					{
						app.Shutdown();
					});
				}
			});
			thread2.IsBackground = true;
			thread2.Start();
			app.DispatcherUnhandledException += delegate(object s, DispatcherUnhandledExceptionEventArgs e)
			{
				Sys.Log("ui error: " + e.Exception);
				e.Handled = true;
			};
			AppDomain.CurrentDomain.UnhandledException += delegate
			{
				try
				{
					Taskbar.Restore();
				}
				catch
				{
				}
			};
			Dock dock = null;
			KeyHook keys = null;
			app.Startup += delegate
			{
				dock = new Dock(cfg);
				dock.OpenSettingsRequested += openSettings;
				dock.Show();
				try
				{
					Audio audio = new Audio(app.Dispatcher);
					island.Audio = audio;
					ControlCenter cc = new ControlCenter(cfg, audio);
					cc.OpenSettingsRequested += openSettings;
					island.OpenControlCenterRequested += delegate
					{
						cc.Toggle();
					};
					audio.Changed += delegate(float lv, bool mu, bool fromUi)
					{
						if (!fromUi)
						{
							island.ShowVolume(lv, mu);
						}
					};
					keys = new KeyHook(cfg, audio, app.Dispatcher);
					keys.ControlCenterKey += delegate
					{
						cc.Toggle();
					};
					Launcher launcher = new Launcher(cfg);
					dock.LauncherRequested += delegate
					{
						launcher.Toggle();
					};
					keys.LauncherKey += delegate
					{
						launcher.Toggle();
					};
				}
				catch (Exception ex)
				{
					Sys.Log("control center: " + ex);
				}
				island.Show();
				Notifier notifier = new Notifier(cfg);
				notifier.Arrived += island.ShowNotice;
				notifier.Start();
				app.Dispatcher.BeginInvoke((Action)delegate
				{
					Sys.Log("ready in " + bootClock.ElapsedMilliseconds + "ms (windows up " + Environment.TickCount / 1000 + "s)");
				}, DispatcherPriority.ApplicationIdle);
				if (a == "--settings")
				{
					openSettings();
				}
				if (cfg.DockEnabled && Seelen.Installed && Seelen.WegEnabled())
				{
					Task.Run(delegate
					{
						bool flag2 = Sys.IsRunning("seelen-ui");
						Sys.Kill("seelen-ui");
						Thread.Sleep(700);
						Seelen.DisableDock();
						if (flag2)
						{
							Seelen.Start();
						}
					});
				}
			};
			Genie genie = null;
			app.Startup += delegate
			{
				genie = new Genie(cfg);
			};
			app.Exit += delegate
			{
				paste.Dispose();
				if (genie != null)
				{
					genie.Dispose();
				}
				if (dock != null)
				{
					dock.Shutdown();
				}
				if (keys != null)
				{
					keys.Dispose();
				}
			};
			app.Run();
			GC.KeepAlive(obj);
			return 0;
		}
	}
}
