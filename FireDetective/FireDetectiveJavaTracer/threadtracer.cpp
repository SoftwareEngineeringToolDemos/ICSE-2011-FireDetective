#include "stdafx.h"

#include "threadtracer.h"

ThreadTracer::ThreadTracer(int id, std::string jspFile, EventProcessor *eventProcessor) :
	m_id(id),
	m_jspFile(jspFile),
	m_buffer(new EventBuffer()), // TODO memory leakage for every recorded call, however, not significant enough for our PetStore study so going to leave it as is.
	m_eventProcessor(eventProcessor),
	m_nextMethodId(1)
{
	//m_filteredStack.reserve(2048);
}

Method *ThreadTracer::GetMethodInfo(jvmtiEnv *jvmti, jmethodID method)
{
	stdext::hash_map<jmethodID, Method *>::iterator it = m_methodInfo.find(method);
	if (it == m_methodInfo.end())
		it = m_methodInfo.insert(std::pair<jmethodID, Method *>(method, new Method(jvmti, method, m_nextMethodId++))).first;
	return it->second;
}

void ThreadTracer::NotifyStarted()
{
	m_buffer->NotifyStarted(m_id, m_jspFile);
}

void ThreadTracer::NotifyStopped()
{
	m_buffer->NotifyStopped();
	MemoryBarrier();
	m_eventProcessor->AddBuffer(m_buffer);
}

void ThreadTracer::EnterMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method)
{
	Method *mi = GetMethodInfo(jvmti, method);
	if (!mi->IsFiltered)
	{
		if (!mi->IsProcessed)
		{
			m_buffer->NotifyNewMethod(mi);
			mi->IsProcessed = true;
		}

		m_buffer->NotifyEnterMethod(mi);
	}
	else
	{
		if (m_filteredStack.size() > 0 && m_filteredStack.back() == false)
			m_buffer->NotifyEnterFilteredMethod();
	}
	
	m_filteredStack.push_back(mi->IsFiltered);
}

void ThreadTracer::LeaveMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method, jboolean was_popped_by_exception, jvalue return_value)
{
	m_filteredStack.pop_back();

	Method *mi = GetMethodInfo(jvmti, method);
	if (!mi->IsFiltered)
	{
		m_buffer->NotifyLeaveMethod(mi);
	}
	else
	{
		if (m_filteredStack.size() > 0 && m_filteredStack.back() == false)
			m_buffer->NotifyLeaveFilteredMethod();
	}
}
