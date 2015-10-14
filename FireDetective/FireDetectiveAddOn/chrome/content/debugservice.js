function WFDebugService(logger)
{
	this.jsdIDebuggerService = Components.interfaces.jsdIDebuggerService;
	this.jsdIExecutionHook = Components.interfaces.jsdIExecutionHook; 
	this.jsdICallHook = Components.interfaces.jsdICallHook;

	this.filterUrls = [ "chrome://", "XStringBundle", "file:/C:/Program%20Files/Mozilla%20Firefox/", "file:/C:/Program%20Files%20(x86)/Mozilla%20Firefox/", "XPCSafeJSObjectWrapper.cpp" ]; // TODO Get firefox location dynamically
	this.logger = logger;
	this.scriptInfo = {};
	this.newScripts = [];
	this.stackDepth = 0;
	this.stackInfo = {};

	this.jsd = Components.classes['@mozilla.org/js/jsd/debugger-service;1'].getService(this.jsdIDebuggerService);
	this.jsd.functionHook =
	{
		onCall : wfHook(this.onEnterLeave, this)
	};
	this.jsd.topLevelHook =
	{
		onCall : wfHook(this.onEnterLeave, this)
	};
	this.jsd.breakpointHook =
	{
		onExecute : wfHook(this.onBreakpoint, this)
	};
	/*this.jsd.interruptHook =
	{
		onExecute : wfHook(this.onInterrupt, this)
	};*/
	this.jsd.on();
	//this.jsd.flags = this.jsdIDebuggerService.ENABLE_NATIVE_FRAMES;
	this.jsd.scriptHook = 
	{
		onScriptCreated : wfHook(this.onScriptCreated, this),
		onScriptDestroyed : wfHook(this.onScriptDestroyed, this)
	};

	//this.c1 = this.c2 = this.c3 = this.c4 = 0;
}

WFDebugService.prototype =
{
	runOnNewDocument : 
		'window.wf_xhrs = [];\
		window.wf_its = [];\
		window.wf_nextITId = 1;\
		function wf_register() { }\
		function wf_overrideSetIT(name, desc)\
		{\
			var orig = name + "Method";\
			function wf_newSetIntervalMethod() { return wf_hiddenFuncNewSetITMethod.apply(this, arguments); }\
			function wf_newSetTimeoutMethod() { return wf_hiddenFuncNewSetITMethod.apply(this, arguments); }\
			function wf_hiddenFuncNewSetITMethod(it_func)\
			{\
				var id = window.wf_nextITId++;\
				window.wf_info = { call: true, itId: id, desc: "set-" + desc };\
				window.wf_register();\
				var func_for_real = it_func;\
				function wf_hiddenFuncNewITHandler() {\
					window.wf_info = { event: true, itId: id, desc: desc };\
					window.wf_register();\
					if (typeof func_for_real == "string" || typeof func_for_real == "String")\
						var result = window.eval(func_for_real);\
					else\
						var result = func_for_real.apply(this, arguments);\
					return result;\
				};\
				arguments[0] = wf_hiddenFuncNewITHandler;\
				var clearId = window[orig].apply(this, arguments);\
				self.wf_its[clearId] = id;\
				return clearId;\
			}\
			window[orig] = window[name];\
			window[name] = (name == "setInterval" ? wf_newSetIntervalMethod : wf_newSetTimeoutMethod);\
		}\
		function wf_overrideClearIT(name, desc)\
		{\
			var orig = name + "Method";\
			function wf_newClearIntervalMethod() { return wf_hiddenFuncNewClearITMethod.apply(this, arguments); }\
			function wf_newClearTimeoutMethod() { return wf_hiddenFuncNewClearITMethod.apply(this, arguments); }\
			function wf_hiddenFuncNewClearITMethod(clearId)\
			{\
				if (window.wf_its[clearId])\
				{\
					window.wf_info = { call: true, itId: window.wf_its[clearId], desc: "clear-" + desc };\
					window.wf_register();\
					delete window.wf_its[clearId];\
				}\
				return window[orig].apply(this, arguments);\
			}\
			window[orig] = window[name];\
			window[name] = (name == "clearInterval" ? wf_newClearIntervalMethod : wf_newClearTimeoutMethod);\
		}\
		function wf_hiddenFuncFindXhrObject(xhr)\
		{\
			for (var i = window.wf_xhrs.length - 1; i >= 0; i--)\
				if (window.wf_xhrs[i].xhr === xhr)\
					return window.wf_xhrs[i];\
			var xo = { xhr: xhr };\
			window.wf_xhrs.push(xo);\
			return xo;\
		}\
		function wf_overrideGetter(name)\
		{\
			var orig = name + "Getter";\
			function wf_hiddenFuncNewGetter()\
			{\
				window.wf_info = { call: true, xhrId: wf_hiddenFuncFindXhrObject(this).id, desc: name };\
				window.wf_register();\
				return this[orig]();\
			}\
			window.XMLHttpRequest.prototype[orig] = window.XMLHttpRequest.prototype.__lookupGetter__(name);\
			window.XMLHttpRequest.prototype.__defineGetter__(name, wf_hiddenFuncNewGetter);\
		}\
		function wf_hiddenFuncSetupSingleEventListener(xhr, name)\
		{\
			function wf_hiddenFuncXhrEventListener()\
			{\
				window.wf_info = { event: true, xhrId: wf_hiddenFuncFindXhrObject(xhr).id, desc: name };\
				window.wf_register();\
			}\
			xhr.addEventListener(name, wf_hiddenFuncXhrEventListener, true);\
		}\
		function wf_hiddenFuncSetupEventListeners(xhr)\
		{\
			var xo = wf_hiddenFuncFindXhrObject(xhr);\
			if (!xo.hasSetupEventListeners)\
			{\
				window.wf_hiddenFuncSetupSingleEventListener(xhr, "readystatechange");\
				xo.hasSetupEventListeners = true;\
			}\
		}\
		function wf_overrideEventSetter(name)\
		{\
			var orig = name + "Setter";\
			function wf_hiddenFuncNewEventSetter(val)\
			{\
				window.wf_hiddenFuncSetupEventListeners(this);\
				this[orig](val);\
			}\
			window.XMLHttpRequest.prototype[orig] = window.XMLHttpRequest.prototype.__lookupSetter__(name);\
			window.XMLHttpRequest.prototype.__defineSetter__(name, wf_hiddenFuncNewEventSetter);\
		}\
		function wf_overrideSendMethod()\
		{\
			function wf_newSendMethod()\
			{\
				window.wf_hiddenFuncSetupEventListeners(this);\
				return window.XMLHttpRequest.prototype["sendMethod"].apply(this, arguments);\
			}\
			window.XMLHttpRequest.prototype["sendMethod"] = window.XMLHttpRequest.prototype["send"];\
			window.XMLHttpRequest.prototype["send"] = wf_newSendMethod;\
		}\
		function wf_overrideEvalMethod()\
		{\
			function wf_newEvalMethod(expr)\
			{\
				window.wf_evalExpressionSource = expr.toString();\
				return window["evalMethod"].apply(this, arguments);\
			}\
			window["evalMethod"] = window["eval"];\
			window["eval"] = wf_newEvalMethod;\
		}\
		function wf_beginOverride() { }\
		window.wf_beginOverride();\
		window.wf_overrideGetter("responseText");\
		window.wf_overrideGetter("responseXML");\
		window.wf_overrideGetter("readyState");\
		window.wf_overrideGetter("status");\
		window.wf_overrideGetter("statusText");\
		window.wf_overrideEventSetter("onreadystatechange");\
		window.wf_overrideSendMethod();\
		window.wf_overrideEvalMethod();\
		window.wf_overrideSetIT("setInterval", "interval");\
		window.wf_overrideSetIT("setTimeout", "timeout");\
		window.wf_overrideClearIT("clearInterval", "interval");\
		window.wf_overrideClearIT("clearTimeout", "timeout");',

	onNewDocument : function(win)
	{
		var doc = win.document;
		win = win.wrappedJSObject;
		this.contentWindow = win;

		// Create a dummy page object that we can set properties on
		this.pageObject = new Object();
		this.pageObject.newPageScriptsComingUp = true;
		
		// Attach event listeners, so the debug service can later check which event was fired last
		this.lastEvent = "";
		this.addEventListener(win, "DOMContentLoaded");
		this.addEventListener(win, "pageshow");
		this.addEventListener(win, "pagehide");
		this.addEventListener(win, "load");
		this.addEventListener(win, "unload");
		this.addEventListener(win, "select");
		this.addEventListener(win, "change");
		this.addEventListener(win, "submit");
		this.addEventListener(win, "reset");
		this.addEventListener(win, "resize");
		this.addEventListener(win, "scroll");
		this.addEventListener(win, "focus");
		this.addEventListener(win, "blur");
		this.addEventListener(win, "textInput");
		this.addEventListener(win, "click");
		this.addEventListener(win, "dblclick");
		this.addEventListener(win, "mousedown");
		this.addEventListener(win, "mouseup");
		this.addEventListener(win, "mouseover");
		this.addEventListener(win, "mousemove");
		this.addEventListener(win, "mouseout");
		this.addEventListener(win, "keydown");
		this.addEventListener(win, "keyup");
	},

	onNewPageScripts : function()
	{
		this.logger.logNewPageScripts();
		this.pageObject.newPageScriptsComingUp = false;
		this.xhrDataHashCodes = {};
	},

	onXhrRequestCompleted : function(id, data)
	{
		if (!this.xhrDataHashCodes) return;
		var hash = wf_getStringHashCode(data);
		this.xhrDataHashCodes[hash] = id;
	},

	addEventListener : function(win, name)
	{
		var self = this;
		win.addEventListener(name, function(e)
		{
			self.logger.logCustom("WIN EVENT " + name + " ON " + e.target);			
			if (self.pageObject.newPageScriptsComingUp)
			{
				self.onNewPageScripts();
			}
			if (e.target == win)
				self.logger.logEvent(name, "window");
			else
				self.logger.logDomEvent(name, e.target);			
			self.lastEvent = name;
			e.wf_logged = true;
		}, true);

		// Some events only get dispatched to the document
		win.document.addEventListener(name, function(e)
		{			
			self.logger.logCustom("DOC EVENT " + name + " ON " + e.target + " --> " + !!e.wf_logged);
			if (e.wf_logged) return; // Already logged by window handler
			if (self.pageObject.newPageScriptsComingUp)
			{
				self.onNewPageScripts();
			}
			self.logger.logDomEvent(name, e.target);
			self.lastEvent = name;
		}, true);
	},

	onScriptCreated : function(script)
	{
		if (this.isUserUrl(script.fileName))
		{
			var info = { script: script, alreadyCalled: false };
			this.newScripts.push(script);
			this.scriptInfo[script.tag] = info;

			this.logger.logCustom("PARSING ==============> " + script.tag.toString());
			//this.logger.log("SCRIPT CREATED: {0}, file: {1}, func: {2}, start: {3}, length: {4}".format(script.tag, script.fileName, script.functionName, script.baseLineNumber, script.lineExtent + 1));
			// + "-------------------- Function Source --------------------\n" + script.functionSource);
			//
			//this.logger.log("--- Details --------------------------------------");
			//this.logger.log(script.toStringAllFields());
		}
	},

	onScriptDestroyed : function(script)
	{
		if (this.scriptInfo[script.tag])
		{
			//	this.logger.log('SCRIPT DESTROYED: ' + script.tag);
		}
	},

	onBreakpoint : function(stackFrame, callType, val)
	{
		if (this.scriptInfo[stackFrame.script.tag] && !this.scriptInfo[stackFrame.script.tag].alreadyCalled)
		{
			// Special breakpoint hit. Clear it and process new scripts.
			stackFrame.script.clearAllBreakpoints();
			this.onEnterNewScript(stackFrame);
		}
		return 1;
	},

	onEnterLeave : function(stackFrame, callType)
	{
		if (callType == this.jsdICallHook.TYPE_FUNCTION_CALL)
		{
			//this.c1++;
			this.onEnter(stackFrame);
		}
		else if (callType == this.jsdICallHook.TYPE_FUNCTION_RETURN)
		{
			//this.c2++;
			this.onLeave(stackFrame);
		}
		else if (callType == this.jsdICallHook.TYPE_TOPLEVEL_START)
		{
			//this.c3++;
			this.onEnter(stackFrame);
		}
		else if (callType == this.jsdICallHook.TYPE_TOPLEVEL_END)
		{
			//this.c4++;
			this.onLeave(stackFrame);			
		}
		else
			throw "Invalid call type in onEnterLeave: " + callType.toString();
		//this.logger.logCustom("COUNTERS: " + this.c1 + " " + this.c2 + " " + this.c3 + " " + this.c4);
	},

	onEnter : function(stackFrame)
	{
		var si = this.scriptInfo[stackFrame.script.tag];
		if (si)
		{
			var isNew = !this.scriptInfo[stackFrame.script.tag].alreadyCalled;
			if (isNew)
			{
				// See if we got the first script of a new page
				if (this.pageObject.newPageScriptsComingUp && this.stackDepth == 0)
				{
					this.onNewPageScripts();
				}

				// Can't call stackFrame.eval from enter/leave hooks because it crashes firefox.
				// So, set up break point which will be hit immediately.
				stackFrame.script.setBreakpoint(0);

				// Postpone the onEnter call.
				// 1. Copy stackFrame: construct a fake stackFrame object that contains all the details that are needed 
				// (because stackFrame objects are destroyed by the debugger interface after leaving this function).
				var copyStackFrame = function(frame, recurseOnce) {
					var result = { fake: true }; // For debugging purposes
					if (frame.script)
					{
						result.script = {};
						result.script.tag = frame.script.tag;
						result.script.functionName = frame.script.functionName;
					}
					result.line = frame.line;
					if (recurseOnce && frame.callingFrame) result.callingFrame = copyStackFrame(frame.callingFrame, false);
					return result;
				};
				var copy = copyStackFrame(stackFrame, true);

				// 2. Set callback handler.
				var self = this;
				this.onEnterNewScriptCallback = function() { self.onEnter(copy); };

				// 3. Wait for the magic to happen.
				return;
			}

			/*if (this.stackDepth == 0 && stackFrame.callingFrame)
			{
				// Firefox forgot to tell us there was another call before this one.
				// Reconstruct.
				this.onEnter(stackFrame.callingFrame);
				this.logger.logCustom("RECONSTRUCTED");
			}*/

			// Check whether the injected DOM script has registered new events (such as timeout, interval and XMLHttpRequest events) or call info.
			if (si.registerFunc)
			{
				var info = this.contentWindow["wf_info"];
				if (info.event)
				{
					if (info.xhrId)
						this.logger.logEvent("<xhr." + info.desc + ">", "", info.xhrId);
					else if (info.itId)
						this.logger.logEvent("<" + info.desc + ">", "", info.itId);
					else
						this.logger.logEvent("<strange event>");
				}
				else
				{
					this.logger.logCallInfo(info.desc, info.xhrId, info.itId);
				}
			}

			// Log the call.
			if (!si.hiddenFunc)
			{
				this.logger.logCall(this.stackDepth, stackFrame, stackFrame.callingFrame);
			}

			// Keep track of the stack.			
			this.stackInfo[this.stackDepth] = stackFrame.script.tag.toString();
			//this.logger.logfile.logWithSpacing("[" + this.stackDepth.toString() + "] = " + stackFrame.script.tag.toString(), this.stackDepth);
			this.stackDepth++;
		}
	},

	onEnterNewScript : function(stackFrame)
	{
		this.newScripts.forEach(function(s) { 
			this.scriptInfo[s.tag].alreadyCalled = true;
			if (s.functionName == 'wf_register')
			{
				this.scriptInfo[s.tag].registerFunc = true;
				this.scriptInfo[s.tag].hiddenFunc = true;
			}
			if (s.functionName.substr(0, 13) == 'wf_hiddenFunc')
				this.scriptInfo[s.tag].hiddenFunc = true;
		}, this);
		
		if (this.newScripts.length == 0)
		{
			this.logger.logError("No new scripts" + stackFrame.script.tag);
			return;
		}

		// This is based on the assumption that after loading a script, it's main "body" will be executed, which shows
		// up as a call to the function that was parsed last.
		var last = this.newScripts.pop(); // Exclude script on stackFrame (asserted below)
		var isNormalScript;
		var parent;

		if (last != stackFrame.script)
		{
			// However, some plugins mess up the above assumption, e.g. Flash player.
			parent = this.getNormalScriptDetailsFromScript(last);
			
			/*var a = [];
			this.newScripts.push(last);
			for (var i = this.newScripts.length - 1; i >= 0; i--)
				if (this.newScripts[i] != stackFrame.script)
					a.unshift(this.newScripts[i]);
			
			if (a.length == this.newScripts.length)
				this.logger.logError("ERROR,Can't find script");

			this.newScripts = a;*/
			//this.logger.logError("Expected different script object {0} vs {1}".format(last.functionName, stackFrame.script.functionName));
			//return;
			isNormalScript = true;
		}
		else if (this.isEvalScriptFrame(stackFrame))
		{
			parent = this.getEvalScriptDetails(stackFrame);
		}
		else if (this.isEventScriptFrame(stackFrame))
		{
			parent = this.getEventScriptDetails(stackFrame);
		}
		else
		{
			parent = this.getNormalScriptDetails(stackFrame);
			isNormalScript = true;
		}

		var all = this.newScripts.map(function(s) { return this.getChildScriptDetails(s, parent); }, this);
		all.push(parent);
		all.forEach(this.logger.logScriptCreated, this.logger);
		this.newScripts = [];

		for (var k = 0; k < all.length; k++)
			this.logger.logCustom("ACTUAL FILE: " + all[k].script.fileName);

		if (this.onEnterNewScriptCallback)
		{
			if (isNormalScript)
			{
				// Let trace consumers know that top-level execution of the script is going to follow.
				this.logger.logEvent("<toplevel-script>", "");
			}

			this.onEnterNewScriptCallback();
			delete this.onEnterNewScriptCallback;
		}
	},

	isEvalScriptFrame : function(frame)
	{
		return (frame.script.functionName == null || frame.script.functionName == '') && frame.callingFrame != null;
	},

	getEvalScriptDetails : function(frame)
	{
		var src = this.contentWindow["wf_evalExpressionSource"];
		if (!src) src = "[no eval expression found]";
		var xhrId;
		if (src && this.xhrDataHashCodes) xhrId = this.xhrDataHashCodes[wf_getStringHashCode(src)];

		return { 
			script : frame.script,
			sourceFile : "<eval>",
			sourceText : src,
			sourceXhr : xhrId,
			startLine : "ALL",
			endLine : 0,
			deltaLine : -(frame.script.baseLineNumber - 1)
		};
	},

	getChildScriptDetails : function(script, parent)
	{
		return {
			script : script,
			sourceFile : parent.sourceFile,
			sourceTextComingUp : !!parent.sourceText,
			sourceXhr : parent.sourceXhr,
			startLine : script.baseLineNumber,
			endLine : script.baseLineNumber + script.lineExtent,
			deltaLine : parent.deltaLine
		};
	},

	isEventScriptFrame : function(frame)
	{
		return this.evaluateInFrame("this instanceof Element", "isEventScriptFrame_isElement", frame);
	},

	getEventScriptDetails : function(frame)
	{
		var infoAttr = "wf_posinfo_for_" + frame.functionName;
		var expr = "this.hasAttribute('" + infoAttr + "') ? this.getAttribute('" + infoAttr + "') : ''";
		var posInfoStr = this.evaluateInFrame(expr, "getEventScriptDetails_getPosInfo", frame);
		if (posInfoStr)
		{
			start = "POS;" + posInfoStr;
			end = "";
		}
		else
		{
			var expr = "this.hasAttribute('" + frame.functionName + "') ? this.getAttribute('" + frame.functionName + "') : ''";
			var src = this.evaluateInFrame(expr, "getEventScriptDetails_getSrc", frame);
			if (src)
			{
				return {
					script : frame.script,
					sourceFile : "",
					sourceText : src,
					startLine : "ALL",
					endLine : 0,
					deltaLine : 0
				};
			}
			else
			{
				start = "UNKNOWN";
				end = 0;
			}
		}

		return {
			script : frame.script,
			sourceFile : frame.script.fileName,
			startLine : start,
			endLine : end,
			deltaLine : 0
		};
	},

	getNormalScriptDetails : function(frame)
	{
		return {
			script : frame.script,
			sourceFile : frame.script.fileName,
			startLine : frame.script.baseLineNumber,
			endLine : frame.script.baseLineNumber + frame.script.lineExtent,
			deltaLine : 0
		};
	},

	getNormalScriptDetailsFromScript : function(script)
	{
		return {
			script : script,
			sourceFile : script.fileName,
			startLine : script.baseLineNumber,
			endLine : script.baseLineNumber + script.lineExtent,
			deltaLine : 0
		};
	},

	evaluateInFrame : function(expr, desc, frame)
	{
		var result = {};
		if (frame.eval(expr, "eval_" + desc, 1, result))
			return result.value.getWrappedValue();
		else
			this.logger.logError("In eval");
	},

	onLeave : function(stackFrame)
	{
		var si = this.scriptInfo[stackFrame.script.tag];
		if (si)
		{
			if (!si.hiddenFunc)
			{
				this.logger.logReturn(stackFrame);
			}
			this.stackDepth--;

			/*var fn = stackFrame.script.tag;
			if (this.stackInfo[this.stackDepth - 1] == fn)
			{
				this.logger.logReturn(stackFrame);
				this.stackDepth--;
			}
			else
			{
				//this.logger.logError("Negative stack! ({0} @ depth {1} unmatched got {2})".format(this.stackInfo[this.stackDepth - 1], this.stackDepth - 1, fn));
				this.logger.logReturn(stackFrame);
				this.stackDepth--;
			}*/
		}
	},


	/*
	onEnter : function(stackFrame)
	{
		var fn = this.getFunctionName(stackFrame);
		this.stackInfo[this.stackDepth] = fn;

		if (stackFrame.script != null && this.scriptInfo[stackFrame.script.tag])
		{
			//var s = "";
			//var c = -1;
			//do
			//{	
			//	s += this.stackFrameToString(stackFrame) + " ### ";
			//	stackFrame = stackFrame.callingFrame;
			//	c++;
			//}
			//while (stackFrame != null);
			//this.logger.logWithSpacing(this.stackDepth.toString() + "/" + c.toString() + " " + s, this.stackDepth);

			this.logger.logWithSpacing(this.stackFrameToString(stackFrame), this.stackDepth);
		}
		
		this.stackDepth++;
	},

	stackFrameToTagLine : function(stackFrame)
	{
		return "(tag: {0}, line: {1})".format(stackFrame.script != null ? stackFrame.script.tag : "???", stackFrame.line);
	},

	onLeave : function(stackFrame)
	{
		var fn = this.getFunctionName(stackFrame);
		if (this.stackInfo[this.stackDepth - 1] == fn)
		{
			if (stackFrame.script != null && this.scriptInfo[stackFrame.script.tag])
				this.logger.logWithSpacing("RETURN " + this.stackFrameToTagLine(stackFrame), this.stackDepth);
			this.stackDepth--;
		}
		else
		{
			this.logger.log("ERROR Negative stack! ({0} @ depth {1} unmatched, got {2})", this.stackInfo[this.stackDepth - 1], this.stackDepth - 1, fn);
		}
	},*/

	/*onInterrupt : function(stackFrame, callType, val)
	{
		if (stackFrame.script != null)
		{
			if (this.scriptInfo[stackFrame.script.tag])
			{
				var script = this.scriptInfo[stackFrame.script.tag];
				var line = script.pcToLine(stackFrame.pc, 1);
				var linePp = script.pcToLine(stackFrame.pc, 2);

				//this.logger.log("Tag: {0}, func: {1}, line: {2}, line: {3}, line (pp): {4}, pc: {5}".format(stackFrame.script.tag, stackFrame.functionName, stackFrame.line, line, linePp, stackFrame.pc));
				this.logger.logWithSpacing("... " + this.stackFrameToTagLine(stackFrame), this.stackDepth);
			}
		}
		else
			this.logger.log("ERROR Weird stack frame (onInterrupt)");
		return 1;
	},*/

	isUserUrl : function(url)
	{
		for (var i = 0; i < this.filterUrls.length; i++)
		{
			filter = this.filterUrls[i];
			if (url.substr(0, filter.length) == filter)
				return false;
		}
		return true;
	}
};


