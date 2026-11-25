using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace MacStyle
{
	internal class Notice
	{
		public string Aumid = "";

		public string App = "";

		public string Title = "";

		public string Body = "";

		public ImageSource Icon;
	}
	internal class Notifier
	{
		public static Notifier Current;

		private readonly Config cfg;

		private readonly DispatcherTimer timer;

		private readonly Dictionary<string, string> names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private HashSet<uint> seen;

		private bool busy;

		private string lastError = "";

		public event Action<Notice> Arrived;

		public static bool HasAccess
		{
			get
			{
				try
				{
					return UserNotificationListener.Current.GetAccessStatus() == UserNotificationListenerAccessStatus.Allowed;
				}
				catch
				{
					return false;
				}
			}
		}

		public Notifier(Config cfg)
		{
			Current = this;
			this.cfg = cfg;
			// shortcut: polls because NotificationChanged needs package identity (0x80070490), switch to the event if MacStyle ships as MSIX/sparse package.
			timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1.0)
			};
			timer.Tick += delegate
			{
				Poll();
			};
		}

		public void Start()
		{
			timer.Start();
			Poll();
		}

		private async void Poll()
		{
			if (!cfg.NotifyEnabled || !cfg.IslandEnabled)
			{
				seen = null;
				return;
			}
			if (busy)
			{
				return;
			}
			busy = true;
			try
			{
				if (!HasAccess)
				{
					seen = null;
					return;
				}
				IReadOnlyList<UserNotification> list = await UserNotificationListener.Current.GetNotificationsAsync(NotificationKinds.Toast);
				HashSet<uint> old = seen;
				seen = new HashSet<uint>(list.Select((UserNotification n) => n.Id));
				foreach (UserNotification item in list.Where((UserNotification n) => old == null || !old.Contains(n.Id)).OrderBy((UserNotification n) => n.CreationTime))
				{
					Notice notice = Read(item);
					// The first pass only learns what is already in the notification center.
					if (old != null && NoticeFilter.Allowed(cfg.NotifyEnabled, cfg.NotifyMuted, notice.Aumid) && this.Arrived != null)
					{
						notice.Icon = IconOf(notice.Aumid);
						this.Arrived(notice);
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.Message != lastError)
				{
					lastError = ex.Message;
					Sys.Log("notifications: " + ex.Message);
				}
			}
			finally
			{
				busy = false;
			}
		}

		private Notice Read(UserNotification n)
		{
			Notice notice = new Notice();
			try
			{
				notice.Aumid = n.AppInfo.AppUserModelId ?? "";
				notice.App = n.AppInfo.DisplayInfo.DisplayName ?? "";
			}
			catch
			{
			}
			if (notice.Aumid != "")
			{
				names[notice.Aumid] = ((notice.App != "") ? notice.App : NameOf(notice.Aumid));
				notice.App = names[notice.Aumid];
			}
			try
			{
				NotificationBinding binding = n.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
				List<string> list = ((binding == null) ? new List<string>() : (from x in binding.GetTextElements()
					select x.Text into x
					where !string.IsNullOrWhiteSpace(x)
					select x.Trim()).ToList());
				notice.Title = ((list.Count > 0) ? list[0] : notice.App);
				notice.Body = string.Join("\n", list.Skip(1));
			}
			catch
			{
				notice.Title = notice.App;
			}
			return notice;
		}

		public static ImageSource IconOf(string aumid)
		{
			if (string.IsNullOrEmpty(aumid))
			{
				return null;
			}
			return ShellApi.PackagedLogo(aumid) ?? ShellApi.Icon("shell:AppsFolder\\" + aumid, 48, useCache: true);
		}

		private static bool IsSystem(string aumid)
		{
			return aumid.StartsWith("Windows.") || aumid.StartsWith("Microsoft.Windows.");
		}

		private static string NameOf(string aumid)
		{
			string text = ShellApi.DisplayName("shell:AppsFolder\\" + aumid);
			return string.IsNullOrEmpty(text) ? ((IsSystem(aumid) ? "ويندوز: " : "") + Island.AppName(aumid.Substring(aumid.LastIndexOf('/') + 1))) : text;
		}

		// Every app Windows has registered for notifications, plus anything seen now or already muted.
		public List<KeyValuePair<string, string>> Apps()
		{
			HashSet<string> ids = new HashSet<string>(names.Keys, StringComparer.OrdinalIgnoreCase);
			ids.UnionWith(NoticeFilter.Split(cfg.NotifyMuted));
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings"))
				{
					if (registryKey != null)
					{
						ids.UnionWith(registryKey.GetSubKeyNames().Where((string x) => (!x.StartsWith("NotifyIconGeneratedAumid_") && !x.StartsWith("{")) || names.ContainsKey(x)));
					}
				}
			}
			catch
			{
			}
			return (from x in ids
				select new KeyValuePair<string, string>(x, names.ContainsKey(x) ? names[x] : NameOf(x)) into x
				orderby IsSystem(x.Key), x.Value
				select x).ToList();
		}
	}
}
