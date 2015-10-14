#pragma once

#include "utils.h"
#include "traceprovider.h"
#include "method.h"
#include "event.h"

class ThreadTracer
{
public:
	ThreadTracer(int id, std::string jspFile, EventProcessor *eventProcessor);
	void EnterMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method);
	void LeaveMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method, jboolean was_popped_by_exception, jvalue return_value);
	void NotifyStarted();
	void NotifyStopped();

private:
	int m_id;
	std::string m_jspFile;
	EventProcessor *m_eventProcessor;
	EventBuffer *m_buffer;
	int m_nextMethodId;
	stdext::hash_map<jmethodID, Method *> m_methodInfo;
	Method *GetMethodInfo(jvmtiEnv *jvmti, jmethodID method);
	std::vector<bool> m_filteredStack;
};