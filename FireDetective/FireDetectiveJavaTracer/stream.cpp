#include "stdafx.h"

#include "stream.h"

StringStream::StringStream(IByteStream *stream) :
	m_stream(stream)
{
}

std::string StringStream::ReadString()
{
	long stringLength;
	m_stream->ReadBytes((char *)&stringLength, 4);
	std::auto_ptr<char> buffer(new char[stringLength]);
	m_stream->ReadBytes(buffer.get(), stringLength);

	return std::string(buffer.get());
}

void StringStream::WriteString(std::string str)
{
	long stringLength = str.length();
	m_stream->WriteBytes((char *)&stringLength, 4);
	m_stream->WriteBytes(str.c_str(), stringLength);
}
