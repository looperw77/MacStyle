using System;
using System.Runtime.InteropServices;

namespace MacStyle
{
	internal struct Mon
	{
		public Native.RECT Px;

		public Native.RECT Work;

		public double Dpi;

		public double W => (Px.R - Px.L) / Dpi;

		public double H => (Px.B - Px.T) / Dpi;

		public double WorkW => (Work.R - Work.L) / Dpi;

		public double WorkH => (Work.B - Work.T) / Dpi;

		public int CenterX => (Px.L + Px.R) / 2;

		public bool Contains(Native.POINT p)
		{
			return p.X >= Px.L && p.X < Px.R && p.Y >= Px.T && p.Y < Px.B;
		}

		public bool Same(Mon o)
		{
			return Px.Equals(o.Px) && Work.Equals(o.Work) && Dpi == o.Dpi;
		}
	}
	// Everything here is physical pixels: WPF Left/Top are ambiguous once screens have different scales.
	internal static class Screens
	{
		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromPoint(Native.POINT p, uint f);

		[DllImport("shcore.dll")]
		private static extern int GetDpiForMonitor(IntPtr m, int type, out uint x, out uint y);

		public static Mon Primary()
		{
			return Get(MonitorFromPoint(default(Native.POINT), 1u));
		}

		public static Mon AtCursor()
		{
			Native.GetCursorPos(out var p);
			return Get(MonitorFromPoint(p, 2u));
		}

		private static Mon Get(IntPtr h)
		{
			Native.MONITORINFO mi = default(Native.MONITORINFO);
			mi.cbSize = Marshal.SizeOf(mi);
			Native.GetMonitorInfo(h, ref mi);
			if (GetDpiForMonitor(h, 0, out var x, out var y) != 0 || x == 0)
			{
				x = 96u;
			}
			return new Mon
			{
				Px = mi.rcMonitor,
				Work = mi.rcWork,
				Dpi = x / 96.0
			};
		}

		public static void Place(IntPtr h, int x, int y, int w, int ht)
		{
			if (h == IntPtr.Zero || (Native.GetWindowRect(h, out var r) && r.L == x && r.T == y && r.R - r.L == w && r.B - r.T == ht))
			{
				return;
			}
			Native.SetWindowPos(h, IntPtr.Zero, x, y, w, ht, 20u);
		}

		// Centers a size-to-content window. WPF keeps the pixel width of the scale it was hidden at, so it is set here too.
		public static void Center(IntPtr h, Mon mon, double w, double top)
		{
			int num = (int)Math.Round(w * mon.Dpi);
			for (int i = 0; i < 2; i++)
			{
				Native.GetWindowRect(h, out var r);
				Native.SetWindowPos(h, IntPtr.Zero, mon.CenterX - num / 2, mon.Px.T + (int)(top * mon.Dpi), num, r.B - r.T, 20u);
			}
		}
	}
}
