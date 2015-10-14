#include "stdafx.h"

#include "event.h"

EventBuffer::EventBuffer() :
	IsDone(false),
	m_processedIndex(0)
{
}

void EventBuffer::NotifyStarted(int requestId, std::string jspFile)
{
	m_requestId = requestId;
	m_jspFile = jspFile;
	m_buffer.push_back(Event(Event::Start, NULL));
}

void EventBuffer::NotifyStopped()
{
	m_buffer.push_back(Event(Event::Stop, NULL));
}

void EventBuffer::NotifyNewMethod(Method *info)
{
	m_buffer.push_back(Event(Event::NewMethod, info));
}

void EventBuffer::NotifyEnterMethod(Method *info)
{
	m_buffer.push_back(Event(Event::EnterMethod, info));
}

void EventBuffer::NotifyLeaveMethod(Method *info)
{
	m_buffer.push_back(Event(Event::LeaveMethod, info));
}

void EventBuffer::NotifyEnterFilteredMethod()
{
	m_buffer.push_back(Event(Event::EnterFilteredMethod, NULL));
}

void EventBuffer::NotifyLeaveFilteredMethod()
{
	m_buffer.push_back(Event(Event::LeaveFilteredMethod, NULL));
}

void EventBuffer::Process(std::vector<std::string> &messages, int count)
{
	int i = 0;
	for (; i < count; i++)
	{
		if (m_processedIndex >= (int)m_buffer.size())
		{
			IsDone = true;
			break;
		}

		std::ostringstream msg;
		Event &e = m_buffer[m_processedIndex++];
		Method *mi = (Method *)e.Info;
		switch (e.Type)
		{
		case Event::Start:
			msg << "REQUEST," << m_requestId << "," << m_jspFile;
			break;
		case Event::Stop:
			msg << "REQUEST-DONE," << m_requestId;
			break;
		case Event::NewMethod:
			msg << "METHOD," << m_requestId << "," << mi->Id << "," << mi->DeclaringClass.PackageName << "," << mi->DeclaringClass.Name << "," << mi->Name << ","
				<< mi->BeginLine << "," << mi->EndLine << "," << mi->SourceFile;
			break;
		case Event::EnterMethod:
			msg << "C," << m_requestId << "," << mi->Id << "," << 0;
			break;
		case Event::LeaveMethod:
			msg << "R," << m_requestId << "," << 0;
			break;
		case Event::EnterFilteredMethod:
			msg << "C," << m_requestId << "," << 0 << "," << 0;
			break;		
		case Event::LeaveFilteredMethod:
			msg << "R," << m_requestId << "," << 0;
			break;
		}
		messages.push_back(msg.str());
	}
}

unsigned int __stdcall ProcessorFunc(void *arg)
{
	((EventProcessor *)arg)->RunProcessor();
	return 0;
}

EventProcessor::EventProcessor(TraceConsumerConnection *consumer) :
	m_consumer(consumer)
{
	m_hProcessorThread = (HANDLE)_beginthreadex(NULL, 0, &ProcessorFunc, this, 0, NULL);	
}

void EventProcessor::AddBuffer(EventBuffer *buffer)
{
	Lock lock(m_thisLock);
	m_buffers.push_back(buffer);
}

void EventProcessor::RunProcessor()
{
	try
	{
		while (true)
		{
			std::vector<EventBuffer *> buffersCopy;
			std::vector<EventBuffer *> done;

			{
				Lock lock(m_thisLock) ;
				buffersCopy = std::vector<EventBuffer *>(m_buffers.begin(), m_buffers.end());
			}

			bool hasProcessedAny = false;
			for (std::vector<EventBuffer *>::iterator it = buffersCopy.begin(); it != buffersCopy.end(); it++)
			{
				std::vector<std::string> messages;
				(*it)->Process(messages, 10000);
				hasProcessedAny = hasProcessedAny || messages.size() > 0;
				for (std::vector<std::string>::iterator jt = messages.begin(); jt != messages.end(); jt++)
					m_consumer->SendMessage(*jt);
				messages.clear();

				if ((*it)->IsDone)
				{
					done.push_back(*it);
				}
			}

			{
				Lock lock(m_thisLock);
				for (std::vector<EventBuffer *>::iterator it = done.begin(); it != done.end(); it++)
					m_buffers.erase(std::find(m_buffers.begin(), m_buffers.end(), *it));
			}

			if (!hasProcessedAny)
			{
				Sleep(10);
			}
		}
	}
	catch (SocketException &)
	{
	}
}
