function WFRemoteEventLogger(logfile)
{
	this.logfile = logfile;
	this.disconnect();
}

WFRemoteEventLogger.prototype =
{
	connect : function(consumer)
	{
		this.consumer = consumer;
	},

	disconnect : function()
	{
		this.consumer = { notifyEvent : function() { }, sendData : function() { } };
	},

	log : function(e)
	{
		this.consumer.notifyEvent(e);
		this.logfile.log(e);
	},

	log2 : function(e)
	{
		this.consumer.notifyEvent(e);
	},

	logCall : function(depth, frame, frameCalling)
	{
		this.log2("C," + depth + "," + this.getStackFrameDesc(frame) + "," + (frameCalling ? this.getStackFrameDesc(frameCalling) : ""));		
		//this.logfile.log("PRECALL6 " + (!!("a".repeat)) + "->" + frame.script.functionName + " " + "C," + depth + "," + this.getStackFrameDesc(frame) + "," + (frameCalling ? this.getStackFrameDesc(frameCalling) : ""));
		this.logfile.logWithSpacing(frame.script.functionName + " " + "C," + depth + "," + this.getStackFrameDesc(frame) + "," + (frameCalling ? this.getStackFrameDesc(frameCalling) : ""), depth);
		this.keepDepth = depth;
	},

	logReturn : function(frame)
	{
		this.log2("R," + this.getStackFrameDesc(frame));
		this.logfile.logWithSpacing("RET " + this.getStackFrameDesc(frame), this.keepDepth + 1);
		if (this.keepDepth > 0) this.keepDepth--;
	},

	logScriptCreated : function(details)
	{
		this.log("SCRIPT," + details.script.tag + "," + (details.script.functionName || '') + "," +  details.startLine + "," + details.endLine + "," + details.deltaLine + "," + (details.sourceXhr || '') + "," + details.sourceFile);
		if (details.sourceTextComingUp)
			this.log("SCRIPT-SOURCE,comingup,");
		else if (details.sourceText)
			this.log("SCRIPT-SOURCE,src,"  + details.sourceText);
		else
			this.log("SCRIPT-SOURCE,,");
	},

	logUserRequest : function()
	{
		this.log("NEW-PAGE-REQUEST");
	},

	logNewPage : function(page)
	{
		this.log("NEW-PAGE-LOADED," + page);
	},

	logNewPageScripts : function()
	{
		this.log("NEW-PAGE-SCRIPTS,");
	},

	logRequest : function(id, request, isInitial, isInitialForReal, xhr, startTime)
	{
		var flags = (isInitialForReal ? "initial" : (isInitial ? "initial-duplicate" : (xhr ? "xhr" : "")));
		this.log("REQUEST,{0},{1},{2},{3},{4}".format(id, startTime, flags, request.requestMethod, request.URI.spec));
	},

	logResponse : function(id, request, xhr, hasContent, endTime)
	{
		this.log("RESPONSE,{0},{1},{2},{3},{4},{5},{6},{7}".format(id, endTime, xhr ? "xhr" : "", request.responseStatus, hasContent ? "content" : "", request.contentType, request.contentCharset, request.URI.spec));
	},

	logResponseData : function(id, data)
	{
		this.log("RESPONSE-DATA,{0},{1}".format(id, data.length));
		this.consumer.sendData(data);
	},

	logError : function(error)
	{
		this.log("ERROR," + error);
	},

	logDomEvent : function(name, target)
	{
		var targetName = target ? target.nodeName.toLowerCase() : "unknown";
		if (targetName == "#document") targetName = "document";
		this.logEvent(name, targetName);
	},

	logEvent : function(name, target, id)
	{
		this.log("EVENT," + name + "," + (target ? target : "") + "," + (id ? id : ""));
	},

	logCallInfo : function(name, xhrId, itId)
	{
		this.log("CALL-INFO," + name + "," + (xhrId ? xhrId : "") + "," + (itId ? itId : ""));
	},

	logMod : function(id)
	{
		this.log("MOD," + id);
	},

	logCustom : function(s)
	{
		this.logfile.log(s);
	},

	getStackFrameDesc : function(stackFrame)
	{
		return stackFrame.script.tag + "," + stackFrame.line; //+ "," + stackFrame.script.pcToLine(stackFrame.pc, 2);
	}
};


