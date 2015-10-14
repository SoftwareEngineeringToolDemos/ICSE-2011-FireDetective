#pragma once

#include "tcp.h"
#include "stream.h"
#include "utils.h"

class TraceConsumerConnection
{
public:
	TraceConsumerConnection(TcpClient *tcpClient);
	void SendMessage(std::string message);
	void ProcessMessages();

private:
	TcpClient *m_tcpClient;
	NetworkStream *m_networkStream;
	StringStream *m_dataStream;
};

class ITraceProviderListener
{
public:
	virtual void TraceProvider_ConsumerConnected(TraceConsumerConnection *consumer) = 0;
	virtual void TraceProvider_ConsumerDisconnected(TraceConsumerConnection *consumer) = 0;
};

class TraceProvider
{
public:
	TraceProvider(ITraceProviderListener *listener);
	void RunListener();

private:
	HANDLE m_hListenerThread;
	TcpListener *m_tcpListener;
	ITraceProviderListener *m_listener;
};

