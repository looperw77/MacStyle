using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("MacStyle Update")]
[assembly: AssemblyProduct("MacStyle")]
[assembly: AssemblyCompany("Ahmed Kamel")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Ahmed Kamel")]
[assembly: AssemblyVersion("2.0.0.0")]

// One file to send to people who already have MacStyle: replaces the installed exe, leaves their settings alone.
internal static class Updater
{
	private const string Key = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\MacStyle";

	// Process.MainModule is denied for an elevated MacStyle; this is not.
	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern bool QueryFullProcessImageName(IntPtr h, int flags, StringBuilder sb, ref int size);

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(IntPtr h);

	// args: the install folder, passed only when it restarts itself as administrator.
	[STAThread]
	private static int Main(string[] args)
	{
		Application.EnableVisualStyles();
		string dir = (args.Length > 0) ? args[0] : InstallDir();
		if (dir == null)
		{
			Say("MacStyle مش متثبّت على الجهاز ده، فمفيش حاجة تتحدّث.", MessageBoxIcon.Information);
			return 1;
		}
		string exe = Path.Combine(dir, "MacStyle.exe");
		string next = Path.Combine(dir, "MacStyle.new.exe");
		string old = Path.Combine(dir, "MacStyle.old.exe");
		bool stopped = false;
		try
		{
			using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MacStyle.exe"))
			using (FileStream f = File.Create(next))
			{
				s.CopyTo(f);
			}
			Version mine = VersionOf(next);
			if (VersionOf(exe) >= mine)
			{
				File.Delete(next);
				Say("عندك آخر إصدار بالفعل (v" + mine.ToString(2) + ").", MessageBoxIcon.Information);
				return 2;
			}
			if (!Stop(exe))
			{
				File.Delete(next);
				if (args.Length > 0)
				{
					throw new Exception("MacStyle شغّال ومش راضي يقفل.");
				}
				// MacStyle itself runs as administrator here, so only an administrator can close it.
				using (Process p = Process.Start(new ProcessStartInfo(Assembly.GetExecutingAssembly().Location, "\"" + dir + "\"")
				{
					Verb = "runas",
					UseShellExecute = true
				}))
				{
					p.WaitForExit();
					if (p.ExitCode == 0)
					{
						SetVersion(mine);
					}
					return p.ExitCode;
				}
			}
			stopped = true;
			Swap(next, exe, old);
			SetVersion(mine);
			Process.Start(new ProcessStartInfo(exe)
			{
				UseShellExecute = true,
				WorkingDirectory = dir
			});
			Say("MacStyle اتحدّث لـ v" + mine.ToString(2) + " وشغّال دلوقتي. إعداداتك زي ما هي.", MessageBoxIcon.Information);
			return 0;
		}
		catch (Exception ex)
		{
			try
			{
				File.Delete(next);
				if (stopped && File.Exists(exe))
				{
					Process.Start(exe);
				}
			}
			catch
			{
			}
			Say("التحديث مكملش، والنسخة القديمة زي ما هي.\n" + ex.Message, MessageBoxIcon.Warning);
			return 3;
		}
	}

	private static string InstallDir()
	{
		string found = null;
		using (RegistryKey k = Registry.CurrentUser.OpenSubKey(Key))
		{
			if (k != null)
			{
				found = k.GetValue("InstallLocation") as string;
			}
		}
		string usual = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\MacStyle");
		foreach (string d in new[] { found, usual })
		{
			if (!string.IsNullOrWhiteSpace(d) && File.Exists(Path.Combine(d, "MacStyle.exe")))
			{
				return d.TrimEnd('\\');
			}
		}
		return null;
	}

	private static Version VersionOf(string file)
	{
		FileVersionInfo v = FileVersionInfo.GetVersionInfo(file);
		return new Version(v.FileMajorPart, v.FileMinorPart, v.FileBuildPart, v.FilePrivatePart);
	}

	private static string PathOf(int pid)
	{
		IntPtr h = OpenProcess(0x1000, false, pid);
		if (h == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			StringBuilder sb = new StringBuilder(1024);
			int size = sb.Capacity;
			return QueryFullProcessImageName(h, 0, sb, ref size) ? sb.ToString() : null;
		}
		finally
		{
			CloseHandle(h);
		}
	}

	private static bool Running(string exe, bool kill)
	{
		bool any = false;
		foreach (Process p in Process.GetProcessesByName("MacStyle"))
		{
			using (p)
			{
				if (!string.Equals(PathOf(p.Id), exe, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				any = true;
				if (kill)
				{
					try
					{
						p.Kill();
						p.WaitForExit(4000);
					}
					catch
					{
					}
				}
			}
		}
		return any;
	}

	private static bool Stop(string exe)
	{
		try
		{
			EventWaitHandle quit;
			if (EventWaitHandle.TryOpenExisting("MacStyle_Quit", out quit))
			{
				quit.Set();
				quit.Dispose();
			}
		}
		catch
		{
		}
		for (int i = 0; i < 60 && Running(exe, false); i++)
		{
			Thread.Sleep(100);
		}
		if (Running(exe, true))
		{
			Thread.Sleep(300);
		}
		return !Running(exe, false);
	}

	private static void Swap(string next, string exe, string old)
	{
		for (int i = 0; ; i++)
		{
			try
			{
				File.Delete(old);
				File.Replace(next, exe, old);
				return;
			}
			catch (IOException)
			{
				// Antivirus can hold the fresh file for a moment.
				if (i == 10)
				{
					throw;
				}
				Thread.Sleep(300);
			}
		}
	}

	private static void SetVersion(Version v)
	{
		using (RegistryKey k = Registry.CurrentUser.OpenSubKey(Key, true))
		{
			if (k != null)
			{
				k.SetValue("DisplayVersion", v.ToString(2));
			}
		}
	}

	private static void Say(string text, MessageBoxIcon icon)
	{
		MessageBox.Show(text, "MacStyle", MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
	}
}
