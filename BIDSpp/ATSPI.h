//Headder for ATS Plugin
//Tetsu Otter
//DLL export��Z�k
#ifdef BIDSPP_EXPORTS
#define DE __declspec(dllexport) 
#else
#define DE __declspec(dllimport)
#endif
struct Spec{
	int B = 0;	//�u���[�L�i��
	int P = 0;	//�m�b�`�i��
	int A = 0;	//ATS�m�F�i��
	int J = 0;	//��p�ő�i��
	int C = 0;	//�Ґ��ԗ���
};
struct State{
	double X = 0;	//��Ԉʒu[m]
	float V = 0;	//��ԑ��x[km/h]
	int T = 0;		//0������̌o�ߎ���[ms]
	float BC = 0;	//BC����[kPa]
	float MR = 0;	//MR����[kPa]
	float ER = 0;	//ER����[kPa]
	float BP = 0;	//BP����[kPa]
	float SAP = 0;	//SAP����[kPa]
	float I = 0;	//�d��[A]
};
struct Hand{
	int B = 0;	//�u���[�L�n���h���ʒu
	int P = 0;	//�m�b�`�n���h���ʒu
	int R = 0;	//���o�[�T�[�n���h���ʒu
	int C = 0;	//�葬������
};
struct Beacon{
	int Num = 0;	//Beacon�̔ԍ�
	int Sig = 0;	//�Ή�����ǂ̌����ԍ�
	float X = 0;	//�Ή�����ǂ܂ł̋���[m]
	int Data = 0;	//Beacon�̑�O�����̒l
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