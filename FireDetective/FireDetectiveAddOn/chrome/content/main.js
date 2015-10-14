function WFServiceImpl()
{
}

WFServiceImpl.prototype =
{
	init : function()
	{
		WFConsole.log("Starting up now!");

		this.consumer = null;
		this.logFile = new WFLogFile();
		this.logFile.log("Logging starts");
		this.logger = new WFRemoteEventLogger(this.logFile);

		/*var mainThread = Components.classes["@mozilla.org/thread-manager;1"].getService(Components.interfaces.nsIThreadManager).mainThread;
		alert(Components.classes["@mozilla.org/thread-manager;1"].getService(Components.interfaces.nsIThreadManager).isMainThread);

		var _observer = mainThread.observer;
		var s = "";
		for (key in _observer) s += key + " ";
		alert(s);

		var self = this;
		this.eventLoop = 0;

		mainThread.observer = {
			QueryInterface: function(iid) { return _observer.QueryInterface(iid); },
			onDispatchedEvent : function(t) { _observer.onDispatchedEvent(t); },
			onProcessNextEvent : function(t, mw, rd) { self.logger.logCustom("PROCESSING " + self.eventLoop++); _observer.onProcessNextEvent(t, mw, rd); },
			afterProcessNextEvent : function(t, rd) { self.logger.logCustom("DONE" + --self.eventLoop); _observer.afterProcessNextEvent(t, rd); }
		}

		this.initProviderService();
		this.initDebugService();
		this.initDomSpy();
		this.initNetworkSpy();		

		alert('here');

		setTimeout(function()
		{
			this.initProviderService();
			this.initDebugService();
			this.initDomSpy();
			this.initNetworkSpy();		
		}, 2000);

		var jsd = Components.classes['@mozilla.org/js/jsd/debugger-service;1'].getService(this.jsdIDebuggerService);
		jsd.enterNestedEventLoop({onNest:function() {}});		
		alert('done');*/

		this.initProviderService();

		this.debugService = new WFDebugService(this.logger);
		this.logFile.log("Debugging service is on");

		//this.domSpy = new WFDomSpy(this.logger);

		var self = this;

		this.networkSpy = new WFNetworkSpy(this.logger, 
			[this.debugService /*, this.domSpy*/ ],
			function(id, data) { return self.debugService.onXhrRequestCompleted(id, data); },
			function() { return self.debugService.runOnNewDocument; });
	},

	initProviderService : function()
	{
		new WFTraceProviderService(
		{
			onConsumerConnected : function(cn)
			{
				if (WFService.consumer == null)
				{
					WFService.onConsumerConnect(cn);
					cn.init(
					{ 
						//onStart : wfHook(WFService.onStart, WFService),
						//onStop : wfHook(WFService.onStop, WFService),
						//onMayRequestStart : wfHook(WFService.onMayRequestStart, WFService),
						onHighlight : wfHook(WFService.onHighlight, WFService),
						onClose : wfHook(WFService.onConsumerDisconnect, WFService)
					});
				}
				else
				{
					cn.notifyBusy();
					WFConsole.log("A second trace consumer tried to connect but was ignored.");
				}
			}
		});
	},

	onConsumerConnect : function(cn)
	{
		this.consumer = cn;
		this.logger.connect(cn);
		WFGui.setStateConnected();
	},

	onConsumerDisconnect : function()
	{
		this.consumer = null;
		this.logger.disconnect();
		WFGui.setStateInitial();
	},

	onHighlight : function(idList)
	{
		this.domSpy.highlight(idList);
	}

};

var WFService;
try
{
	WFService = new WFServiceImpl();
	WFService.init();
}
catch (ex)
{
	wfAlertObjectDetails(ex);
}


