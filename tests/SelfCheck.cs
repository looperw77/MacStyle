using System;
using MacStyle;

internal static class SelfCheck
{
	private static int n;

	private static void Is(bool ok, string what)
	{
		n++;
		if (!ok)
		{
			throw new Exception("FAILED: " + what);
		}
	}

	private static string Mode(IslandStages s, bool stepped = true, bool open = false, bool media = true, bool notice = false)
	{
		return IslandStages.Mode(false, open, false, media, notice, false, stepped, s.Clock, s.Near, true);
	}

	private static void Main()
	{
		// island stages
		IslandStages s = new IslandStages();
		s.Update(0, false, false, false, false);
		Is(Mode(s) == "hidden", "far away = nothing");
		s.Update(30, IslandStages.IsNear(0, 30, false), false, false, false);
		Is(Mode(s) == "line", "near = line only");
		s.Update(60, true, true, false, true);
		Is(Mode(s) == "line", "touching the edge for an instant is not enough");
		s.Update(60 + EdgeDwell.Ms, true, true, false, true);
		Is(Mode(s) == "clock", "holding the edge = clock");
		Is(Mode(s, open: true) == "hoverMedia", "opens only when asked");
		s.Update(5000, true, true, false, true);
		Is(Mode(s) == "clock", "clock stays while held at the edge");
		s.Update(5030, true, false, false, true);
		s.Update(5030 + IslandStages.KeepOnPillMs - 1, true, false, false, true);
		Is(Mode(s) == "clock", "time to click the clock");
		s.Update(5030 + IslandStages.KeepOnPillMs, true, false, false, true);
		Is(Mode(s) == "line", "clock releases what is under it");
		s.Update(9000, false, false, false, false);
		Is(Mode(s) == "hidden", "closes when the mouse leaves");
		IslandStages d = new IslandStages();
		d.Update(0, true, true, true, true);
		d.Update(1000, true, true, true, true);
		Is(Mode(d) == "line", "dragging along the edge does not open the clock");
		IslandStages off = new IslandStages();
		off.Update(0, true, true, false, true);
		off.Update(200, true, true, false, true);
		off.Update(230, true, false, false, false);
		off.Update(230 + IslandStages.KeepOffPillMs, true, false, false, false);
		Is(Mode(off) == "line", "leaving the clock closes it fast");
		foreach (string m in new[] { "hidden", "line", "volume", "toast" })
		{
			Is(!IslandStages.Clickable(m), m + " lets clicks through");
		}
		Is(IslandStages.Clickable("clock") && IslandStages.Clickable("notice"), "clock and notices take clicks");
		Is(Mode(s, stepped: false) == "media" && Mode(s, stepped: false, media: false) == "idle", "desktop keeps the pill");
		Is(Mode(s, open: true, notice: true) == "hoverMedia" && Mode(s, notice: true) == "notice", "a notice waits for the open player");
		// near zone
		Is(!IslandStages.IsNear(0, -1, true), "screen above does not count");
		Is(IslandStages.IsNear(0, 60, true) && !IslandStages.IsNear(0, 60, false), "near hysteresis");
		Is(!IslandStages.IsNear(160, 5, false), "near is narrow");
		// edge dwell
		EdgeDwell e = new EdgeDwell();
		Is(!e.Update(0, true, false) && !e.Update(EdgeDwell.Ms - 1, true, false) && e.Update(EdgeDwell.Ms, true, false), "dwell");
		Is(!e.Update(200, false, false) && !e.Update(210, true, false), "leaving resets dwell");
		Is(!e.Update(900, true, true) && !e.Update(910, true, false), "a button resets dwell");
		// notification filter
		Is(NoticeFilter.Allowed(true, "", "a") && !NoticeFilter.Allowed(false, "", "a"), "master switch");
		string muted = NoticeFilter.SetMuted("", "App!X", true);
		Is(!NoticeFilter.Allowed(true, muted, "app!x") && NoticeFilter.Allowed(true, muted, "App!Y"), "one app muted");
		Is(NoticeFilter.Allowed(true, muted, "App!"), "no partial match");
		muted = NoticeFilter.SetMuted(muted, "b", true);
		Is(NoticeFilter.SetMuted(muted, "APP!X", false) == "b", "unmute");
		Is(NoticeFilter.SetMuted("b", "b", true) == "b", "no duplicates");
		Is(NoticeFilter.Allowed(true, null, null), "missing id");
		// media source
		Is(MediaSource.Owns(@"C:\x\brave.exe", "Brave") && MediaSource.Owns(@"C:\x\brave.exe", "Brave._crx_abc"), "brave");
		Is(MediaSource.Owns(@"C:\x\msedge.exe", "MSEdge") && MediaSource.Owns(@"C:\x\Spotify.exe", "Spotify.exe"), "edge, spotify");
		Is(MediaSource.Owns(@"C:\Program Files\WindowsApps\SpotifyAB.SpotifyMusic_1.2_x64__zp\Spotify.exe", "SpotifyAB.SpotifyMusic_zp!Spotify"), "store app");
		Is(!MediaSource.Owns(@"C:\x\chrome.exe", "Brave") && !MediaSource.Owns("", "Brave"), "other app");
		Is(MediaSource.Time(65.9) == "1:05" && MediaSource.Time(3725) == "1:02:05" && MediaSource.Time(-3) == "0:00", "time text");
		// dock fit at every scale of a 1920x1200 and a 1366x768 screen
		foreach (double px in new[] { 1920.0, 1366.0 })
		{
			foreach (double dpi in new[] { 1.0, 1.25, 1.5, 2.0 })
			{
				foreach (int items in new[] { 4, 12, 24 })
				{
					double w = px / dpi, icon = Fit.DockIcon(72, items, 2.0, w), slot = icon + Math.Round(icon * 0.2);
					Is(icon == 16 || items * slot + slot * 2.6 + Math.Round(icon * 0.42) + 2 * Math.Round(icon * 0.2) <= w, "dock fits " + w + " x" + items);
				}
			}
		}
		Is(Fit.DockIcon(46, 8, 1.55, 1536) == 46, "dock keeps the wanted size when it fits");
		Console.WriteLine(n + " checks ok");
	}
}
