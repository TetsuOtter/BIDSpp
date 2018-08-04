//Headder for ATS Plugin
//Tetsu Otter

//DLL export��Z�k
#define DE __declspec(dllexport)
struct Spec{
	int B;	//�u���[�L�i��
	int P;	//�m�b�`�i��
	int A;	//ATS�m�F�i��
	int J;	//��p�ő�i��
	int C;	//�Ґ��ԗ���
};
struct State{
	double X;	//��Ԉʒu[m]
	float V;	//��ԑ��x[km/h]
	int T;		//0������̌o�ߎ���[ms]
	float BC;	//BC����[kPa]
	float MR;	//MR����[kPa]
	float ER;	//ER����[kPa]
	float BP;	//BP����[kPa]
	float SAP;	//SAP����[kPa]
	float I;	//�d��[A]
};
struct Hand{
	int B;	//�u���[�L�n���h���ʒu
	int P;	//�m�b�`�n���h���ʒu
	int R;	//���o�[�T�[�n���h���ʒu
	int C;	//�葬������
};
struct Beacon{
	int Num;	//Beacon�̔ԍ�
	int Sig;	//�Ή�����ǂ̌����ԍ�
	float X;	//�Ή�����ǂ܂ł̋���[m]
	int Data;	//Beacon�̑�O�����̒l
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