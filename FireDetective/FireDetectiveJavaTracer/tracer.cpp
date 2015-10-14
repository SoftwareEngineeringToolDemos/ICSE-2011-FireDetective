// Defines the exported functions for the DLL application (and more).

#include "stdafx.h"
#include "tracer.h"
#include "utils.h"

#include "agent.h"

Agent *g_Agent;

void JNICALL VmStart(jvmtiEnv *jvmti, JNIEnv* jni)
{
	std::cout << "VM Started\n";
}

void JNICALL VmInit(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread)
{
	std::cout << "VM Inited\n";
}

void JNICALL VmDeath(jvmtiEnv *jvmti, JNIEnv* jni)
{
	std::cout << "VM Destruct\n";
}

void JNICALL VmThreadStart(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread)
{
	std::cout << "VM Thread Created\n";
}

void JNICALL VmThreadEnd(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread)
{
	std::cout << "VM Thread Ended\n";
}

void JNICALL VmCatchException(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method, jlocation location, jobject exception)
{	
	jclass cls = jni->GetObjectClass(exception);

	JvmtiString sigStr(jvmti);
	jvmti->GetClassSignature(cls, sigStr.get_ref(), NULL);
	std::string sig(sigStr.get());

	if (sig == "Lcom/thechiselgroup/firedetective/StartTraceException;")
	{
		int id = 0;
		std::string jspFile;

		jint tableSize;
		JvmtiAutoPtr<jvmtiLocalVariableEntry> tablePtr(jvmti);
		jvmti->GetLocalVariableTable(method, &tableSize, tablePtr.get_ref());

		for (int i = 0; i < tableSize; i++)
		{
			jvmtiLocalVariableEntry *var = &tablePtr.get()[i];
			if (std::string(var->name) == "fireDetectiveRequestId")
			{
				jint val;
				jvmti->GetLocalInt(thread, 0, var->slot, &val);
				id = val;
			}
			else if (std::string(var->name) == "fireDetectiveJspFile")
			{				
				jobject str;
				jvmti->GetLocalObject(thread, 0, var->slot, &str);
				jboolean isCopy;
				const char *strData = jni->GetStringUTFChars((jstring)str, &isCopy);
				jspFile = std::string(strData);
				jni->ReleaseStringUTFChars((jstring)str, strData);				
				jni->DeleteLocalRef(str);
			}

			// Need to deallocate these struct members explicitly
			jvmti->Deallocate((unsigned char *)var->name);
			jvmti->Deallocate((unsigned char *)var->signature);
			jvmti->Deallocate((unsigned char *)var->generic_signature);
		}

		if (id > 0)
			g_Agent->StartTrace(id, jspFile);
	}
	else if (sig == "Lcom/thechiselgroup/firedetective/EndTraceException;")
	{
		g_Agent->StopTrace();
	}
}

void JNICALL VmEnterMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method)
{
	g_Agent->EnterMethod(jvmti, jni, thread, method);
}

void JNICALL VmLeaveMethod(jvmtiEnv *jvmti, JNIEnv* jni, jthread thread, jmethodID method, jboolean was_popped_by_exception, jvalue return_value)
{
	g_Agent->LeaveMethod(jvmti, jni, thread, method, was_popped_by_exception, return_value);
}

JNIEXPORT jint JNICALL Agent_OnLoad(JavaVM *jvm, char *options, void *reserved)
{
	jvmtiEnv *jvmti;
	CheckJNIError(
		jvm->GetEnv((void **)&jvmti, JVMTI_VERSION_1_1),
		"Unable to access JVMTI version 1.1, need at least JDK 5.0.");

	try
	{
		g_Agent = new Agent(jvmti);
	}
	catch (SocketException &)
	{
		//std::cerr << "FAIL!\n";
	}

	if (g_Agent != NULL)
	{
		//std::cerr << "OK!\n";

		jvmtiCapabilities caps;
		ZeroMemory(&caps, sizeof(caps));
		caps.can_generate_exception_events = 1;
		caps.can_generate_method_entry_events = 1;
		caps.can_generate_method_exit_events = 1;
		caps.can_get_line_numbers = 1;
		caps.can_get_source_file_name = 1;
		caps.can_access_local_variables = 1;
		CheckJVMTIError(
			jvmti->AddCapabilities(&caps),
			"Can't set JVMTI caps.");

		jvmtiEventCallbacks callbacks;
		ZeroMemory(&callbacks, sizeof(callbacks));
		callbacks.VMStart = &VmStart;
		callbacks.VMDeath = &VmDeath;
		callbacks.VMInit = &VmInit;
		callbacks.ThreadStart = &VmThreadStart;
		callbacks.ThreadEnd = &VmThreadEnd;
		callbacks.ExceptionCatch = &VmCatchException;
		callbacks.MethodEntry = &VmEnterMethod;
		callbacks.MethodExit = &VmLeaveMethod;
		CheckJVMTIError(
			jvmti->SetEventCallbacks(&callbacks, sizeof(callbacks)),
			"Can't set JVMTI callbacks.");

		g_Agent->EnableDefaultNotifications();
	}

	return JNI_OK;
}

JNIEXPORT void JNICALL Agent_OnUnload(JavaVM *jvm)
{
	//delete g_Agent;
}


