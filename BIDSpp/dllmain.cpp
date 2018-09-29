// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "stdafx.h"
#include "ATSPI.h"
#include <iostream>
#include <string>
#include <stdio.h>
#include <Windows.h>
BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

/*
InputPI併用案
 予めBIDSid.dllをInputDeviceに登録しておいてもらう
 Load時にBIDSidで作られた共有メモリ「BIDSSharedMem」を作る(読む)
  中身がNULLだったら破棄
 逐一書き込んでいく。<=読まない。
*/


void ElapDo() {
	//StateD[0] = 0x10;
}

struct BIDSSharedMemoryData
{
	bool IsEnabled;
	int VersionNum;
	Spec SpecData;
	State StateData;
	Hand HandleData;
	bool IsDoorClosed;
	int Panel[256];
	int Sound[256];
	//Beacon BeaconData[16];
	//bool IsKeysPushed[16];
	//int SignalSetInt;
};
auto name = "BIDSSharedMem";
auto sizes = sizeof(BIDSSharedMemoryData);
BIDSSharedMemoryData BSMD;
HANDLE hSharedMemory = CreateFileMappingA(NULL, NULL, PAGE_READWRITE, NULL, sizes, name);
auto pMemory = (BIDSSharedMemoryData*)MapViewOfFile(hSharedMemory, FILE_MAP_ALL_ACCESS, NULL, NULL, sizes);
void DataWrite() {
	*pMemory = BSMD;
}

//HANDLE SizeCallHand = CreateFileMappingA(NULL, NULL, PAGE_READWRITE, NULL, 4, "BIDSSharedMemSize");
//auto SizeMemMap = (int*)MapViewOfFile(SizeCallHand, FILE_MAP_ALL_ACCESS, NULL, NULL, 4);
DE void Load() {
	//*SizeMemMap = sizeof(BIDSSharedMemoryData);
	//*pMemory = BSMD;
	BSMD.IsEnabled = true;
	BSMD.VersionNum = 100;
	DataWrite();
}
DE void Dispose() {
	//UnmapViewOfFile(SizeMemMap);
	//CloseHandle(SizeCallHand);
	UnmapViewOfFile(pMemory);
	CloseHandle(hSharedMemory);
}
DE int GetPluginVersion() {
	return 0x00020000;
}
DE void SetVehicleSpec(Spec s)
{
	pMemory->SpecData = s;
}
DE void Initialize(int b) {

}
DE Hand Elapse(State VS, int * P, int * S)
{
	//*SizeMemMap = VS.T;
	//ElapDo();
	BSMD.HandleData.C = 0;
	BSMD.StateData = VS;

	DataWrite();
	return BSMD.HandleData;
}
DE void SetPower(int p) {
	BSMD.HandleData.P = p;
}
DE void SetBrake(int b) {
	BSMD.HandleData.B = b;
}
DE void SetReverser(int r) {
	BSMD.HandleData.R = r;
}
DE void KeyDown(int k) {
	//BSMD.IsKeysPushed[k] = true;
}
DE void KeyUp(int k) {
	//BSMD.IsKeysPushed[k] = false;
	//int t = pMemory->StateData.T;
	//TCHAR timechar[8];
	//wsprintf(timechar, TEXT("%d"), t);
	//MessageBoxA(NULL, timechar, "BIDSpp.dll", MB_OK);
}
DE void HornBlow(int k) {

}

DE void DoorOpen() {
	BSMD.IsDoorClosed = false;
}
DE void DoorClose() {
	BSMD.IsDoorClosed = true;
}
DE void SetSignal(int a) {
	//BSMD.SignalSetInt = a;
}
DE void SetBeaconData(Beacon b) {
	/*for (int i = 16 - 1; i > 0; i--)
	{
		BSMD.BeaconData[i + i] = BSMD.BeaconData[i];
	}
	BSMD.BeaconData[0] = b;*/
}

