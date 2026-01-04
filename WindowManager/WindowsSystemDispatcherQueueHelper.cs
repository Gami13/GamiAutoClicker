using System;
using System.Runtime.InteropServices;



namespace GamiAutoClicker.WindowManager;

internal class WindowsSystemDispatcherQueueHelper {
	[StructLayout(LayoutKind.Sequential)]
	struct DispatcherQueueOptions {
		internal int dwSize;
		internal int threadType;
		internal int apartmentType;
	}

	[DllImport("CoreMessaging.dll")]
	private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

	object? m_dispatcherQueueController = null;
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
	public void EnsureWindowsSystemDispatcherQueueController() {
		if (Windows.System.DispatcherQueue.GetForCurrentThread() != null) {
			// one already exists, so we'll just use it.
			return;
		}

		if (m_dispatcherQueueController == null) {
			DispatcherQueueOptions options;
			options.dwSize = Marshal.SizeOf<DispatcherQueueOptions>();
			options.threadType = 2;    // DQTYPE_THREAD_CURRENT
			options.apartmentType = 2; // DQTAT_COM_STA

#pragma warning disable IL2050 // Correctness of COM interop cannot be guaranteed after trimming. Interfaces and interface members might be removed.
#pragma warning disable CS8601 // Possible null reference assignment.
			CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore IL2050 // Correctness of COM interop cannot be guaranteed after trimming. Interfaces and interface members might be removed.
		}
	}
}
