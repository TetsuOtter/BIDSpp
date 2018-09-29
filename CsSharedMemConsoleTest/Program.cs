using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsSharedMemConsoleTest
{
  class Program
  {
    static void Main(string[] args)
    {
      SharedMemoryMgr smm = new SharedMemoryMgr();
      smm.Test();
    }
  }

  public class SharedMemoryMgr
  {
    ////////////////////////////////////////////////////// 
    // 使用するAPIをC#用にマーシャリングして再定義
    // http://www.pinvoke.net/index.aspx 
    //////////////////////////////////////////////////////

    //==========================
    // アクセス制限設定等
    //==========================
    const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
    const uint SECTION_QUERY = 0x0001;
    const uint SECTION_MAP_WRITE = 0x0002;
    const uint SECTION_MAP_READ = 0x0004;
    const uint SECTION_MAP_EXECUTE = 0x0008;
    const uint SECTION_EXTEND_SIZE = 0x0010;
    const uint SECTION_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SECTION_QUERY | SECTION_MAP_WRITE | SECTION_MAP_READ | SECTION_MAP_EXECUTE | SECTION_EXTEND_SIZE);
    const uint FILE_MAP_ALL_ACCESS = SECTION_ALL_ACCESS;
    const uint PAGE_READWRITE = 4;

    //==========================
    // DLLImport
    //==========================
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile,
    IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh,
    uint dwMaximumSizeLow, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint
    dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,
    uint dwNumberOfBytesToMap);

    [DllImport("kernel32.dll")]
    static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

    [DllImport("kernel32.dll")]
    static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);

    ////////////////////////////////////////////////
    // 定数
    ////////////////////////////////////////////////
    //すでに共有メモリ作成済の場合のエラー値 
    const int ERROR_ALREADY_EXISTS = 183;

    ////////////////////////////////////////////////
    // 構造体
    ////////////////////////////////////////////////
    //テスト用の共有データ型 
    public struct SharedData
    {
      public int m_iData1;
      public int m_iData2;
      public int m_iData3;
    }

    ////////////////////////////////////////////////
    // メンバ変数
    ////////////////////////////////////////////////
    //共有メモリ確保用のハンドルとポインタを定義 
    private IntPtr m_memAreaHandle = IntPtr.Zero;
    private IntPtr m_memAreaPointer = IntPtr.Zero;

    //共有メモリへのアクセス用排他ミューテックス
    private Mutex m_Mutex;

    ////////////////////////////////////////////////
    // メンバ関数
    ////////////////////////////////////////////////
    //==========================
    // コンストラクタ
    //==========================
    public SharedMemoryMgr()
    {
      // ミューテックスを作成
      m_Mutex = new Mutex(false, "ComonMemMutex");
    }

    //==========================
    // デストラクタ
    //==========================
    ~SharedMemoryMgr()
    {
      if (m_memAreaPointer != IntPtr.Zero)
      {
        UnmapViewOfFile(m_memAreaPointer);
        m_memAreaPointer = IntPtr.Zero;
      }

      if (m_memAreaHandle != IntPtr.Zero)
      {
        CloseHandle(m_memAreaHandle);
        m_memAreaHandle = IntPtr.Zero;
      }
    }

    //=====================================
    // 共有メモリ作成
    // 戻り値　true:成功 false:失敗
    //=====================================
    public unsafe bool CreateMemory()
    {
      try
      {
        m_memAreaHandle = CreateFileMapping((UIntPtr)0xFFFFFFFF, // ファイルハンドル 
        IntPtr.Zero, // セキュリティ属性 
        PAGE_READWRITE, // 保護属性(R/W) 
        0, // サイズ上位 
        1024, // サイズ下位 
        "CommonMem"); // オブジェクト名

        // マッピング処理
        m_memAreaPointer = MapViewOfFile(m_memAreaHandle, SECTION_ALL_ACCESS, 0, 0, 0);

        int retval = Marshal.GetLastWin32Error();
        if (retval != ERROR_ALREADY_EXISTS)
        {
          //エリア初期化処理など
        }
      }
      catch (Exception ex)
      {
        ex.ToString();
      }
      return false;
    }

    //=====================================
    // 共有メモリに文字列を書込む
    // <Param> 
    // strText 文字列
    // <Return> true:成功 false:失敗
    //=====================================
    public void WriteMemory(string strText)
    {
      //排他制御　ミューテックス取得 
      m_Mutex.WaitOne(Timeout.Infinite, true);

      // stringをIntPtrへ変換　※この時メモリを確保するので、後でFreeCoTaskMemすること！！
      IntPtr ptrTemp = Marshal.StringToHGlobalUni(strText);
      CopyMemory(m_memAreaPointer, ptrTemp, (uint)(((int)strText.Length) * Marshal.SizeOf(typeof(IntPtr))));
      Marshal.FreeCoTaskMem(ptrTemp);

      //ミューテックス解放
      m_Mutex.ReleaseMutex();
    }

    //=====================================
    // 共有メモリより文字列を読込む
    // <Return> 文字列
    //=====================================
    public string ReadMemory()
    {
      string strText;
      //排他制御　ミューテックス取得 
      m_Mutex.WaitOne(Timeout.Infinite, true);
      //共有メモリのunmanagedメモリよりmanagedデータへ変換して文字列へコピー
      strText = Marshal.PtrToStringUni(m_memAreaPointer);
      //ミューテックス解放
      m_Mutex.ReleaseMutex();
      return strText;
    }

    //=====================================
    // 共有メモリに構造体データを書込む
    // <Param> 
    // strText 構造体データ
    //=====================================
    public void WriteMemoryData(SharedData SharedData)
    {
      //排他制御　ミューテックス取得 
      m_Mutex.WaitOne(Timeout.Infinite, true);
      Marshal.StructureToPtr(SharedData, m_memAreaPointer, true);
      //ミューテックス解放
      m_Mutex.ReleaseMutex();
    }

    //=====================================
    // 共有メモリより構造体データを読込む
    // <Param> 
    // strText 構造体データ参照
    //=====================================
    public void ReadMemoryData(out SharedData SharedData)
    {
      //排他制御　ミューテックス取得 
      m_Mutex.WaitOne(Timeout.Infinite, true);
      object obj = Marshal.PtrToStructure(m_memAreaPointer, typeof(SharedData));
      //ミューテックス解放
      m_Mutex.ReleaseMutex();

      SharedData = (SharedData)obj;
    }
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

    public void Test()
    {
      string SRAMName = "BIDSSharedMem";
      uint size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
      //string name = "hoge";
      //uint size = 4;

      IntPtr SizeSM = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, PAGE_READWRITE, 0, 4, "BIDSSharedMemSize");
      IntPtr SizeSMMap = MapViewOfFile(SizeSM, FILE_MAP_ALL_ACCESS, 0, 0, 4);
      size = (uint)(int)Marshal.PtrToStructure(SizeSMMap, typeof(int));

      IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, PAGE_READWRITE, 0, size, SRAMName);
      IntPtr pMemory = MapViewOfFile(hSharedMemory, FILE_MAP_ALL_ACCESS, 0, 0, size);
      BIDSSharedMemoryData bmd = new BIDSSharedMemoryData();

      for (int i = 0; i < 10; i++)
      {
        bmd = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
        Console.WriteLine(bmd.StateData.T);
        
        //cout << *pMemory << endl;
        Thread.Sleep(1000);
      }

      UnmapViewOfFile(SizeSMMap);
      CloseHandle(SizeSM);
      CloseHandle(hSharedMemory);
      UnmapViewOfFile(pMemory);
      CloseHandle(hSharedMemory);
    }
  }

}
