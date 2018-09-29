// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "stdafx.h"
#include "ATSPI.h"
#include <Windows.h>
#include <string.h>
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
HANDLE hSharedMemory = CreateFileMappingA(NULL, NULL, PAGE_READWRITE, NULL, sizes, name);
auto pMemory = (BIDSSharedMemoryData*)MapViewOfFile(hSharedMemory, FILE_MAP_ALL_ACCESS, NULL, NULL, sizes);

DE void Load() {
	pMemory->IsEnabled = true;
	pMemory->VersionNum = 100;
}
DE void Dispose() {
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
	pMemory->HandleData.C = 0;
	pMemory->StateData = VS;
	memcpy(pMemory->Panel, P, sizeof(int) * 256);
	memcpy(pMemory->Sound, S, sizeof(int) * 256);
	return pMemory->HandleData;
}
DE void SetPower(int p) {
	pMemory->HandleData.P = p;
}
DE void SetBrake(int b) {
	pMemory->HandleData.B = b;
}
DE void SetReverser(int r) {
	pMemory->HandleData.R = r;
}
DE void DoorOpen() {
	pMemory->IsDoorClosed = false;
}
DE void DoorClose() {
	pMemory->IsDoorClosed = true;
}


DE void KeyDown(int k) {
}
DE void KeyUp(int k) {
}
DE void HornBlow(int k) {

}
DE void SetSignal(int a) {
}
DE void SetBeaconData(Beacon b) {
}

