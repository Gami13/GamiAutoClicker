using Microsoft.UI.Dispatching;
using System;

namespace Gami;

public static partial class EasyWindows {
	internal static class WindowsSystemDispatcherQueueHelper {
		public static void EnsureWindowsSystemDispatcherQueueController() {
			var dispatcherQueue = DispatcherQueue.GetForCurrentThread()
				?? throw new InvalidOperationException("The WinUI dispatcher queue is not available on the current thread.");

			dispatcherQueue.EnsureSystemDispatcherQueue();
		}
	}
}
