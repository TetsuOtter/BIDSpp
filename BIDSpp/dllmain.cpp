// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "stdafx.h"
#include "ATSPI.h"
#include <iostream>
#include <string>
#include <stdio.h>
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

struct PipeInfoData
{
	HANDLE hde;
	int Tnum;
};

//常にバックでReadを回しておく

//パイプ名を登録
//パイプの接続待機(Loop For 16 times)
//Read Thread / Write Threadを建てる
//常時Readを待機 Headder.11で必要な情報をTF受信
//上記Headder.11でTとなった情報のみ、情報の変更を監視して送信。
//Headder.12が来たらDisconnect. 接続待機に戻る。
//Dispose()が呼ばれたら全パイプにHeadder.12を投げて閉じる。



Hand H;
//パイプのハンドル。一応成功。
//HANDLE PHandleSC;
HANDLE PHandleCS;
HANDLE PThreadHSC;
HANDLE PThreadHCS;
bool PWriteThreadUseTF[16];
HANDLE PWriteThread[16];
bool PReadThreadUseTF[16];
HANDLE PReadThread[16];
LPDWORD NumOfByteWritten;
BYTE SendData[32];
BYTE SpecD[32];//14
BYTE StateD[32];//15
BYTE State2D[32];//16
BYTE SpecDO[32];
BYTE StateDO[32];
BYTE State2DO[32];

DWORD WINAPI PipeDoSC(LPVOID);

void ElapDo() {
	StateD[0] = 0x10;
}
bool IsDisposed = false;
bool IsPipeCloseComp = false;
bool PipeConnection = false;

//参考 : http://eternalwindows.jp/ipc/namedpipe/namedpipe03.html (マルチclientパイプについて)
//参考 : http://goodfuture.candypop.jp/nifty/hidouki.htm (overlapの扱い方について)


//SCパイプの確立を行う。
DWORD WINAPI PipeOpenSC(LPVOID lpV){
	//SCパイプの確立を待つ。これは繰り返し行う。
	//確立はオーバーラップを伴う。そうしないと終わらせることができない。
	//パイプを確立したら新たにスレッドを建てて、引数にハンドルを渡す。
	//新たなスレッドではデータ更新boolの監視とそれに伴う送信を行う。
	//IsDisposedがtrueで終了。
	HANDLE PHandleSC;
	OVERLAPPED OLD;
	MessageBox(NULL, "WINAPI Test Msg\nPipeOpenSC Starded", "BIDSpp.dll", MB_OK);
	while (IsDisposed == false) {
		PHandleSC = CreateNamedPipe("\\\\.\\pipe\\BIDSPipeSC", PIPE_ACCESS_OUTBOUND, PIPE_TYPE_BYTE, 1, 32, 32, 500, NULL);
		ZeroMemory(&OLD, sizeof(OVERLAPPED));
		ConnectNamedPipe(PHandleSC, &OLD);
		int i = 5;
		while (HasOverlappedIoCompleted(&OLD) == false && IsDisposed == false)
		{
			Sleep(i);
			if (i < 5000) {
				i++;
			}
		}
		MessageBox(NULL, "WINAPI Test Msg\nPipeOpenSC Thread WaitLoop End", "BIDSpp.dll", MB_OK);
		if (HasOverlappedIoCompleted(&OLD)) {
			for (int i = 0; i < 16; i++) {
				if (!PWriteThreadUseTF[i]) {
					PWriteThread[i] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)PipeDoSC, PHandleSC, 0, NULL);
					PWriteThreadUseTF[i] = true;
					break;
				}
			}
		}
	}
	MessageBox(NULL, "WINAPI Test Msg\nPipeOpenSC Thread Close Start", "BIDSpp.dll", MB_OK);
	CloseHandle(PHandleSC);
	for (int i = 0; i < 16; i++) {
		if (PWriteThreadUseTF[i]) {
			WaitForSingleObject(PWriteThread[i], INFINITE);
			CloseHandle(PWriteThread[i]);
			PWriteThreadUseTF[i] = false;
		}
	}
	MessageBox(NULL, "WINAPI Test Msg\nPipeOpenSC Thread Closed", "BIDSpp.dll", MB_OK);
	return 0;
}
//CSパイプの確立を行う。
DWORD WINAPI PipeOpenCS(LPVOID lpV) {
	//CSパイプの確立を待つ。これは繰り返し行う。
	//パイプを確立したら新たにスレッドを建てて、引数にハンドルを渡す。
	//確立はオーバーラップを伴う。そうしないと終わらせることができない。
	//新たなスレッドでは受信待機を永遠に行い、何か来たらそれに対応する動作をする。
	//PHandleCS = CreateNamedPipe("\\\\.\\pipe\\BIDSPipeCS", PIPE_ACCESS_INBOUND, PIPE_TYPE_BYTE, 1, 32, 32, 500, NULL);
	Sleep(15000);
	MessageBox(NULL, "WINAPI Test Msg\nPipeOpenCS 15s", "BIDSpp.dll", MB_OK);
	CloseHandle(PHandleCS);
	for (int i = 0; i < 16; i++) {
		if (PReadThread[i] != NULL) {
			WaitForSingleObject(PReadThread[i], INFINITE);
			CloseHandle(PReadThread[i]);
		}
	}
	return 0;
}
//ReWriteNumの確認を行い、今持ってるのと違ったら流す。引数はパイプのハンドル
DWORD WINAPI PipeDoSC(LPVOID lpV) {
	//Elapseなどで書き換えられるReWriteNumを参照・記録
	//もし所持している番号と違えば該当するデータを投げる。
	//
	HANDLE hdle = (HANDLE)lpV;
	int PanelNum = 0;
	int SoundNum = 0;
	long StateNum = 0;
	long State2Num = 0;
	MessageBox(NULL, "WINAPI Test Msg\nPipeDoSC Starded", "BIDSpp.dll", MB_OK);
	while (!IsDisposed)
	{
		Sleep(5);
	}
	DisconnectNamedPipe(hdle);
	CloseHandle(hdle);
	MessageBox(NULL, "WINAPI Test Msg\nPipeDoSC Closed", "BIDSpp.dll", MB_OK);
	return 0;
}
//受信監視を行い、内容によって操作を行う。引数はパイプのハンドル
DWORD WINAPI PipeDoCS(LPVOID lpV) {
	//Read監視を行う。オーバーラップを伴う。
	return 0;
}


DE void Load() {
	MessageBox(NULL, "WINAPI Test Msg\nLoad Start", "BIDSpp.dll", MB_OK);
	PThreadHSC = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)PipeOpenSC, NULL, 0, NULL);
	//PThreadHCS = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)PipeOpenCS, NULL, 0, NULL);
	MessageBox(NULL, "WINAPI Test Msg", "BIDSpp.dll", MB_OK);
}
DE void Dispose() {
	MessageBox(NULL, "WINAPI Test Msg\nDispose Start", "BIDSpp.dll", MB_OK);
	IsDisposed = true;
	WaitForSingleObject(PThreadHSC, INFINITE);
	//WaitForSingleObject(PThreadHCS, INFINITE);
	CloseHandle(PThreadHSC);
	//CloseHandle(PThreadHCS);
	MessageBox(NULL, "WINAPI Test Msg\nDispose Comp", "BIDSpp.dll", MB_OK);

}
DE int GetPluginVersion() {
	MessageBox(NULL, "ver 2.00", "BIDSpp.dll", MB_OK);
	return 0x00020000;
}
DE void SetVehicleSpec(Spec s) {
}
DE void Initialize(int b) {

}
DE Hand Elapse(State VS, int * P, int * S)
{
	//ElapDo();
	H.C = 0;
	return H;
}
DE void SetPower(int p) {
	H.P = p;
}
DE void SetBrake(int b) {
	H.B = b;
}
DE void SetReverser(int r) {
	H.R = r;
}
DE void KeyDown(int k) {

}
DE void KeyUp(int k) {

}
DE void HornBlow(int k) {

}

DE void DoorOpen() {

}
DE void DoorClose() {

}
DE void SetSignal(int a) {

}
DE void SetBeaconData(Beacon b) {

}

