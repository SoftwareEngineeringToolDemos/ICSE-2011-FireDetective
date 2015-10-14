#include "stdafx.h"

#include "agent.h"

Agent::Agent(jvmtiEnv *jvmti) :
	m_jvmti(jvmti),
	m_thisLock(),
	m_threadTracerCount(0),
	m_eventProcessor(NULL)
{
	m_traceProvider = new TraceProvider(this);
}

void Agent::EnableDefaultNotifications()
{
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_VM_START, (jthread)NULL), "Could not set VM start notification.");
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_VM_INIT, (jthread)NULL), "Could not set VM init notification.");
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_VM_DEATH, (jthread)NULL), "Could not set VM death notification.");
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_THREAD_START, (jthread)NULL), "Could not set thread start notification.");
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_THREAD_END, (jthread)NULL), "Could not set thread death notification.");
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_EXCEPTION_CATCH, (jthread)NULL), "Could not set exception catch notification.");
}

void Agent::EnableSpecialNotifications()
{
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_METHOD_ENTRY, (jthread)NULL), "Could not set method entry notification.");
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_ENABLE, JVMTI_EVENT_METHOD_EXIT, (jthread)NULL), "Could not set method exit notification.");
}

void Agent::DisableSpecialNotifications()
{
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_DISABLE, JVMTI_EVENT_METHOD_ENTRY, (jthread)NULL), "Could not disable method entry notification.");
	CheckJVMTIError(m_jvmti->SetEventNotificationMode(JVMTI_DISABLE, JVMTI_EVENT_METHOD_EXIT, (jthread)NULL), "Could not disable method exit notification.");
}

void Agent::StartTrace(int id, std::string jspFile)
{
	if (m_eventProcessor != NULL)
	{
		ThreadTracer *tt = new ThreadTracer(id, jspFile, m_eventProcessor);

		SetCurrentThreadTracer(tt);
		
		{	
			Lock lock(m_thisLock);
			m_threadTracerCount++;
			EnableSpecialNotifications();
		}

		tt->NotifyStarted();
	}
}

void Agent::StopTrace()
{
	StopCurrentThreadTracer(GetCurrentThreadTracer());
}

void Agent::StopCurrentThreadTracer(ThreadTracer *tt)
{
	if (tt != NULL)
	{		
		SetCurrentThreadTracer(NULL);

		{
			Lock lock(m_thisLock);
			m_threadTracerCount--;
			if (m_threadTracerCount == 0)
				DisableSpecialNotifications();
		}

		tt->NotifyStopped();

		// Can't delete tt yet...
	}
}

ThreadTracer *Agent::GetCurrentThreadTracer()
{
	ThreadTracer *tt = NULL;
	CheckJVMTIError(m_jvmti->GetThreadLocalStorage(NULL, (void **)&tt), "GetThreadLocalStorage failed.");
	return tt;
}

void Agent::SetCurrentThreadTracer(ThreadTracer *tt)
{
	CheckJVMTIError(m_jvmti->SetThreadLocalStorage(NULL, tt), "SetThreadLocalStorage failed.");
}

void Agent::EnterMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method)
{
	ThreadTracer *tt = GetCurrentThreadTracer();
	if (tt != NULL)
	{
		try
		{
			tt->EnterMethod(jvmti, jni, thread, method);
		}
		catch (SocketException &)
		{
			StopCurrentThreadTracer(tt);
		}
	}
}

void Agent::LeaveMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method, jboolean was_popped_by_exception, jvalue return_value)
{
	ThreadTracer *tt = GetCurrentThreadTracer();
	if (tt != NULL)
	{
		try
		{
			tt->LeaveMethod(jvmti, jni, thread, method, was_popped_by_exception, return_value);
		}
		catch (SocketException &)
		{
			StopCurrentThreadTracer(tt);
		}
	}
}

void Agent::TraceProvider_ConsumerConnected(TraceConsumerConnection *consumer)
{
	m_eventProcessor = new EventProcessor(consumer);
}

void Agent::TraceProvider_ConsumerDisconnected(TraceConsumerConnection *consumer)
{
	m_eventProcessor = NULL;

	// Can't delete event processor yet...
}
