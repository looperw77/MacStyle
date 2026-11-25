using System;
using System.Collections.Generic;
using System.Linq;

namespace MacStyle
{
	// Pure rules (no WPF), compiled on their own by tests\run.ps1.
	internal class EdgeDwell
	{
		// Touching the last pixel row is not enough: people fling the mouse there while working.
		public const long Ms = 120;

		private long since = -1;

		public bool Update(long now, bool atEdge, bool buttonDown)
		{
			if (!atEdge || buttonDown)
			{
				since = -1;
				return false;
			}
			if (since < 0)
			{
				since = now;
			}
			return now - since >= Ms;
		}
	}
	internal class IslandStages
	{
		public const long KeepOnPillMs = 1200;

		public const long KeepOffPillMs = 250;

		private readonly EdgeDwell dwell = new EdgeDwell();

		private long leftAt = -1;

		public bool Near;

		public bool Clock;

		// dx/dy are DIPs from the top-center of the island's screen.
		public static bool IsNear(double dx, double dy, bool wasNear)
		{
			return dy >= 0.0 && Math.Abs(dx) < (wasNear ? 200 : 150) && dy < (wasNear ? 70 : 40);
		}

		public void Update(long now, bool near, bool atEdge, bool buttonDown, bool onPill)
		{
			Near = near;
			bool pressed = dwell.Update(now, atEdge, buttonDown);
			if (atEdge)
			{
				leftAt = -1;
				Clock |= pressed;
				return;
			}
			if (Clock)
			{
				if (leftAt < 0)
				{
					leftAt = now;
				}
				// The clock gives the pixels back even when the mouse stays on it, so a tab under it is always reachable.
				Clock = now - leftAt < (onPill ? KeepOnPillMs : KeepOffPillMs);
			}
		}

		public static string Mode(bool volume, bool open, bool tray, bool media, bool notice, bool toast, bool stepped, bool clock, bool near, bool playing)
		{
			if (volume)
			{
				return "volume";
			}
			if (open)
			{
				return tray ? "tray" : (media ? "hoverMedia" : "hoverIdle");
			}
			if (notice)
			{
				return "notice";
			}
			if (toast)
			{
				return "toast";
			}
			if (stepped)
			{
				return clock ? "clock" : (near ? "line" : "hidden");
			}
			return (media && playing) ? "media" : "idle";
		}

		public static bool Clickable(string mode)
		{
			return mode != "hidden" && mode != "line" && mode != "volume" && mode != "toast";
		}
	}
	internal static class NoticeFilter
	{
		public static bool Allowed(bool enabled, string muted, string aumid)
		{
			return enabled && !IsMuted(muted, aumid);
		}

		public static bool IsMuted(string muted, string aumid)
		{
			return Split(muted).Contains(aumid ?? "", StringComparer.OrdinalIgnoreCase);
		}

		public static string SetMuted(string muted, string aumid, bool on)
		{
			List<string> list = Split(muted).Where((string x) => !string.Equals(x, aumid, StringComparison.OrdinalIgnoreCase)).ToList();
			if (on)
			{
				list.Add(aumid);
			}
			return string.Join("|", list);
		}

		public static string[] Split(string muted)
		{
			return (muted ?? "").Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
	internal static class MediaSource
	{
		// Does this exe belong to the media source id Windows reports ("Brave", "Spotify.exe", "Name_hash!App", "Brave._crx_x")?
		public static bool Owns(string exe, string aumid)
		{
			if (string.IsNullOrEmpty(exe) || string.IsNullOrEmpty(aumid))
			{
				return false;
			}
			int bang = aumid.IndexOf('!');
			if (bang > 0 && aumid.IndexOf('_') > 0)
			{
				return exe.IndexOf("\\WindowsApps\\" + aumid.Substring(0, aumid.IndexOf('_')) + "_", StringComparison.OrdinalIgnoreCase) >= 0;
			}
			string name = System.IO.Path.GetFileNameWithoutExtension(exe);
			return string.Equals(name, aumid.Split('.')[0], StringComparison.OrdinalIgnoreCase);
		}

		public static string Time(double seconds)
		{
			TimeSpan t = TimeSpan.FromSeconds(Math.Max(0.0, Math.Floor(seconds)));
			return (t.TotalHours >= 1.0) ? ((int)t.TotalHours + t.ToString("\\:mm\\:ss")) : t.ToString("m\\:ss");
		}
	}
	internal static class Fit
	{
		// Largest icon size, up to the wanted one, whose dock still fits the screen.
		public static double DockIcon(double wanted, int items, double magnify, double screenW)
		{
			double s = wanted;
			while (s > 16.0)
			{
				double slot = s + Math.Round(s * 0.2);
				double hover = (magnify - 1.0) * slot * 2.6;
				if (items * slot + hover + Math.Round(s * 0.42) + 2.0 * Math.Round(s * 0.2) + 8.0 <= screenW)
				{
					break;
				}
				s -= 1.0;
			}
			return s;
		}
	}
}
