#pragma once

#include "method.h"

#include "traceprovider.h"

struct Event
{
	enum { Start, Stop, NewMethod, EnterMethod, LeaveMethod, EnterFilteredMethod, LeaveFilteredMethod };
	Event() { }
	Event(int type, void *info) : Type(type), Info(info) { }
	int Type;
	void *Info;
};

class EventBuffer
{
public:
	EventBuffer();
	void NotifyStarted(int requestId, std::string jspFile);
	void NotifyStopped();
	void NotifyNewMethod(Method *info);
	void NotifyEnterMethod(Method *info);
	void NotifyLeaveMethod(Method *info);
	void NotifyEnterFilteredMethod();
	void NotifyLeaveFilteredMethod();
	void Process(std::vector<std::string> &messages, int max);
	bool IsDone;

private:
	std::vector<Event> m_buffer;
	int m_requestId;
	std::string m_jspFile;
	int m_processedIndex;
};

class EventProcessor
{
public:
	EventProcessor(TraceConsumerConnection *consumer);
	void AddBuffer(EventBuffer *buffer);
	void RunProcessor();

private:
	TraceConsumerConnection *m_consumer;
	HANDLE m_hProcessorThread;
	Locker m_thisLock;
	std::list<EventBuffer *> m_buffers;
};