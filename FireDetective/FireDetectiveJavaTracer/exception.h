#pragma once

#include <string>

class Exception
{
public:
	Exception(std::string message);
	std::string GetMessage();

private:
	std::string m_Message;
};