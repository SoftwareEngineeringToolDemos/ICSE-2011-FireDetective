#pragma once

#include "traceprovider.h"

#include "threadtracer.h"

#include "event.h"

class Agent : public ITraceProviderListener
{
public:
	Agent(jvmtiEnv *jvmti);
	void StartTrace(int id, std::string jspFile);
	void StopTrace();
	void EnterMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method);
	void LeaveMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method, jboolean was_popped_by_exception, jvalue return_value);
	void EnableDefaultNotifications();

private:
	jvmtiEnv *m_jvmti;
	TraceProvider *m_traceProvider;	
	volatile int m_threadTracerCount;
	Locker m_thisLock;
	EventProcessor *m_eventProcessor;

	void EnableSpecialNotifications();
	void DisableSpecialNotifications();

	ThreadTracer *GetCurrentThreadTracer();
	void SetCurrentThreadTracer(ThreadTracer *tt);
	void StopCurrentThreadTracer(ThreadTracer *tt);

	virtual void TraceProvider_ConsumerConnected(TraceConsumerConnection *consumer);
	virtual void TraceProvider_ConsumerDisconnected(TraceConsumerConnection *consumer);
};

