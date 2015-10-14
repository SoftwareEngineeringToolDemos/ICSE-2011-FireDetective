#include "stdafx.h"

#include "method.h"

Method::Method(jvmtiEnv *jvmti, jmethodID method, int id) :
	m_jvmti(jvmti),
	m_method(method),
	Id(id),
	IsProcessed(false)
{
	jclass cls;
	jvmti->GetMethodDeclaringClass(method, &cls);

	JvmtiString methodNameStr(jvmti), methodSigStr(jvmti), methodTypeArgsStr(jvmti), clsStr(jvmti), clsTypeArgsStr(jvmti);
	jvmti->GetMethodName(method, methodNameStr.get_ref(), methodSigStr.get_ref(), methodTypeArgsStr.get_ref());
	jvmti->GetClassSignature(cls, clsStr.get_ref(), clsTypeArgsStr.get_ref());

	Name = methodNameStr.get();
	DeclaringClass = JniTypeDesc(clsStr.get());

	JvmtiString sourceFileStr(jvmti);
	jvmti->GetSourceFileName(cls, sourceFileStr.get_ref());
	SourceFile = sourceFileStr.is_set() ? sourceFileStr.get() : "";

	jint tableSize;
	JvmtiAutoPtr<jvmtiLineNumberEntry> tablePtr(m_jvmti);
	int result = m_jvmti->GetLineNumberTable(m_method, &tableSize, tablePtr.get_ref());
	if (result == JVMTI_ERROR_NONE)
	{
		m_lineNumbers.clear();
		for (int i = 0; i < tableSize; i++)
			m_lineNumbers.push_back(tablePtr.get()[i]);
	}	

	if (m_lineNumbers.size() > 0)
	{
		BeginLine = m_lineNumbers.front().line_number;
		EndLine = m_lineNumbers.back().line_number;
	}
	else
	{
		BeginLine = EndLine = -1;
	}

	static std::string packageFilter1("com.thechiselgroup");
	static std::string packageFilter2("com.sun.javaee.blueprints.petstore");
	static std::string packageFilter3("org.apache.jsp");
	static std::string methodNameFilter3("_jspService");
	IsFiltered = !(
		DeclaringClass.PackageName.compare(0, packageFilter1.length(), packageFilter1) == 0
		|| DeclaringClass.PackageName.compare(0, packageFilter2.length(), packageFilter2) == 0
		|| (DeclaringClass.PackageName.compare(0, packageFilter3.length(), packageFilter3) == 0 && Name == methodNameFilter3)
	);
}

struct jvmtiLineNumberEntryComparer
{
	 bool operator()(const jvmtiLineNumberEntry &e, const jvmtiLineNumberEntry &f)
	 {
		 return e.start_location < f.start_location;
	 }
};

int Method::GetLineFromPc(jlocation pc)
{
	jvmtiLineNumberEntry target;
	target.start_location = pc;
	std::vector<jvmtiLineNumberEntry>::iterator it = std::upper_bound(m_lineNumbers.begin(), m_lineNumbers.end(), target, jvmtiLineNumberEntryComparer());
	if (it == m_lineNumbers.begin())
		return -1;
	else
		return (it - 1)->line_number;
}

