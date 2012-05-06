using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PrintScreen.Properties;

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
            Icon = (Icon)Resources.ResourceManager.GetObject("appicon")
         };

         icon.ContextMenu = new ContextMenu(
            new [] {
               new MenuItem("E&xit", ExitClicked)
            });

         context = new WindowsFormsSynchronizationContext();

         HotKeyManager.RegisterHotKey(Keys.PrintScreen, 0);
         HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

         Application.Run(new ApplicationContext());
      }

      private static void ExitClicked(object sender, EventArgs args)
      {
         icon.Dispose();
         Application.Exit();
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

         string fileName = string.Format("snapshot_{0}.png",
            DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss"));

         var filePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
         screenshot.Save(filePath, ImageFormat.Png);
      }
   }
}
