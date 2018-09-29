
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Mackoy.Bvets;


namespace BIDSid
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class SettingWindow : Window
  {
    public SettingWindow()
    {

    }
  }

  class Program
  {
    static void Main(string[] Args)
    {
      Console.WriteLine("Hello World!");
    }
  }


  public class BIDSid : IInputDevice
  {
    public event InputEventHandler LeverMoved;
    public event InputEventHandler KeyDown;
    public event InputEventHandler KeyUp;

    public void Configure(System.Windows.Forms.IWin32Window owner)
    {
      var sw = new SettingWindow();
      sw.Show();
    }

    private static string SRAMName = "BIDSSharedMem";
    //private MemoryMappedFile MMFile;
    //private MemoryMappedViewAccessor MMVAccessor;


    /// <summary>
    /// 指定されたFileに対する、NamedまたはNamelessのFileMappingObjectをMake or Open。
    /// </summary>
    /// <param name="hFile">FileのHandle</param>
    /// <param name="lpFileMappingAttributes">GetしたHandleをCoProcessへ継承することをPermit or Notを決定する、1個の 構造体へのIntPtr</param>
    /// <param name="flProtect">FileをMapする際に、FileViewに割り当てられる保護属性</param>
    /// <param name="dwMaximumSizeHigh">File Mapping ObjectのMax Sizeの上位</param>
    /// <param name="dwMaximumSizeLow">File Mapping ObjectのMax Sizeの下位</param>
    /// <param name="lpName">File Mapping Objectの名前を保持している、NULL で終わる文字列</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
    /// <summary>
    /// FileのViewを呼び出し側Processのアドレス空間にMapする
    /// </summary>
    /// <param name="hFileMappingObject">FileMappingObjectへのOpenHandleのHandle</param>
    /// <param name="dwDesiredAccess">File Mapping ObjectへのAccess Typeと、FileによりMapされたPageのページ保護</param>
    /// <param name="dwFileOffsetHigh">Mappingを開始する上位32bitのFile Offset</param>
    /// <param name="dwFileOffsetLow">Mappingを開始する下位32bitのFile Offset</param>
    /// <param name="dwNumberOfBytesToMap">MapするFileのByte数を指定します。0を指定すると、File全体がMapされます。</param>
    /// <returns>ポインタ</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

    [DllImport("kernel32.dll")]
    static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);
    public struct Spec
    {
      int B;  //ブレーキ段数
      int P;  //ノッチ段数
      int A;  //ATS確認段数
      int J;  //常用最大段数
      int C;  //編成車両数
    };
    public struct State
    {
      double X; //列車位置[m]
      float V;  //列車速度[km/h]
      int T;    //0時からの経過時間[ms]
      float BC; //BC圧力[kPa]
      float MR; //MR圧力[kPa]
      float ER; //ER圧力[kPa]
      float BP; //BP圧力[kPa]
      float SAP;  //SAP圧力[kPa]
      float I;  //電流[A]
    };
    public struct Hand
    {
      int B;  //ブレーキハンドル位置
      int P;  //ノッチハンドル位置
      int R;  //レバーサーハンドル位置
      int C;  //定速制御状態
    };
    public struct Beacon
    {
      int Num;  //Beaconの番号
      int Sig;  //対応する閉塞の現示番号
      float X;  //対応する閉塞までの距離[m]
      int Data; //Beaconの第三引数の値
    };

    //参考 : https://qiita.com/hmuronaka/items/619f8889e36c7b5db92d
    public struct BIDSSharedMemoryData
    {
      public bool IsEnabled;
      public int VersionNum;
      public Spec SpecData;
      public State StateData;
      public Hand HandleData;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public Beacon[] BeaconData;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public bool[] IsKeysPushed;
      public bool IsDoorClosed;
      public int SignalSetInt;
    };
    static uint size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));

    private static IntPtr hCFM = CreateFileMapping((UIntPtr)0, (IntPtr)0, 0x04, 0, size, SRAMName);
    private IntPtr MVOF = MapViewOfFile(hCFM, (uint)((0x000F0000L) | 0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x0010), 0, 0, size);
    public void Dispose()
    {
      //MMVAccessor.Dispose();
      //MMFile.Dispose();
    }
    BIDSSharedMemoryData BSMD = new BIDSSharedMemoryData();
    public void Load(string settingsPath)
    {
      BSMD.IsEnabled = true;
      Marshal.StructureToPtr(BSMD, MVOF, true);
      //MMFile = MemoryMappedFile.CreateOrOpen(SRAMName, BIDSSharedMemoryData.Size);
      //MMVAccessor = MMFile.CreateViewAccessor();
    }

    public void SetAxisRanges(int[][] ranges)
    {
    }
    int Count;
    public void Tick()
    {
      if (Count > 100)
      {
        object oj = Marshal.PtrToStructure(MVOF, typeof(BIDSSharedMemoryData));
        BSMD = (BIDSSharedMemoryData)oj;
        //MessageBox.Show("IsEnabled? : " + BSMD.IsEnabled.ToString());
        /*MessageBox.Show("CanRead = "+MMVAccessor.CanRead.ToString());
        Count = 0;
        if (MMVAccessor.CanRead)
        {
          MMVAccessor.Read(0, out BSMD);
          MessageBox.Show(BSMD.VersionNum.ToString());
        }*/
      }
      Count++;
    }
  }

}
