function WFTraceConsumerConnection(inStr, outStr)
{
	this.mainThread = Components.classes["@mozilla.org/thread-manager;1"].getService(Components.interfaces.nsIThreadManager).mainThread;

	this.outStr = outStr;
	this.binaryOutStr = Components.classes["@mozilla.org/binaryoutputstream;1"].createInstance(Components.interfaces.nsIBinaryOutputStream);
	this.binaryOutStr.setOutputStream(outStr);

	this.inStr = inStr;
	this.binaryInStr = Components.classes["@mozilla.org/binaryinputstream;1"].createInstance(Components.interfaces.nsIBinaryInputStream);
	this.binaryInStr.setInputStream(inStr);

	this.mayRequestStart = false;
	this.isStarted = false;
}

WFTraceConsumerConnection.prototype =
{
	init : function(callback)
	{
		this.callback = callback;
		this.beginRead();
	},

	beginRead : function()
	{
		var t = this;
		this.inStr.asyncWait(
		{
			onInputStreamReady : function(stream)
			{
				var msg;
				try
				{
					msg = t.binaryInStr.readString(); // Blocks main thread until whole message is received.
				}
				catch (ex)
				{
					t.callback.onClose();
					return;
				}
				t.dispatchMessage(msg);				
				t.beginRead();
			}
		},
		0,
		0,
		this.mainThread);
	},

	dispatchMessage : function(msg)
	{
		/*if (msg == 'START')
		{
			this.confirmStart();
			this.isStarted = true;
			this.callback.onStart();
		}
		else if (msg == 'STOP')
		{
			this.callback.onStop();
			this.isStarted = false;
			this.confirmStop();
		}
		else if (msg == 'MAY-REQUEST-START')
		{
			this.mayRequestStart = true;
			this.callback.onMayRequestStart(true);
		}
		else if (msg == 'MAY-NOT-REQUEST-START')
		{
			this.mayRequestStart = false;
			this.callback.onMayRequestStart(false);
		}
		else if (msg.substr(0, 10) == 'HIGHLIGHT,')
		{
			var idList = msg.substr(10).split(",")
			this.callback.onHighlight(idList);
		}
		else*/
			alert("Received unknown message: " + msg);			
	},

	/*
	confirmStart : function()
	{
		this.binaryOutStr.writeUtf8Z("START-OK");
	},

	confirmStop : function()
	{
		this.binaryOutStr.writeUtf8Z("STOP-OK");
	},*/

	notifyEvent : function(e)
	{
		this.binaryOutStr.writeUtf8Z(e);
	},

	sendData : function(data)
	{
		this.binaryOutStr.writeBytes(data, data.length);
	},

	/*
	requestStart : function()
	{
		this.binaryOutStr.writeUtf8Z("REQUEST-START");
	},

	requestStop : function()
	{
		this.binaryOutStr.writeUtf8Z("REQUEST-STOP");
	},*/

	startMark : function()
	{
		this.binaryOutStr.writeUtf8Z("MARK-START");
	},

	stopMark : function()
	{
		this.binaryOutStr.writeUtf8Z("MARK-STOP");
	},

	notifyBusy : function()
	{
		this.binaryOutStr.writeUtf8Z("BUSY");
	}
};

function WFTraceProviderService(callback)
{
	try
	{
		this.serverSocket = Components.classes["@mozilla.org/network/server-socket;1"].createInstance(Components.interfaces.nsIServerSocket);
		this.serverSocket.init(34971, true, -1);

		//alert('Starting to listen..');
		this.serverSocket.asyncListen(
		{
			onSocketAccepted : function(serverSocket, transport)
			{			
				//alert('Connected! Data: ' + callback);
				callback.onConsumerConnected(new WFTraceConsumerConnection(
					transport.openInputStream(Components.interfaces.nsITransport.OPEN_BLOCKING, 0, 0),
					transport.openOutputStream(Components.interfaces.nsITransport.OPEN_BLOCKING, 0, 0)));
					
			},
		
			onStopListening: function(serverSocket, socketStatus)
			{
			}		
		}
		);
		//alert('Starting to listen.. (all set up)');
	}
	catch (ex)
	{
		alert('tp:ex');
		throw ("Failed to set up trace data provider: " + ex);
	}
}


