using System;
using System.Runtime.InteropServices;
using System.Threading;

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
    //SECTION_ALL_ACCESS=983071
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

    [DllImport("kernel32.dll")]
    static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

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

    public void Test()
    {
      string SRAMName = "BIDSSharedMem";
      uint size = 0;
      //Console.WriteLine(SECTION_ALL_ACCESS);
      size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
      //string name = "hoge";
      //uint size = 4;
      //Console.WriteLine("BIDSSharedMemoryData Size");
      //Console.WriteLine(size);
      //Console.WriteLine(sizeof(BIDSSharedMemoryDataC));
      IntPtr SizeSM = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, 4, "BIDSSharedMemSize");
      IntPtr SizeSMMap = MapViewOfFile(SizeSM, 983071, 0, 0, 4);
      //size = (uint)(int)Marshal.PtrToStructure(SizeSMMap, typeof(int));
      //Console.WriteLine(size);
      IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
      IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
      BIDSSharedMemoryData bmd = new BIDSSharedMemoryData();

      for (int i = 0; i < 10; i++)
      {
        bmd = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
        Console.WriteLine(bmd.StateData.T);
        //Console.WriteLine(Marshal.PtrToStructure(SizeSMMap, typeof(int)));
        //Console.WriteLine(bmd.ToString());
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
