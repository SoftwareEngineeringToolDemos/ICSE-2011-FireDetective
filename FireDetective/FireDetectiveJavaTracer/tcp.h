#pragma once

#include "exception.h"
#include "stream.h"

class SocketException : public Exception
{
public:
	SocketException(std::string message);
	SocketException::SocketException(std::string message, bool skipLastError);
};

class NetworkStream : public IByteStream
{
public:
	NetworkStream(SOCKET socket);
	virtual void ReadBytes(char *buffer, int bytesToRead);
	virtual void WriteBytes(const char *buffer, int bytesToWrite);

private:
	SOCKET m_Socket;	
};

class TcpClient
{
public:
	TcpClient(SOCKET socket);
	~TcpClient();
	NetworkStream *GetNetworkStream();
	void Close();

private:
	SOCKET m_Socket;
};

class TcpListener
{
public:
	TcpListener(int localPort);
	~TcpListener();
	void Start(int connections);
	TcpClient *AcceptTcpClient();	
	void Close();

private:
	SOCKET m_Socket;
};

