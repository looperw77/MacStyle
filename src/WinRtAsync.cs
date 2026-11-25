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
	internal static class WinRtAsync
	{
		public static Task<T> ToTask<T>(this IAsyncOperation<T> op)
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			op.Completed = delegate(IAsyncOperation<T> o, AsyncStatus status)
			{
				switch (status)
				{
				case AsyncStatus.Completed:
					tcs.TrySetResult(o.GetResults());
					break;
				case AsyncStatus.Error:
					tcs.TrySetException(o.ErrorCode);
					break;
				default:
					tcs.TrySetCanceled();
					break;
				}
			};
			return tcs.Task;
		}

		public static TaskAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> op)
		{
			return op.ToTask().GetAwaiter();
		}
	}
}
