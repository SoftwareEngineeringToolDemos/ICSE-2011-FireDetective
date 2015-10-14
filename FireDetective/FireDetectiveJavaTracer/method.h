#pragma once

#include "utils.h"

class Method
{
public:
	Method(jvmtiEnv *jvmti, jmethodID method, int id);
	int GetLineFromPc(jlocation pc);
	int Id;
	std::string Name;
	JniTypeDesc DeclaringClass;
	int BeginLine;
	int EndLine;
	std::string SourceFile;
	bool IsProcessed;
	bool IsFiltered;

private:
	jmethodID m_method;
	jvmtiEnv *m_jvmti;
	std::vector<jvmtiLineNumberEntry> m_lineNumbers;
};
