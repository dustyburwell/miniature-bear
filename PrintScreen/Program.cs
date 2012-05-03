using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PrintScreen
{
   static class Program
   {
      private static SynchronizationContext context;
      private static NotifyIcon icon;
      
      [STAThread]
      static void Main()
      {
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);

         icon = new NotifyIcon {
            Visible = true, 
            Icon = new Icon("appicon.ico")
         };

         context = new WindowsFormsSynchronizationContext();

         HotKeyManager.RegisterHotKey(Keys.PrintScreen, 0);
         HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

         Application.Run(new ApplicationContext());
      }

      private static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
      {
         context.Send(o => TakeSnapshot(), null);
      }

      public static void TakeSnapshot()
      {
         Win32.WINDOWINFO info;
         Bitmap           screenshot;
         Graphics         screen;

         info = new Win32.WINDOWINFO();

         Win32.GetWindowInfo(Win32.GetForegroundWindow(), ref info);

         screenshot = new Bitmap(info.rcWindow.Width, info.rcWindow.Height, 
                                 PixelFormat.Format32bppArgb);
         screen = Graphics.FromImage(screenshot);
         screen.CopyFromScreen(info.rcWindow.X, info.rcWindow.Y,
                               0, 0, info.rcWindow.Size,
                               CopyPixelOperation.SourceCopy);

         Clipboard.SetImage(screenshot);

         var filePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "snapshot.png");
         screenshot.Save(filePath, ImageFormat.Png);
      }
   }
}
