
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Mackoy.Bvets;


namespace TR.BIDSid
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void CloseEv(object sender, RoutedEventArgs e)
    {
      Close();
    }
    public bool BIDSppIsEnabled = false;
    public int BIDSppVersionInt = 0;
    private void OnLoad(object sender, RoutedEventArgs e)
    {
      if (BIDSppIsEnabled && BIDSppVersionInt > 0) 
      {
        BIDSppIsConnectedEllipse.Fill = new SolidColorBrush(Colors.LightGreen);
        string VStr = string.Empty;
        switch (BIDSppVersionInt)
        {
          case 100:
            VStr = "1.00";
            break;
        }
        BIDSppVersion.Content = VStr;
      }
      else
      {
        BIDSppIsConnectedEllipse.Fill = new SolidColorBrush(Colors.Red);
        BIDSppVersion.Content = string.Empty;
      }
      string IPAd = string.Empty;
      foreach(IPAddress addr in Dns.GetHostAddresses(Dns.GetHostName()))
      {
        if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) IPAd += addr.ToString();
      }
      if (IPAd == string.Empty) IPAd = "NULL";
      ThisIPLab.Content = "IP:" + IPAd;
      ThisPCNameLab.Content = "Name:" + Dns.GetHostName();
    }
  }



  public class BIDSid : IInputDevice
  {
    public event InputEventHandler LeverMoved;
    public event InputEventHandler KeyDown;
    public event InputEventHandler KeyUp;
    private MainWindow mw = new MainWindow();
    public void Configure(System.Windows.Forms.IWin32Window owner)
    {
      mw.Show();
    }

    private static string SRAMName = "BIDSSharedMem";
    //SECTION_ALL_ACCESS=983071
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
    //[DllImport("kernel32.dll")]
    //static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);//予約
    [DllImport("kernel32.dll")]
    static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);


    public struct Spec
    {
      public int B;  //ブレーキ段数
      public int P;  //ノッチ段数
      public int A;  //ATS確認段数
      public int J;  //常用最大段数
      public int C;  //編成車両数
    };
    public struct State
    {
      public double X; //列車位置[m]
      public float V;  //列車速度[km/h]
      public int T;    //0時からの経過時間[ms]
      public float BC; //BC圧力[kPa]
      public float MR; //MR圧力[kPa]
      public float ER; //ER圧力[kPa]
      public float BP; //BP圧力[kPa]
      public float SAP;  //SAP圧力[kPa]
      public float I;  //電流[A]
    };
    public struct Hand
    {
      public int B;  //ブレーキハンドル位置
      public int P;  //ノッチハンドル位置
      public int R;  //レバーサーハンドル位置
      public int C;  //定速制御状態
    };
    public struct Beacon
    {
      public int Num;  //Beaconの番号
      public int Sig;  //対応する閉塞の現示番号
      public float X;  //対応する閉塞までの距離[m]
      public int Data; //Beaconの第三引数の値
    };
    //Version 200ではBeaconData,IsKeyPushed,SignalSetIntはDIsabled
    public unsafe struct BIDSSharedMemoryData
    {
      public bool IsEnabled;
      public int VersionNum;
      public Spec SpecData;
      public State StateData;
      public Hand HandleData;
      public bool IsDoorClosed;
      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public unsafe fixed int Panel[256];
      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public unsafe fixed int Sound[256];


      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public Beacon[] BeaconData;
      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public bool[] IsKeysPushed;
      //public int SignalSetInt;
    };
    static uint size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
    static IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
    BIDSSharedMemoryData bmd = new BIDSSharedMemoryData();
    BIDSSharedMemoryData OldData = new BIDSSharedMemoryData();
    public void Dispose()
    {
    }
    public void Load(string settingsPath)
    {
    }

    public void Tick()
    {
      bmd = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
      if (bmd.IsEnabled != OldData.IsEnabled)
      {
        mw.BIDSppIsEnabled = bmd.IsEnabled;
        mw.BIDSppVersionInt = bmd.VersionNum;
      }
      OldData = bmd;
    }

    public void SetAxisRanges(int[][] ranges)
    {
    }
  }

}
