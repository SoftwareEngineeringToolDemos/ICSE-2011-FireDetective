#pragma once

void CheckJNIError(jint result, char *message);

void CheckJVMTIError(jvmtiError result, char *message);

template <class T>
class JvmtiAutoPtr
{
public:
	JvmtiAutoPtr(jvmtiEnv *env) : jvmti(env), data(NULL) { }
	~JvmtiAutoPtr() { if (is_set()) jvmti->Deallocate((unsigned char *)data); }
	T **get_ref() { return &data; }
	T *get() { return data; }
	bool is_set() { return data != NULL; }

private:
	jvmtiEnv *jvmti;
	T* data;
};

typedef JvmtiAutoPtr<char> JvmtiString;

class JniTypeDesc
{
public:
	JniTypeDesc() { }
	JniTypeDesc(char *desc);
	std::string Name;
	std::string PackageName;
};

class Locker
{
public:
	Locker();
	~Locker();
	void Lock();
	void Unlock();

private:
	CRITICAL_SECTION m_cs;
};

class Lock
{
public:
	Lock(Locker &locker);
	~Lock();

private:
	Locker &m_locker;
};