using System.Windows;
using System.Windows.Interop;
using System.Management;
using System.Diagnostics;
namespace DeviceChangeMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVNODES_CHANGED0 = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x0005;
        private const int DIGCF_PRESENT = 0x0002;
        private const int DIGCF_DEVICEINTERFACE = 0x0010;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {            
            if(msg == WM_DEVICECHANGE)
            {
                Debug.WriteLine(wParam);
                switch((int)wParam)
                {
                    
                    case DBT_DEVICEARRIVAL:
                        {
                            Debug.WriteLine("USBDeviceAttached.");
                            // USBデバイスが接続された
                            GetUSBDevices();
                            break;
                        }
                    case DBT_DEVICEREMOVECOMPLETE:
                        {
                            // USBデバイスが取り外された
                            GetUSBDevices();
                            break;
                        }
                }
            }
            return IntPtr.Zero;
        }

        private void GetUSBDevices()
        {
            //ManagementObjectSearcher do not work STA mode. WPF work STA Mode.
            ThreadPool.QueueUserWorkItem(_ => {               
                using(var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                {                
                    var collection = searcher.Get();                
                    foreach(var device in collection)
                    {
                        string? deviceId = device.GetPropertyValue("DeviceID").ToString();
                        Debug.WriteLine($"deviceId:${deviceId}");
                        string[] parts = deviceId != null ? deviceId.Split('\\') : [];
                        if(parts.Length > 1 && parts[1].StartsWith("VID_"))
                        {
                            string vid = parts[1].Substring(4, 4);
                            string pid = parts[1].Substring(13, 4);
                            Console.WriteLine($"VID: {vid}, PID: {pid}");
                            Debug.WriteLine($"VID: {vid}, PID: {pid}");
                        }
                    }
                }
            });
        }
    }
}