#include "stdafx.h"

#include "traceprovider.h"

TraceConsumerConnection::TraceConsumerConnection(TcpClient *tcpClient) :
	m_tcpClient(tcpClient),
	m_networkStream(tcpClient->GetNetworkStream()),
	m_dataStream(new StringStream(m_networkStream))
{
}

void TraceConsumerConnection::ProcessMessages()
{
	while (true)
	{
		// TODO: Do something with these messages...!
		std::string message = m_dataStream->ReadString();
	}
}

void TraceConsumerConnection::SendMessage(std::string message)
{
	// Send data!
	m_dataStream->WriteString(message);
}

unsigned int __stdcall ListenerFunc(void *arg)
{
	((TraceProvider *)arg)->RunListener();
	return 0;
}

TraceProvider::TraceProvider(ITraceProviderListener *listener) :
	m_listener(listener),
	m_tcpListener(new TcpListener(38056))
{
	m_tcpListener->Start(1);

	// Create listener thread
	m_hListenerThread = (HANDLE)_beginthreadex(NULL, 0, &ListenerFunc, this, 0, NULL);	
}

void TraceProvider::RunListener()
{
	while (true)
	{		
		TcpClient *tcpClient;
		try
		{
			tcpClient = m_tcpListener->AcceptTcpClient();
		}
		catch (SocketException &)
		{
			return;
		}

		TraceConsumerConnection *current = new TraceConsumerConnection(tcpClient);

		m_listener->TraceProvider_ConsumerConnected(current);

		//std::cout << "Connected!\n";

		try
		{
			current->ProcessMessages();
		}
		catch (SocketException &ex)
		{
			//std::cout << "SocketException: " << ex.GetMessage() << "\n";
		}

		//std::cout << "Disconnected\n";

		m_listener->TraceProvider_ConsumerDisconnected(current);

		// Can't delete current yet, it might be accessed from other threads (TODO: use boost::shared_ptr?).
	}
}