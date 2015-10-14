#include "stdafx.h"

#include "exception.h"

Exception::Exception(std::string message)
{
	m_Message = message;
}

std::string Exception::GetMessage()
{
	return m_Message;
}