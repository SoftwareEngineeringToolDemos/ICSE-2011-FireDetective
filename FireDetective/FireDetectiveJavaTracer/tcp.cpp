#include "stdafx.h"

#include "tcp.h"

std::string GetSocketErrorCodeSuffix()
{
	std::ostringstream out;
	out << " Socket error " << WSAGetLastError() << ".";
	return out.str();
}

SocketException::SocketException(std::string message) 
	: Exception(message + GetSocketErrorCodeSuffix())
{
}

SocketException::SocketException(std::string message, bool skipLastError)
	: Exception(message + (skipLastError ? std::string("") : GetSocketErrorCodeSuffix()))
{
}

NetworkStream::NetworkStream(SOCKET socket)
	: m_Socket(socket)
{
}

void NetworkStream::ReadBytes(char *buffer, int bytesToRead)
{
	int toRead = bytesToRead;
	char *p = buffer;
	while (toRead > 0)
	{
		int result = recv(m_Socket, p, toRead, 0);
		if (result == SOCKET_ERROR)
			throw SocketException("recv failed.");
		p += result;
		toRead -= result;
	}
}

void NetworkStream::WriteBytes(const char *buffer, int bytesToWrite)
{
	int result = send(m_Socket, buffer, bytesToWrite, 0);
	if (result == SOCKET_ERROR)
		throw SocketException("send failed.");
}

TcpClient::TcpClient(SOCKET socket)
	: m_Socket(socket)
{	
}

TcpClient::~TcpClient()
{
	Close();
}

NetworkStream *TcpClient::GetNetworkStream()
{
	return new NetworkStream(m_Socket);
}

void TcpClient::Close()
{
	if (m_Socket != NULL)
	{
		closesocket(m_Socket);
		m_Socket = NULL;
	}
}

TcpListener::TcpListener(int localPort)
{
	// Initialize winsock
    WSADATA wsaData;
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);	
    if (result != 0) throw SocketException("WSAStartup failed.");

	// Create socket
	m_Socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (m_Socket == INVALID_SOCKET) 
	{
		m_Socket = NULL;
		throw SocketException("socket failed.");
	}

	sockaddr_in addr;
	ZeroMemory(&addr, sizeof(addr));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(localPort);
	addr.sin_addr.s_addr = htonl(INADDR_LOOPBACK);

	// Bind the socket
	result = bind(m_Socket, (const sockaddr *)&addr, sizeof(addr));
	if (result == SOCKET_ERROR)
	{
		closesocket(m_Socket);
		m_Socket = NULL;
		throw SocketException("bind failed.");
	}
}

TcpListener::~TcpListener()
{
	Close();
}

void TcpListener::Start(int connections)
{
	int result = listen(m_Socket, connections);
    if (result == SOCKET_ERROR)
        throw SocketException("listen failed.");
}

TcpClient *TcpListener::AcceptTcpClient()
{
    // Accept a client socket
    SOCKET clientSocket = accept(m_Socket, NULL, NULL);
    if (clientSocket == INVALID_SOCKET)
		throw SocketException("accept failed.");

	return new TcpClient(clientSocket);
}

void TcpListener::Close()
{
	if (m_Socket != NULL)
	{
		closesocket(m_Socket);
		m_Socket = NULL;
	}
}
