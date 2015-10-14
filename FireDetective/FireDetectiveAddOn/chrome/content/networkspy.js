function WFNetworkSpy(logger, newDocumentListeners, onXhrRequestCompleted, getRunFirstScript)	
{
	this.logger = logger;
	this.interestingContentTypes = ["text/html", "text/javascript", "application/x-javascript", "text/css"];
	this.blacklist = [
		/^http:\/\/([^.]+\.)?mozilla\.com\//,
		/^https:\/\/([^.]+\.)?mozilla\.org\//,
		/^http:\/\/([^.]+\.)?fxfeeds\.mozilla\.com\//,
		/^http:\/\/safebrowsing\.clients\.google\.com\//,
		/^http:\/\/safebrowsing-cache\.google\.com\//,
		/^https:\/\/sb-ssl\.google\.com\/safebrowsing\//,
		/^http:\/\/suggestqueries\.google\.com\//,
		/^https:\/\/([^.]+\.)?addons\.mozilla\.org\//,
		/^https:\/\/go\.microsoft\.com\//,
		/^http:\/\/ocsp\.verisign\.com\//];

	this.progressSpy = new WFProgressSpy(this.logger, wfHook(this.onNewPage, this));
	this.newDocumentListeners = newDocumentListeners;
	this.onXhrRequestCompleted = onXhrRequestCompleted;
	this.getRunFirstScript = getRunFirstScript;

	this.observerService = Components.classes['@mozilla.org/observer-service;1'].getService(Components.interfaces.nsIObserverService);
	this.observerService.addObserver({ observe : wfHook(this.onHttpRequest, this) }, "http-on-modify-request", false);
	this.observerService.addObserver({ observe : wfHook(this.onHttpResponse, this) }, "http-on-examine-response", false);
	this.observerService.addObserver({ observe : wfHook(this.onHttpResponse, this) }, "http-on-examine-cached-response", false);

	this.requestId = 1;

	this.isInitialRequest = {};

	this.requests = new WFObjectHashTable();
}

WFNetworkSpy.prototype =
{
	isBlacklisted : function(request)
	{
		var url = request.originalURI.spec;
		for (var i = 0; i < this.blacklist.length; i++)
			if (url.match(this.blacklist[i]))
				return true;
		return false;
	},
			
	onHttpRequest: function(request, topic, data)
	{
		wfAssert(topic == "http-on-modify-request");
		request = request.QueryInterface(Components.interfaces.nsIHttpChannel);
		//this.logger.logCustom("INCOMING " + request.originalURI.spec + " (" + request.name + ")");

		// Ignore blacklisted urls
		if (this.isBlacklisted(request)) return;
		this.logger.logCustom("NOT BLACKLISTED");

		/*
		var lc = this.getLoadContext(request);
		var lcId = this.loadContexts.get(lc);
		if (!lcId)
		{
			lcId = this.loadContextId++;
			this.loadContexts.add(lc, lcId);
		}
		this.logger.logCustom("LOAD-CONTEXT " + lcId);
		*/

		// Determine if this request has been directly initiated by the user
		var isInitial = !!(request.loadFlags & Components.interfaces.nsIChannel.LOAD_INITIAL_DOCUMENT_URI);
		var isInitialForReal = isInitial && !this.duplicateInitialComingUp;
		if (isInitial && this.duplicateInitialComingUp)
			this.duplicateInitialComingUp = false;

		// Store id for later when we receive the response.
		var id = this.requestId++;
		this.requests.add(request, id);
		this.isInitialRequest[id] = isInitial;

		// Log request
		var xhr = this.getXhr(request);
		this.logger.logRequest(id, request, isInitial, isInitialForReal, xhr, new Date().getTime());

		// Set the id for the xhr so the content window has access to it
		if (xhr)
		{
			// This call will not show up because it is a hidden function, and it's a good thing, since its call stack would be weird (it would have the current function on it).
			this.win.wrappedJSObject["wf_hiddenFuncFindXhrObject"](xhr.wrappedJSObject).id = id;
		}

		// Append header before forwarding
		request.setRequestHeader("X-Fire-Detective-Request-Id", id, false);
	},

	onHttpResponse: function(request, topic, data)
	{
		var isNormal = (topic == "http-on-examine-response");
		var isCached = (topic == "http-on-examine-cached-response");
		wfAssert(isNormal || isCached);
		request = request.QueryInterface(Components.interfaces.nsIHttpChannel);

		// Ignore blacklisted urls
		if (this.isBlacklisted(request)) return;

		// Fetch id
		var id = this.requests.get(request);

		// Initial request?
		if (this.isInitialRequest[id])
		{
			// See if this initial request was redirected. 
			// If yes, firefox will mark the next request as initial as well, even though it's not. 
			// So set a flag here to compensate for this.
			if (request.responseStatus >= 300 && request.responseStatus <= 399)
				this.duplicateInitialComingUp = true;
		}

		// Is it worth to consider the content of the response?
		var xhr = this.getXhr(request);
		var hasContent = request.requestSucceeded && (xhr || this.interestingContentTypes.indexOf(request.contentType) >= 0);
		this.logger.logResponse(id, request, xhr, hasContent, new Date().getTime());

		// Hook spy up to the channel to read the response content, if it's interesting.
		if (hasContent)
		{
			var requestAsTracableChannel = request.QueryInterface(Components.interfaces.nsITraceableChannel);
			new WFChannelTrafficSpy(requestAsTracableChannel, this.logger, id, this.isInitialRequest[id] && !this.duplicateInitialComingUp, (!!xhr) ? this.onXhrRequestCompleted : undefined, this.getRunFirstScript);
		}
	},

	onNewPage : function(win)
	{
		this.win = win;

		for (var i = 0; i < this.newDocumentListeners.length; i++)
			this.newDocumentListeners[i].onNewDocument(win);
	},

	getXhr : function(request)
	{
		try
		{
			return request.notificationCallbacks.getInterface(Components.interfaces.nsIXMLHttpRequest);
		}
		catch (ex)
		{
			return null;
		}
	},

	getLoadContext : function(request)
	{
		/*
		try
		{
			return request.QueryInterface(Components.interfaces.nsIChannel).notificationCallbacks.getInterface(Components.interfaces.nsILoadContext).associatedWindow.document;
		}
		catch (ex) 
		{
			try
			{
				return request.loadGroup.notificationCallbacks.getInterface(Components.interfaces.nsILoadContext).associatedWindow.document;
			}
			catch (ex)
			{
				return null;
			}
		}*/
		return request.owner;
	}
};

function WFProgressSpy(logger, newPageListener)
{
	this.logger = logger;
	this.newPageListener = newPageListener;

	var self = this;
	window.addEventListener("load", function()
	{
		try
		{
			gBrowser.addProgressListener(self, Components.interfaces.nsIWebProgress.NOTIFY_STATE_DOCUMENT);
		}
		catch (ex)
		{
			wfAlertObjectDetails(ex);
		}
	},
	false);
}

WFProgressSpy.prototype = 
{
	onStateChange: function(webProgress, request, flags, status) { },
	/*{
		this.logger.logCustom("FLAGS " + flags);
		if (flags & Components.interfaces.nsIWebProgressListener.STATE_START)
		{
			//alert(flags + " " + (this.lastWebProgress == webProgress ? "yes" : "no") + " " + request.URI.spec);
			//this.lastWebProgress = webProgress;
			//this.logger.logUserRequest();
		}
	},*/

	onLocationChange: function(webProgress, request, uri)
	{
		// Only fire for real requests (not anchor changes)
		if (request)
		{
			this.logger.logNewPage(uri.spec);
			this.newPageListener(webProgress.DOMWindow);
		}
	},

	onProgressChange: function(aWebProgress, aRequest, curSelf, maxSelf, curTot, maxTot) { },
	onStatusChange: function(aWebProgress, aRequest, aStatus, aMessage) { },
	onSecurityChange: function(aWebProgress, aRequest, aState) { },

	QueryInterface: function(iid)
	{
		if (iid.equals(Components.interfaces.nsIWebProgressListener) || iid.equals(Components.interfaces.nsISupportsWeakReference) || iid.equals(Components.interfaces.nsISupports))
			return this;
		throw Components.results.NS_NOINTERFACE;
	}
};

function WFChannelTrafficSpy(channel, logger, id, inject, onRequestCompleted, getRunFirstScript)
{
	this.forward = channel.setNewListener(this);
	this.logger = logger;
	this.id = id;
	this.inject = inject;
	this.onRequestCompleted = onRequestCompleted;
	this.offset = 0;	
	if (this.inject)
	{
		this.textInput = new WFTextReader();
		this.textOutput = new WFTextWriter();
		this.htmlTransformer = new WFHtmlTransformer(this.textInput, this.textOutput, getRunFirstScript());
	}

	this.memStream = Components.classes["@mozilla.org/storagestream;1"].createInstance(Components.interfaces.nsIStorageStream);
	this.memStream.init(8192, 0xffffffff, null);
	this.memStreamOut = Components.classes["@mozilla.org/binaryoutputstream;1"].createInstance(Components.interfaces.nsIBinaryOutputStream);
	this.memStreamOut.setOutputStream(this.memStream.getOutputStream(0));

	this.originalDataChunks = [];
}

WFChannelTrafficSpy.prototype =
{
	onDataAvailable: function(request, context, inputStream, offset, count)
	{
		try
		{
			var binaryInStr = Components.classes["@mozilla.org/binaryinputstream;1"].createInstance(Components.interfaces.nsIBinaryInputStream);
			binaryInStr.setInputStream(inputStream);

			var chunk = binaryInStr.readBytes(count);
			this.originalDataChunks.push(chunk); // Keep original

			if (this.inject)
			{				
				/*var n = "";
				var x = 25;
				for (var i = 0; i < chunk.length; i += x)
				{					
					this.textInput.write(chunk.substr(i, x));
					this.htmlTransformer.transform();
					var s = this.textOutput.readAll();
					n += s;
				}
				chunk = n;*/

				this.logger.logCustom("START-TRANSFORM");
				this.textInput.write(chunk);
				this.htmlTransformer.transform(this.logger);
				chunk = this.textOutput.readAll();				
				count = chunk.length;
				this.logger.logCustom("END-TRANSFORM");
			}

			offset = this.offset;
			this.offset += count;

			this.memStreamOut.writeBytes(chunk, count);
		}
		catch (ex)
		{
			wfAlertObjectDetails(ex);
			return;
		}

		// Can't catch exceptions here: see https://bugzilla.mozilla.org/show_bug.cgi?id=492534.
		this.forward.onDataAvailable(request, context, this.memStream.newInputStream(offset), offset, count);
	},

	onStartRequest: function(request, context)
	{
		this.forward.onStartRequest(request, context);
	},

	onStopRequest: function(request, context, statusCode)
	{
		var httpRequest = request.QueryInterface(Components.interfaces.nsIHttpChannel);
		if (httpRequest.requestSucceeded)
		{
			var data = this.originalDataChunks.join("");
			this.logger.logResponseData(this.id, data);
			if (this.onRequestCompleted)
				this.onRequestCompleted(this.id, data);
		}

		this.forward.onStopRequest(request, context, statusCode);
	},

	QueryInterface: function(iid)
	{
		if (iid.equals(Components.interfaces.nsIStreamListener) || iid.equals(Components.interfaces.nsISupports)) 
			return this;
	        throw Components.results.NS_NOINTERFACE;
        }
};

