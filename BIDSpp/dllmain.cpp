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
	Beacon BeaconData[16];
	bool IsKeysPushed[16];
	bool IsDoorClosed;
	int SignalSetInt;
};
auto name = "BIDSSharedMem";
auto size = sizeof(BIDSSharedMemoryData);
BIDSSharedMemoryData BSMD;
HANDLE hSharedMemory = CreateFileMapping(NULL, NULL, PAGE_READWRITE, NULL, size, name);
auto pMemory = (BIDSSharedMemoryData*)MapViewOfFile(hSharedMemory, FILE_MAP_ALL_ACCESS, NULL, NULL, size);
void DataWrite() {
	if (pMemory->IsEnabled || pMemory->VersionNum == 100) {
		MessageBox(NULL, "SharedMem Is Enabled", "BIDSpp.dll", MB_OK);//有効
		*pMemory = BSMD;
	}
	else
	{
		MessageBox(NULL, "SharedMem Is Disabled", "BIDSpp.dll", MB_OK);//無効
	}
}
DE void Load() {
	MessageBox(NULL, "WINAPI Test Msg", "BIDSpp.dll", MB_OK);
	DataWrite();
	*pMemory = BSMD;
}
DE void Dispose() {
	UnmapViewOfFile(pMemory);
	CloseHandle(hSharedMemory);
}
DE int GetPluginVersion() {
	//MessageBox(NULL, "ver 2.00", "BIDSpp.dll", MB_OK);
	return 0x00020000;
}
DE void SetVehicleSpec(Spec s) {
}
DE void Initialize(int b) {

}
DE Hand Elapse(State VS, int * P, int * S)
{
	//ElapDo();
	BSMD.HandleData.C = 0;
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
	BSMD.IsKeysPushed[k] = true;
}
DE void KeyUp(int k) {
	BSMD.IsKeysPushed[k] = false;
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
	BSMD.SignalSetInt = a;
}
DE void SetBeaconData(Beacon b) {
	for (int i = 16 - 1; i > 0; i--)
	{
		BSMD.BeaconData[i + i] = BSMD.BeaconData[i];
	}
	BSMD.BeaconData[0] = b;
}

