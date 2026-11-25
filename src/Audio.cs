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
	[ComImport]
	[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	internal class MMDeviceEnumeratorCo
	{
	}
	[ComImport]
	[Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
	internal class PolicyConfigCo
	{
	}
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
	internal interface IMMDeviceEnumerator
	{
		[PreserveSig]
		int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection devices);

		[PreserveSig]
		int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice device);

		[PreserveSig]
		int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
	}
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
	internal interface IMMDeviceCollection
	{
		[PreserveSig]
		int GetCount(out uint count);

		[PreserveSig]
		int Item(uint index, out IMMDevice device);
	}
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
	internal interface IMMDevice
	{
		[PreserveSig]
		int Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object iface);

		[PreserveSig]
		int OpenPropertyStore(int access, out IPropertyStore props);

		[PreserveSig]
		int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

		[PreserveSig]
		int GetState(out int state);
	}
	[ComImport]
	[Guid("657804FA-D6AD-4496-8A60-352752AF4F89")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioEndpointVolumeCallback
	{
		[PreserveSig]
		int OnNotify(IntPtr data);
	}
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
	internal interface IAudioEndpointVolume
	{
		[PreserveSig]
		int RegisterControlChangeNotify(IAudioEndpointVolumeCallback cb);

		[PreserveSig]
		int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback cb);

		[PreserveSig]
		int GetChannelCount(out uint count);

		[PreserveSig]
		int SetMasterVolumeLevel(float db, ref Guid ctx);

		[PreserveSig]
		int SetMasterVolumeLevelScalar(float level, ref Guid ctx);

		[PreserveSig]
		int GetMasterVolumeLevel(out float db);

		[PreserveSig]
		int GetMasterVolumeLevelScalar(out float level);

		[PreserveSig]
		int SetChannelVolumeLevel(uint ch, float db, ref Guid ctx);

		[PreserveSig]
		int SetChannelVolumeLevelScalar(uint ch, float level, ref Guid ctx);

		[PreserveSig]
		int GetChannelVolumeLevel(uint ch, out float db);

		[PreserveSig]
		int GetChannelVolumeLevelScalar(uint ch, out float level);

		[PreserveSig]
		int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, ref Guid ctx);

		[PreserveSig]
		int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
	}
	[ComImport]
	[Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioSessionManager2
	{
		[PreserveSig]
		int GetAudioSessionControl(IntPtr guid, int flags, out IntPtr ctrl);

		[PreserveSig]
		int GetSimpleAudioVolume(IntPtr guid, int flags, out IntPtr vol);

		[PreserveSig]
		int GetSessionEnumerator(out IAudioSessionEnumerator e);
	}
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
	internal interface IAudioSessionEnumerator
	{
		[PreserveSig]
		int GetCount(out int count);

		[PreserveSig]
		int GetSession(int index, [MarshalAs(UnmanagedType.IUnknown)] out object session);
	}
	[ComImport]
	[Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioSessionControl2
	{
		[PreserveSig]
		int GetState(out int state);

		[PreserveSig]
		int GetDisplayName(out IntPtr name);

		[PreserveSig]
		int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, ref Guid ctx);

		[PreserveSig]
		int GetIconPath(out IntPtr path);

		[PreserveSig]
		int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string path, ref Guid ctx);

		[PreserveSig]
		int GetGroupingParam(out Guid g);

		[PreserveSig]
		int SetGroupingParam(ref Guid g, ref Guid ctx);

		[PreserveSig]
		int RegisterAudioSessionNotification(IntPtr n);

		[PreserveSig]
		int UnregisterAudioSessionNotification(IntPtr n);

		[PreserveSig]
		int GetSessionIdentifier(out IntPtr id);

		[PreserveSig]
		int GetSessionInstanceIdentifier(out IntPtr id);

		[PreserveSig]
		int GetProcessId(out uint pid);

		[PreserveSig]
		int IsSystemSoundsSession();

		[PreserveSig]
		int SetDuckingPreference([MarshalAs(UnmanagedType.Bool)] bool optOut);
	}
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
	internal interface ISimpleAudioVolume
	{
		[PreserveSig]
		int SetMasterVolume(float level, ref Guid ctx);

		[PreserveSig]
		int GetMasterVolume(out float level);

		[PreserveSig]
		int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, ref Guid ctx);

		[PreserveSig]
		int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
	}
	[ComImport]
	[Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IPolicyConfig
	{
		[PreserveSig]
		int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr fmt);

		[PreserveSig]
		int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id, int def, IntPtr fmt);

		[PreserveSig]
		int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id);

		[PreserveSig]
		int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr a, IntPtr b);

		[PreserveSig]
		int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string id, int def, IntPtr a, IntPtr b);

		[PreserveSig]
		int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr a);

		[PreserveSig]
		int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr a);

		[PreserveSig]
		int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr a);

		[PreserveSig]
		int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr a, IntPtr b);

		[PreserveSig]
		int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr a, IntPtr b);

		[PreserveSig]
		int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string id, int role);

		[PreserveSig]
		int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string id, int visible);
	}
	internal class AudioDevice
	{
		public string Id;

		public string Name;

		public bool IsDefault;
	}
	internal class AudioApp
	{
		public string Name;

		public string Exe;

		public ImageSource Icon;

		public List<ISimpleAudioVolume> Volumes = new List<ISimpleAudioVolume>();

		public float Level;

		public bool Muted;
	}
	internal class Audio : IAudioEndpointVolumeCallback
	{
		public static readonly Guid UiCtx = new Guid("6d1f2b7e-1a52-4c6e-9e0a-4d5a8c3f0b11");

		private readonly Dispatcher ui;

		private IMMDeviceEnumerator en;

		private IAudioEndpointVolume vol;

		private string devId = "";

		private readonly DispatcherTimer watch;

		public bool Available => vol != null;

		public float Level
		{
			get
			{
				float level = 0f;
				if (vol != null)
				{
					vol.GetMasterVolumeLevelScalar(out level);
				}
				return level;
			}
		}

		public bool Muted
		{
			get
			{
				bool mute = false;
				if (vol != null)
				{
					vol.GetMute(out mute);
				}
				return mute;
			}
		}

		public event Action<float, bool, bool> Changed;

		[DllImport("ole32.dll")]
		private static extern int PropVariantClear(ref ShellApi.PROPVARIANT pv);

		public Audio(Dispatcher ui)
		{
			this.ui = ui;
			try
			{
				en = (IMMDeviceEnumerator)new MMDeviceEnumeratorCo();
				Bind();
			}
			catch (Exception ex)
			{
				Sys.Log("audio: " + ex.Message);
			}
			watch = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(2.0)
			};
			watch.Tick += delegate
			{
				try
				{
					if (DefaultId() != devId)
					{
						Bind();
					}
				}
				catch
				{
				}
			};
			watch.Start();
		}

		private string DefaultId()
		{
			if (en == null || en.GetDefaultAudioEndpoint(0, 1, out var device) != 0 || device == null)
			{
				return "";
			}
			device.GetId(out var id);
			return id ?? "";
		}

		private void Bind()
		{
			if (vol != null)
			{
				try
				{
					vol.UnregisterControlChangeNotify(this);
				}
				catch
				{
				}
				vol = null;
			}
			if (en.GetDefaultAudioEndpoint(0, 1, out var device) != 0 || device == null)
			{
				devId = "";
				return;
			}
			device.GetId(out devId);
			Guid iid = typeof(IAudioEndpointVolume).GUID;
			if (device.Activate(ref iid, 23, IntPtr.Zero, out var iface) == 0)
			{
				vol = (IAudioEndpointVolume)iface;
				vol.RegisterControlChangeNotify(this);
			}
		}

		public void SetLevel(float v, bool fromUi)
		{
			if (vol != null)
			{
				Guid ctx = (fromUi ? UiCtx : Guid.Empty);
				v = Math.Max(0f, Math.Min(1f, v));
				vol.SetMasterVolumeLevelScalar(v, ref ctx);
				if (v > 0f && Muted)
				{
					vol.SetMute(mute: false, ref ctx);
				}
			}
		}

		public void SetMuted(bool m, bool fromUi)
		{
			if (vol != null)
			{
				Guid ctx = (fromUi ? UiCtx : Guid.Empty);
				vol.SetMute(m, ref ctx);
			}
		}

		public void Step(int dir)
		{
			if (vol != null)
			{
				float v = (float)(Math.Round(Level * 50f) / 50.0) + (float)dir * 0.02f;
				SetLevel(v, fromUi: false);
				if (dir > 0 && Muted)
				{
					SetMuted(m: false, fromUi: false);
				}
			}
		}

		public int OnNotify(IntPtr data)
		{
			try
			{
				Guid guid = (Guid)Marshal.PtrToStructure(data, typeof(Guid));
				bool muted = Marshal.ReadInt32(data, 16) != 0;
				float level = BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(data, 20)), 0);
				bool fromUi = guid == UiCtx;
				ui.BeginInvoke((Action)delegate
				{
					if (this.Changed != null)
					{
						this.Changed(level, muted, fromUi);
					}
				});
			}
			catch
			{
			}
			return 0;
		}

		public List<AudioDevice> Devices()
		{
			List<AudioDevice> list = new List<AudioDevice>();
			if (en == null)
			{
				return list;
			}
			string text = DefaultId();
			if (en.EnumAudioEndpoints(0, 1, out var devices) != 0 || devices == null)
			{
				return list;
			}
			devices.GetCount(out var count);
			for (uint num = 0u; num < count; num++)
			{
				if (devices.Item(num, out var device) == 0 && device != null)
				{
					device.GetId(out var id);
					list.Add(new AudioDevice
					{
						Id = id,
						Name = (FriendlyName(device) ?? "Speakers"),
						IsDefault = (id == text)
					});
				}
			}
			return list;
		}

		private static string FriendlyName(IMMDevice d)
		{
			if (d.OpenPropertyStore(0, out var props) != 0 || props == null)
			{
				return null;
			}
			ShellApi.PROPERTYKEY k = new ShellApi.PROPERTYKEY
			{
				fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"),
				pid = 14u
			};
			if (props.GetValue(ref k, out var v) != 0)
			{
				return null;
			}
			try
			{
				return (v.vt == 31 && v.p != IntPtr.Zero) ? Marshal.PtrToStringUni(v.p) : null;
			}
			finally
			{
				PropVariantClear(ref v);
			}
		}

		public void SetDefault(string id)
		{
			try
			{
				IPolicyConfig policyConfig = (IPolicyConfig)new PolicyConfigCo();
				for (int i = 0; i < 3; i++)
				{
					policyConfig.SetDefaultEndpoint(id, i);
				}
				Bind();
			}
			catch (Exception ex)
			{
				Sys.Log("audio default: " + ex.Message);
			}
		}

		public List<AudioApp> Apps()
		{
			Dictionary<string, AudioApp> dictionary = new Dictionary<string, AudioApp>(StringComparer.OrdinalIgnoreCase);
			List<AudioApp> list = new List<AudioApp>();
			try
			{
				if (en == null || en.GetDefaultAudioEndpoint(0, 1, out var device) != 0 || device == null)
				{
					return list;
				}
				Guid iid = typeof(IAudioSessionManager2).GUID;
				if (device.Activate(ref iid, 23, IntPtr.Zero, out var iface) != 0)
				{
					return list;
				}
				IAudioSessionManager2 audioSessionManager = (IAudioSessionManager2)iface;
				if (audioSessionManager.GetSessionEnumerator(out var e) != 0 || e == null)
				{
					return list;
				}
				e.GetCount(out var count);
				for (int i = 0; i < count; i++)
				{
					if (e.GetSession(i, out var session) != 0 || session == null)
					{
						continue;
					}
					IAudioSessionControl2 audioSessionControl = session as IAudioSessionControl2;
					ISimpleAudioVolume simpleAudioVolume = session as ISimpleAudioVolume;
					if (audioSessionControl == null || simpleAudioVolume == null)
					{
						continue;
					}
					audioSessionControl.GetState(out var state);
					if (state == 2)
					{
						continue;
					}
					string text;
					string text2;
					if (audioSessionControl.IsSystemSoundsSession() == 0)
					{
						text = "#system";
						text2 = "أصوات الويندوز";
					}
					else
					{
						audioSessionControl.GetProcessId(out var pid);
						if (pid == 0)
						{
							continue;
						}
						text = Native.ProcessPath((int)pid) ?? "";
						if (text == "")
						{
							continue;
						}
						text2 = null;
						try
						{
							FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
							text2 = ((!string.IsNullOrWhiteSpace(versionInfo.FileDescription)) ? versionInfo.FileDescription.Trim() : versionInfo.ProductName);
						}
						catch
						{
						}
						if (string.IsNullOrWhiteSpace(text2))
						{
							text2 = System.IO.Path.GetFileNameWithoutExtension(text);
						}
					}
					if (!dictionary.TryGetValue(text, out var value))
					{
						AudioApp audioApp = new AudioApp();
						audioApp.Name = text2;
						audioApp.Exe = text;
						value = audioApp;
						if (text != "#system")
						{
							string text3 = null;
							audioSessionControl.GetProcessId(out var pid2);
							if (text.IndexOf("\\WindowsApps\\", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								text3 = ShellApi.ProcessAumid((int)pid2);
							}
							if (!string.IsNullOrEmpty(text3))
							{
								value.Icon = ShellApi.PackagedLogo(text3);
							}
							if (value.Icon == null)
							{
								value.Icon = ShellApi.Icon(text, 256, useCache: true);
							}
						}
						simpleAudioVolume.GetMasterVolume(out var level);
						simpleAudioVolume.GetMute(out var mute);
						value.Level = level;
						value.Muted = mute;
						dictionary[text] = value;
						list.Add(value);
					}
					value.Volumes.Add(simpleAudioVolume);
				}
			}
			catch (Exception ex)
			{
				Sys.Log("audio apps: " + ex.Message);
			}
			list.Sort((AudioApp a, AudioApp b) => ((a.Exe == "#system") ? 1 : 0) - ((b.Exe == "#system") ? 1 : 0));
			return list;
		}

		public static void MuteApp(AudioApp app, bool mute)
		{
			Guid ctx = UiCtx;
			foreach (ISimpleAudioVolume volume in app.Volumes)
			{
				try
				{
					volume.SetMute(mute, ref ctx);
				}
				catch
				{
				}
			}
			app.Muted = mute;
		}

		public static void SetApp(AudioApp app, float level)
		{
			Guid ctx = UiCtx;
			foreach (ISimpleAudioVolume volume in app.Volumes)
			{
				try
				{
					volume.SetMasterVolume(level, ref ctx);
					if (level > 0f)
					{
						volume.SetMute(mute: false, ref ctx);
					}
				}
				catch
				{
				}
			}
			app.Level = level;
		}
	}
}
