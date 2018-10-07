//Headder for ATS Plugin
//Tetsu Otter
//DLL exportを短縮
#ifdef BIDSPP_EXPORTS
#define DE __declspec(dllexport) 
#else
#define DE __declspec(dllimport)
#endif
struct Spec{
	int B = 0;	//ブレーキ段数
	int P = 0;	//ノッチ段数
	int A = 0;	//ATS確認段数
	int J = 0;	//常用最大段数
	int C = 0;	//編成車両数
};
struct State{
	double X = 0;	//列車位置[m]
	float V = 0;	//列車速度[km/h]
	int T = 0;		//0時からの経過時間[ms]
	float BC = 0;	//BC圧力[kPa]
	float MR = 0;	//MR圧力[kPa]
	float ER = 0;	//ER圧力[kPa]
	float BP = 0;	//BP圧力[kPa]
	float SAP = 0;	//SAP圧力[kPa]
	float I = 0;	//電流[A]
};
struct Hand{
	int B = 0;	//ブレーキハンドル位置
	int P = 0;	//ノッチハンドル位置
	int R = 0;	//レバーサーハンドル位置
	int C = 0;	//定速制御状態
};
struct Beacon{
	int Num = 0;	//Beaconの番号
	int Sig = 0;	//対応する閉塞の現示番号
	float X = 0;	//対応する閉塞までの距離[m]
	int Data = 0;	//Beaconの第三引数の値
};
DE void __stdcall Load(void);
DE void __stdcall Dispose(void);
DE int __stdcall GetPluginVersion(void);
DE void __stdcall SetVehicleSpec(Spec);
DE void __stdcall Initialize(int);
DE Hand __stdcall Elapse(State, int *, int *);
DE void __stdcall SetPower(int);
DE void __stdcall SetBrake(int);
DE void __stdcall SetReverser(int);
DE void __stdcall KeyDown(int);
DE void __stdcall KeyUp(int);
DE void __stdcall HornBlow(int);
DE void __stdcall DoorOpen(void);
DE void __stdcall DoorClose(void);
DE void __stdcall SetSignal(int);
DE void __stdcall SetBeaconData(Beacon);