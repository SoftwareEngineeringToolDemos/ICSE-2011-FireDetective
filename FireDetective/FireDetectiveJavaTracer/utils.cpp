#include "stdafx.h"
#include "utils.h"

void CheckJNIError(jint result, char *message)
{
	if (result != JNI_OK)
	{
		std::cerr << message << std::endl;
		exit(-1);
	}
}

void CheckJVMTIError(jvmtiError result, char *message)
{
	if (result != JVMTI_ERROR_NONE)
	{
		std::cerr << message << std::endl;
		exit(-1);
	}
}

JniTypeDesc::JniTypeDesc(char *desc)
{
	std::string d(desc);
	if (d.length() >= 1 && d[0] == 'L' && d[d.length() - 1] == ';')
	{
		d = d.substr(1, d.length() - 2);
		size_t index = d.rfind('/');
		if (index != std::string::npos)
		{
			Name = d.substr(index + 1);
			PackageName = d.substr(0, index);
			std::replace(PackageName.begin(), PackageName.end(), '/', '.');
			return;
		}
	}

	Name = d;
	PackageName = std::string("");
}

// Lock

Locker::Locker()
{
	InitializeCriticalSection(&m_cs);
}

Locker::~Locker()
{
	DeleteCriticalSection(&m_cs);
}

void Locker::Lock()
{
	EnterCriticalSection(&m_cs);
}

void Locker::Unlock()
{
	LeaveCriticalSection(&m_cs);
}

Lock::Lock(Locker &locker) :
	m_locker(locker)
{
	m_locker.Lock();
}

Lock::~Lock()
{
	m_locker.Unlock();
	MemoryBarrier();
}