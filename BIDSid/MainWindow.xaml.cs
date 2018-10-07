
using System;
using System.IO.Pipes;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Mackoy.Bvets;
using System.Linq;
using System.Collections.Generic;

namespace TR.BIDSid
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    static readonly uint size = (uint)Marshal.SizeOf(typeof(BIDSid.BIDSSharedMemoryData));
    static IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
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

    public MainWindow()
    {
      InitializeComponent();
    }

    private void CloseEv(object sender, RoutedEventArgs e)
    {
      Close();
    }
    public bool BIDSppIsEnabled
    {
      get
      {
        return ((BIDSid.BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSid.BIDSSharedMemoryData))).IsEnabled;
      }
    }
    public int BIDSppVersionInt
    {
      get
      {
        return ((BIDSid.BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSid.BIDSSharedMemoryData))).VersionNum;
      }
    }
    private void OnLoad(object sender, RoutedEventArgs e)
    {
      hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
      pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
      if (BIDSppIsEnabled && BIDSppVersionInt > 0) 
      {
        BIDSppIsConnectedEllipse.Fill = new SolidColorBrush(Colors.LightGreen);
        BIDSppVersion.Content = BIDSppVersionInt.ToString();
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

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {
      UnmapViewOfFile(pMemory);
      CloseHandle(hSharedMemory);
    }
  }


  /// <summary>
  /// BIDSppとBIDScsの仲介役を担うクラス
  /// </summary>
  public class BIDSid : IInputDevice
  {
    public event InputEventHandler LeverMoved;
    public event InputEventHandler KeyDown;
    public event InputEventHandler KeyUp;
    public void Configure(System.Windows.Forms.IWin32Window owner)
    {
      (new MainWindow()).Show();
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
    public struct BIDSSharedMemoryData
    {
      public bool IsEnabled;
      public int VersionNum;
      public Spec SpecData;
      public State StateData;
      public Hand HandleData;
      public bool IsDoorClosed;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Panel;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Sound;


      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public Beacon[] BeaconData;
      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public bool[] IsKeysPushed;
      //public int SignalSetInt;
    };
    static readonly uint size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
    static IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
    //BIDSSharedMemoryData bmd = new BIDSSharedMemoryData();
    private int ReverserNum
    {
      set => LeverMoved(this, new InputEventArgs(0, value));
    }
    private int PowerNotchNum
    {
      set => LeverMoved(this, new InputEventArgs(1, value));
    }
    private int BrakeNotchNum
    {
      set => LeverMoved(this, new InputEventArgs(2, value));
    }
    private int SHandleNum
    {
      set => LeverMoved(this, new InputEventArgs(3, value));
    }
    private int BtDown
    {
      set
      {
        if (value < 4)
        {
          KeyDown(this, new InputEventArgs(-1, value));
        }
        else
        {
          KeyDown(this, new InputEventArgs(-2, value - 4));
        }
      }
    }
    private int BtUp
    {
      set
      {
        if (value < 4)
        {
          KeyUp(this, new InputEventArgs(-1, value));
        }
        else
        {
          KeyUp(this, new InputEventArgs(-2, value - 4));
        }
      }
    }
    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hwndParent">親ウィンドウのハンドル</param>
    /// <param name="hwndChildAfter">子ウィンドウのハンドル</param>
    /// <param name="lpszClass">クラス名</param>
    /// <param name="lpszWindow">ウィンドウ名</param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    /// <summary>
    /// 指定されたウィンドウを作成したスレッドに関連付けられているメッセージキューに、1 つのメッセージをポスト
    /// </summary>
    /// <param name="hWnd">ポスト先ウィンドウのハンドル</param>
    /// <param name="Msg">メッセージ</param>
    /// <param name="wParam">メッセージの最初のパラメータ</param>
    /// <param name="lParam">メッセージの 2 番目のパラメータ</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    IntPtr BVEWindow = (IntPtr)0;
    private bool KyDown(int num)
    {
      if (BVEWindow == IntPtr.Zero)
      {
        BVEWindow = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Bve trainsim");
      }
      if (BVEWindow == IntPtr.Zero) return false;
      PostMessage(BVEWindow, 0x0100, (IntPtr)num, (IntPtr)0);
      return true;
    }
    private bool KyUp(int num)
    {
      if (BVEWindow == IntPtr.Zero)
      {
        BVEWindow = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Bve trainsim");
      }
      if (BVEWindow == IntPtr.Zero) return false;
      PostMessage(BVEWindow, 0x0101, (IntPtr)num, (IntPtr)0);
      return true;
    }


    /*
    static int NowPHan = 0;
    static int NowBHan = 0;
    static int NowRHan = 0;
    static int NowSHan = 0;
    static bool[] NowBtn = new bool[20];
    int SettedPHan = 0;
    int SettedBHan = 0;
    int SettedRHan = 0;
    int SettedSHan = 0;
    static bool[] SettedBtn = new bool[20];
    */
    public void Dispose()
    {
      IsDisposing = true;
      UnmapViewOfFile(pMemory);
      CloseHandle(hSharedMemory);
    }
    public void Load(string settingsPath)
    {
      hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
      pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
      PipeSVStart();
    }
    public void Tick()
    {
      /*
      if (SettedPHan != NowPHan)
      {
        LeverMoved(this, new InputEventArgs(1, NowPHan));
        SettedPHan = NowPHan;
      }
      if (SettedBHan != NowBHan)
      {
        LeverMoved(this, new InputEventArgs(2, NowBHan));
        SettedBHan = NowBHan;
      }
      if (SettedRHan != NowRHan)
      {
        LeverMoved(this, new InputEventArgs(0, NowRHan));
        SettedRHan = NowRHan;
      }
      if (SettedSHan != NowSHan)
      {
        LeverMoved(this, new InputEventArgs(3, NowSHan));
        SettedSHan = NowSHan;
      }
      */
    }
    public void SetAxisRanges(int[][] ranges)
    {
    }

    static bool IsDisposing = false;
    Thread PipeConnectWaitThread = new Thread(new ThreadStart(PipeCommunication));
    //static List<NamedPipeServerStream> BIDSPipeList = new List<NamedPipeServerStream>();
    //NamedPipeServerStream BIDSPipe = new NamedPipeServerStream("BIDSPipe", PipeDirection.InOut, 8);
    static int PipeClientCount = 0;



    private void PipeSVStart()
    {
      ErrorCallArray[1] = 13;
      ErrorCallArray[30] = 0xFE;
      ErrorCallArray[31] = 0xFE;
      PipeConnectWaitThread.Start();
    }
    /*
    private void PipeSVEnd()
    {
      IsDisposing = true;
      Thread DisposeThread = new Thread(() =>
      {
        for(int i = 0; i < 5; i++)
        {
          if (PipeConnectWaitThread.IsAlive) Thread.Sleep(1000);//スレッドが生きてる間は待ってあげる
        }
        if (PipeConnectWaitThread.IsAlive) PipeConnectWaitThread.Abort();//5秒経っても生きてたら殺す
      });
      DisposeThread.Start();
      while (DisposeThread.IsAlive) Thread.Sleep(1000) ;//1秒ごとに、スレッドを殺し終えたか確認
    }
    */
    private static byte[] ErrorCallArray = new byte[32];
    private static void PipeCommunication()
    {
      while (PipeClientCount >= 8 || !IsDisposing) Thread.Sleep(500);
      if (IsDisposing) return;
      using (var ns = new NamedPipeServerStream("BIDSPipe", PipeDirection.InOut, 8))
      {
        bool IsErrorThrown = false;
        new Thread(() =>
        {
          try
          {
            ns.WaitForConnection();
          }
          catch (ObjectDisposedException) {/*パイプ終了通知*/}
          catch (InvalidOperationException) {/*既にパイプに接続されている or パイプハンドルの設定がない*/ }
          catch (Exception e)
          {
            MessageBox.Show("接続待機エラー\n" + e.Message, "BIDSid Error Thrown");
            ns.Close();
            IsErrorThrown = true;
          }
        }).Start();
        while (!ns.IsConnected && !IsErrorThrown) Thread.Sleep(50);
        PipeClientCount++;

        
        new Thread(()=> {
          BIDSSharedMemoryData bsmdr = new BIDSSharedMemoryData();
          while (!IsDisposing && ns.IsConnected)
          {
            byte[] ReadByte = new byte[32];
            if (ns.CanRead)
            {
              try
              {
                ns.Read(ReadByte, 0, 32);
              }
              
              catch (ObjectDisposedException) {/*Pipe is Closed*/}
              catch (InvalidOperationException) {/*切断済 or 接続待 or ハンドル未設定*/}
              
              catch (Exception e)
              {
                MessageBox.Show("パイプ読み取り処理 エラー\n" + e.Message, "BIDSid Error Thrown");
              }
              byte[] ReturnArray = new byte[32];
              if (ReadByte.Skip(30).ToArray() == new byte[2] { 0xFE, 0xFE })
              {
                switch (Convert.ToInt16(ReadByte.Take(2)))
                {
                  case 10://Version
                    ReturnArray = ReadByte;
                    break;
                  case 11://Start
                    ReturnArray = ReadByte;
                    break;
                  case 12://End
                    ReturnArray = ReadByte;
                    break;
                  case 13://Error Call
                    MessageBox.Show("パイプ読み取り処理 通知\nError Callを受信しました。該当するパイプを終了します。", "BIDSid Caption");
                    ns.Close();
                    break;
                  case 14://State
                    ReturnArray = ReturnSpecByte();
                    break;
                  case 15://State
                    ReturnArray = ReturnStateByte();
                    break;
                  case 16://State2
                    ReturnArray = ReturnState2Byte();
                    break;
                  case 20://Sound
                    ReturnArray = ReadByte;
                    bsmdr = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
                    for (int i = 0; i < 7; i++)
                    {
                      short ind = Convert.ToInt16(ReadByte.Skip(2 + (4 * i)).Take(2).ToArray());
                      if (ind >= 0 && ind < 256) Array.Copy(BitConverter.GetBytes((short)bsmdr.Sound[ind]), 0, ReturnArray, 4 + (4 * i), 2);
                    }
                    break;
                  case 21://Panel
                    ReturnArray = ReadByte;
                    bsmdr = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
                    for (int i = 0; i < 7; i++)
                    {
                      short ind = Convert.ToInt16(ReadByte.Skip(2 + (4 * i)).Take(2).ToArray());
                      if (ind >= 0 && ind < 256) Array.Copy(BitConverter.GetBytes((short)bsmdr.Panel[ind]), 0, ReturnArray, 4 + (4 * i), 2);
                    }
                    break;
                  default:
                    ReturnArray = ErrorCallArray;
                    break;
                }
              }
              else
              {
                ReturnArray = ErrorCallArray;
              }
              if (ns.IsConnected)
              {
                try
                {
                  ns.Write(ReturnArray, 0, 32);
                }
                catch (Exception e)
                {
                  MessageBox.Show("Pipe 情報返答処理 エラー\n" + e.Message, "BIDSid Error Thrown");
                }
              }
            }
          }
        }).Start();//ReadLoop

        new Thread(()=> {
          BIDSSharedMemoryData bsmdw = new BIDSSharedMemoryData();
          BIDSSharedMemoryData bsmdwl = new BIDSSharedMemoryData();
          while (!IsDisposing && ns.IsConnected)
          {
            List<byte[]> WriteDataList = new List<byte[]>();
            bsmdw = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
            if ((object)bsmdw.HandleData != (object)bsmdwl.HandleData || (object)bsmdw.StateData != (object)bsmdwl.StateData)
            {
              WriteDataList.Add(ReturnStateByte());
            }
            if (bsmdw.IsDoorClosed != bsmdwl.IsDoorClosed || (object)bsmdw.StateData != (object)bsmdwl.StateData)
            {
              WriteDataList.Add(ReturnState2Byte());
            }
            if (bsmdw.Panel != bsmdwl.Panel)
            {
              List<int> ChangedP = new List<int>();
              for (int i = 0; i < 256; i++) if (bsmdw.Panel[i] != bsmdwl.Panel[i]) ChangedP.Add(i);
              byte[] rby = new byte[32];
              for (int i = 0; i < ChangedP.Count; i++)
              {
                if (i % 7 == 0)
                {
                  rby = Enumerable.Repeat(Convert.ToByte(-1), 32).ToArray();
                  Array.Copy(BitConverter.GetBytes((short)21), 0, rby, 0, 2);
                  Array.Copy(new byte[2] { 0xFE, 0xFE }, 0, rby, 30, 2);
                }
                Array.Copy(BitConverter.GetBytes((short)ChangedP[i]), 0, rby, 2 + (4 * (i % 7)), 2);
                Array.Copy(BitConverter.GetBytes((short)bsmdw.Panel[ChangedP[i]]), 0, rby, 4 + (4 * (i % 7)), 2);
                if (i % 7 == 6) WriteDataList.Add(rby);
              }
              if (WriteDataList.Last() != rby) WriteDataList.Add(rby);
            }
            if (bsmdw.Sound != bsmdwl.Sound)
            {
              List<int> ChangedP = new List<int>();
              for (int i = 0; i < 256; i++) if (bsmdw.Sound[i] != bsmdwl.Sound[i]) ChangedP.Add(i);
              byte[] rby = new byte[32];
              for (int i = 0; i < ChangedP.Count; i++)
              {
                if (i % 7 == 0)
                {
                  rby = Enumerable.Repeat(Convert.ToByte(-1), 32).ToArray();
                  Array.Copy(BitConverter.GetBytes((short)20), 0, rby, 0, 2);
                  Array.Copy(new byte[2] { 0xFE, 0xFE }, 0, rby, 30, 2);
                }
                Array.Copy(BitConverter.GetBytes((short)ChangedP[i]), 0, rby, 2 + (4 * (i % 7)), 2);
                Array.Copy(BitConverter.GetBytes((short)bsmdw.Sound[ChangedP[i]]), 0, rby, 4 + (4 * (i % 7)), 2);
                if (i % 7 == 6) WriteDataList.Add(rby);
              }
              if (WriteDataList.Last() != rby) WriteDataList.Add(rby);
            }
            if ((object)bsmdw.SpecData != (object)bsmdwl.SpecData)
            {
              WriteDataList.Add(ReturnSpecByte());
            }
            //Pipe書き込み処理
            if (WriteDataList.Count > 0)
            {
              for(int i = 0; i < WriteDataList.Count; i++)
              {
                if (ns.CanWrite)
                {
                  try
                  {
                    ns.Write(WriteDataList[i], 0, 32);
                  }
                  catch (Exception e)
                  {
                    MessageBox.Show("Pipe書き込み処理エラー\n" + e.Message, "BIDSid Error Thrown");
                  }
                }
              }
            }
            bsmdwl = bsmdw;
            Thread.Sleep(5);
          }
        }).Start();//WriteLoop
        if (!IsDisposing)
        {
          new Thread(PipeCommunication).Start();
        }
        while (ns.IsConnected || !IsDisposing) Thread.Sleep(200);
        if (ns.IsConnected) ns.Close();
        PipeClientCount--;
      }
    }
    static private byte[] ReturnSpecByte()
    {
      BIDSSharedMemoryData bsmdw = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
      byte[] rby = new byte[32];
      Array.Copy(BitConverter.GetBytes((short)14), 0, rby, 0, 2);
      Array.Copy(BitConverter.GetBytes((short)bsmdw.SpecData.B), 0, rby, 2, 2);
      Array.Copy(BitConverter.GetBytes((short)bsmdw.SpecData.P), 0, rby, 4, 2);
      Array.Copy(BitConverter.GetBytes((short)bsmdw.SpecData.A), 0, rby, 6, 2);
      Array.Copy(BitConverter.GetBytes((short)bsmdw.SpecData.J), 0, rby, 8, 2);
      rby[10] = (byte)bsmdw.SpecData.C;
      Array.Copy(new byte[2] { 0xFE, 0xFE }, 0, rby, 30, 2);
      return rby;
    }
    static private byte[] ReturnStateByte()
    {
      BIDSSharedMemoryData bsmdw = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
      byte[] rby = new byte[32];
      Array.Copy(BitConverter.GetBytes((short)15), 0, rby, 0, 2);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.X), 0, rby, 2, 8);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.V), 0, rby, 10, 4);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.I), 0, rby, 14, 4);
      Array.Copy(BitConverter.GetBytes((short)bsmdw.HandleData.B), 0, rby, 22, 2);
      Array.Copy(BitConverter.GetBytes((short)bsmdw.HandleData.P), 0, rby, 24, 2);
      rby[26] = (byte)(sbyte)bsmdw.HandleData.R;
      rby[27] = (byte)bsmdw.HandleData.C;
      Array.Copy(new byte[2] { 0xFE, 0xFE }, 0, rby, 30, 2);
      return rby;
    }
    static private byte[] ReturnState2Byte()
    {
      BIDSSharedMemoryData bsmdw = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
      byte[] ReturnArray = new byte[32];
      Array.Copy(BitConverter.GetBytes((short)16), 0, ReturnArray, 0, 2);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.BC), 0, ReturnArray, 2, 4);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.MR), 0, ReturnArray, 6, 4);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.ER), 0, ReturnArray, 10, 4);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.BP), 0, ReturnArray, 14, 4);
      Array.Copy(BitConverter.GetBytes(bsmdw.StateData.SAP), 0, ReturnArray, 18, 4);
      Array.Copy(BitConverter.GetBytes(bsmdw.IsDoorClosed), 0, ReturnArray, 22, 1);
      TimeSpan ts = new TimeSpan();
      ts = TimeSpan.FromMilliseconds(bsmdw.StateData.T);
      ReturnArray[25] = (byte)ts.Hours;
      ReturnArray[26] = (byte)ts.Minutes;
      ReturnArray[27] = (byte)ts.Seconds;
      Array.Copy(BitConverter.GetBytes((short)ts.Milliseconds), 0, ReturnArray, 28, 2);
      Array.Copy(new byte[2] { 0xFE, 0xFE }, 0, ReturnArray, 30, 2);

      return ReturnArray;
    }



  }




}
