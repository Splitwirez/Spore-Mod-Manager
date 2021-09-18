using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SporeMods.Core
{
	public static class ServantCommands
	{
		public static string CreateDragServant()
        {
			if (File.Exists(DragWindowHwndPath))
				File.Delete(DragWindowHwndPath);

			string id = string.Empty;
			Process dragServant = CrossProcess.StartDragServant();
			if (dragServant != null)
				id = dragServant.Id.ToString();

			return $"{ServantCommands.DRAG_SERVANT_ID_ARG}{id}";
		}

		public const string DRAG_SERVANT_ID_ARG = "-dragServantId:";

		static Process DragServantProcess = null;
		public static bool LocateDragServant()
        {
			if (!HasDragServant)
			{
				string[] clArgs = Environment.GetCommandLineArgs();
				foreach (string arg in clArgs)
				{
					string targ = arg.Trim(' ', '"');
					if (targ.StartsWith(DRAG_SERVANT_ID_ARG))
					{
						DragServantProcess = Process.GetProcessById(int.Parse(targ.Replace(DRAG_SERVANT_ID_ARG, string.Empty)));
						break;
					}
				}
			}


			if (!HasDragServant)
			{
				var firstDragServant = Process.GetProcessesByName(CrossProcess.DRAG_EXE).FirstOrDefault();
				if ((firstDragServant != null) && (!firstDragServant.HasExited))
					DragServantProcess = firstDragServant;
			}


			return HasDragServant;
		}

		public static bool HasDragServant
        {
			get => (DragServantProcess != null) && (!DragServantProcess.HasExited);
		}


		static IntPtr _servantHwnd = IntPtr.Zero;
		public static bool TryGetDragServantHwnd(out IntPtr hWnd)
        {
			if (HasDragServant)
			{
				if (_servantHwnd != IntPtr.Zero)
				{
					hWnd = _servantHwnd;
					return true;
				}

				string hwndPath = DragWindowHwndPath;
				if (File.Exists(hwndPath))
                {
					if (int.TryParse(File.ReadAllText(hwndPath), out int ihWnd))
					{

						_servantHwnd = new IntPtr(ihWnd);
						File.Delete(hwndPath);
					}
				}
				
				if (_servantHwnd == IntPtr.Zero)
					_servantHwnd = DragServantProcess.MainWindowHandle;

				hWnd = _servantHwnd;
				return true;
			}
			else
			{
				hWnd = IntPtr.Zero;
				return false;
			}
        }


		static string DragWindowHwndPath
		{
			get => Path.Combine(Settings.TempFolderPath, "dragWindowHwnd");
		}

		public static void SendDragWindowHwnd(IntPtr hWnd)
        {
			File.WriteAllText(DragWindowHwndPath, hWnd.ToString());
		}
		
		
		public static bool DragWindowHwndSent
        {
			get => File.Exists(DragWindowHwndPath);
        }


		static string DroppedFilesPath
		{
			get => Path.Combine(Settings.TempFolderPath, "droppedFiles");
		}

		public static void SendDroppedFiles(IEnumerable<string> files)
        {
			string msg = "FILES: ";
			foreach (string f in files)
			{
				msg += $"\n\t{f}";
			}
			
			Console.WriteLine(msg);

			File.WriteAllLines(DroppedFilesPath, files);
			Permissions.GrantAccessFile(DroppedFilesPath);
		}


		static FileSystemWatcher _dragWatcher = null;

		public static bool WatchForServantEvents
        {
			get => (_dragWatcher != null) ? _dragWatcher.EnableRaisingEvents : false;
			set
            {
				if (EnsureWatchingForServantEvents(value))
					_dragWatcher.EnableRaisingEvents = value;
			}
		}

		static bool EnsureWatchingForServantEvents(bool enable)
        {
			if (enable && (_dragWatcher == null))
			{
				if (File.Exists(DroppedFilesPath))
					File.Delete(DroppedFilesPath);


				_dragWatcher = new FileSystemWatcher(Settings.TempFolderPath)
				{
					EnableRaisingEvents = false,
					IncludeSubdirectories = false
				};

				_dragWatcher.Created += (sneder, args) =>
				{
					while (Permissions.IsFileLocked(args.FullPath))
					{ }
					if (Path.GetFileName(args.FullPath) == Path.GetFileName(DroppedFilesPath))
					{
						FilesDropped?.Invoke(null, new FileDropEventArgs(File.ReadAllLines(args.FullPath)));

						/*Dispatcher.BeginInvoke(new Action(() =>
						{
							/*if (Path.GetFileName(args.FullPath) == "draggedFiles")
							{*
							InstallModsFromFilesAsync(File.ReadAllLines(args.FullPath));
							File.Delete(args.FullPath);
							DropModsDialogContentControl.IsOpen = false;
							//}
						}));*/
					}
				};
			}

			return _dragWatcher != null;
		}

		public static event EventHandler<FileDropEventArgs> FilesDropped;


		public static void CloseDragServant()
			=> DragServantProcess.Kill();

		public static void RunLauncher()
		{
			string launchGamePath = Path.Combine(Settings.TempFolderPath, "LaunchGame");
			if (File.Exists(launchGamePath))
				File.Delete(launchGamePath);

			File.WriteAllText(launchGamePath, string.Empty);
			Permissions.GrantAccessFile(launchGamePath);
		}

		public static Process RunLkImporter()
		{
			return RunLkImporter(true);
		}

		public static Process RunLkImporter(bool exitCaller)
		{
			string forceLkImportPath = Path.Combine(Settings.ProgramDataPath, "ForceLkImport.info");
			string parentDirectoryPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
			if (File.Exists(forceLkImportPath))
			{
				Process process = CrossProcess.StartLauncherKitImporter("\"--relaunch:" + Process.GetCurrentProcess().MainModule.FileName + "\""); /*Process.Start(new ProcessStartInfo(Path.Combine(parentDirectoryPath, "SporeMods.KitImporter.exe"), )
				{
					UseShellExecute = true
				});*/
				if (exitCaller)
					Process.GetCurrentProcess().Kill();
				
				return process;
			}
			return null;
		}
	}

	public class FileDropEventArgs : EventArgs
    {
		public readonly IEnumerable<string> Files = null;
		internal FileDropEventArgs(IEnumerable<string> lines)
        {
			Files = lines;
		}
    }
}
