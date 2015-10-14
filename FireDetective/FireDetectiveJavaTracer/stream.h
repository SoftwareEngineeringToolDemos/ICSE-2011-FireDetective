#pragma once

/* interface */ class IByteStream
{
public:
	virtual void ReadBytes(char *buffer, int bytesToRead) = 0;
	virtual void WriteBytes(const char *buffer, int bytesToWrite) = 0;
};

class StringStream
{
public:
	StringStream(IByteStream *stream);
	std::string ReadString();
	void WriteString(std::string str);

private:
	IByteStream *m_stream;
};