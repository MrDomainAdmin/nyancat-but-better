using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;


namespace Nyancat
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WindowMover
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        static extern bool IsWindow(IntPtr hWnd);


        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;

        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;
        const uint SC_CLOSE = 0xF060;

        private volatile bool _stop;

        public void Start()
        {
            PlayBackgroundSound();

            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);
            Process currentProcess = Process.GetCurrentProcess();
            // Get the main window handle of the current process
            IntPtr hWnd = GetForegroundWindow();
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            ArrayList hWndArray = new ArrayList();
            hWndArray.Add(hWnd);
            while (!_stop)
            {
                hWnd = GetForegroundWindow();
                if (!hWndArray.Contains(hWnd))
                {
                    hWndArray.Add(hWnd);
                }



                // Query the running processes to find Task Manager's PID
                Process[] processes = Process.GetProcessesByName("Taskmgr");
                foreach (Process process in processes)
                {
                    hWndArray.Add(process.MainWindowHandle);
                }
                    RECT rect;
                for (int i = 0; i < hWndArray.Count; i++)
                {
                    if (IsWindow((IntPtr)hWndArray[i]))
                    {
                        GetWindowRect((IntPtr)hWndArray[i], out rect);
                        int width = rect.right - rect.left;
                        int height = rect.bottom - rect.top;
                        int x = rect.left;
                        int y = rect.top;

                        int newX = x + GetRandomNumber(-25, 25);
                        int newY = y + GetRandomNumber(-25, 25);

                        if (newX < 0) newX = 0;
                        if (newY < 0) newY = 0;
                        if (newX + width > screenWidth) newX = screenWidth - width;
                        if (newY + height > screenHeight) newY = screenHeight - height;

                        SetWindowPos((IntPtr)hWndArray[i], IntPtr.Zero, newX, newY, width, height, 0);
                    }
                }

                Thread.Sleep(10);
            }
        }

        public void Stop()
        {
            _stop = true;
        }

        static void PlayBackgroundSound()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nyancat.nyan.wav"))
            {
                SoundPlayer soundPlayer = new SoundPlayer(stream);
                soundPlayer.PlayLooping();
            }
        }

        static int GetRandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max + 1);
        }

        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
