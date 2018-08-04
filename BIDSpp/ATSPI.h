//Headder for ATS Plugin
//Tetsu Otter

//DLL exportを短縮
#define DE __declspec(dllexport)
struct Spec{
	int B;	//ブレーキ段数
	int P;	//ノッチ段数
	int A;	//ATS確認段数
	int J;	//常用最大段数
	int C;	//編成車両数
};
struct State{
	double X;	//列車位置[m]
	float V;	//列車速度[km/h]
	int T;		//0時からの経過時間[ms]
	float BC;	//BC圧力[kPa]
	float MR;	//MR圧力[kPa]
	float ER;	//ER圧力[kPa]
	float BP;	//BP圧力[kPa]
	float SAP;	//SAP圧力[kPa]
	float I;	//電流[A]
};
struct Hand{
	int B;	//ブレーキハンドル位置
	int P;	//ノッチハンドル位置
	int R;	//レバーサーハンドル位置
	int C;	//定速制御状態
};
struct Beacon{
	int Num;	//Beaconの番号
	int Sig;	//対応する閉塞の現示番号
	float X;	//対応する閉塞までの距離[m]
	int Data;	//Beaconの第三引数の値
};
DE void Load(void);
DE void Dispose(void);
DE int GetPluginVersion(void);
DE void SetVehicleSpec(Spec);
DE void Initialize(int);
DE Hand Elapse(State, int *, int *);
DE void SetPower(int);
DE void SetBrake(int);
DE void SetReverser(int);
DE void KeyDown(int);
DE void KeyUp(int);
DE void HornBlow(int);
DE void DoorOpen(void);
DE void DoorClose(void);
DE void SetSignal(int);
DE void SetBeaconData(Beacon);