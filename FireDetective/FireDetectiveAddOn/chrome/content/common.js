// General utilities

String.prototype.format = function()
{
	var pattern = /\{\d+\}/g;
	var args = arguments;
	return this.replace(pattern, function(capture){ return args[capture.match(/\d+/)]; });
}

String.prototype.repeat = function(num)
{
	//return new Array(num + 1).join(this);
	//return "";
	var result = "";
	for (var i = 0; i < num; i++)
		result += this;
	return result;
}

Number.prototype.toStringPadZeroes = function(width)
{
	var str = this.toString();
	while (str.length < width)
		str = '0' + str;
	return str;
}

function wfAlertObjectDetails(obj)
{
	var result = ""; for (key in obj) { result += key.toString();
		try { result += " = " + obj[key].toString() + "\n"; } catch (ex) { result += "[exception]\n"; }
	}
	alert(result);
}

Date.prototype.toSimpleString = function()
{
	return "{0}-{1}-{2} {3}.{4}".format(
		this.getDate().toStringPadZeroes(2),
		(this.getMonth() + 1).toStringPadZeroes(2),
		this.getFullYear().toStringPadZeroes(4),
		this.getHours().toStringPadZeroes(2),
		this.getMinutes().toStringPadZeroes(2));
}

// String hash code (djb2)

function wf_getStringHashCode(str)
{
	var hash = 5381;
	for (var i = 0; i < str.length; i++)
		hash = ((hash << 5) + hash) + str.charCodeAt(i);
	return hash;
}

// Firefox console

function WFConsoleImpl()
{
	this.cs = Components.classes["@mozilla.org/consoleservice;1"].getService(Components.interfaces.nsIConsoleService);
}

WFConsoleImpl.prototype =
{
	log : function(msg)
	{
		this.cs.logStringMessage(msg);
	}
};

var WFConsole = new WFConsoleImpl();

// URI parsing

function WFUri(uri)
{
	this.uri = uri;
}

WFUri.prototype =
{
	hasAnchor : function()
	{
		return this.uri.indexOf("#") >= 0;
	},

	withoutAnchor : function()
	{
		var index = this.uri.indexOf("#");
		return index >= 0 ? this.uri.substr(0, index) : this.uri;
	}
};

// File I/O

function wfGetFile(filename)
{
	file = Components.classes["@mozilla.org/file/local;1"].createInstance(Components.interfaces.nsILocalFile);
	file.initWithPath(filename);
	return file;
}

function wfGetUniqueFile(base, ext)
{
	var file;
	var filename = base + ext;
	var n = 1;
	do
	{
		file = wfGetFile(filename);
		if (file.exists())
		{
			n++;
			filename = base + " (" + n.toString() + ")" + ext;
		}
		else
			break;
	}
	while (true);
	return { name: filename, handle: file };
}

// Hooking events

function wfHook(fn, thisArg)
{
	return function()
	{
		try
		{
			return fn.apply(thisArg, arguments);
		}
		catch (ex)
		{
			wfAlertObjectDetails(ex);
		}
	}
}

// Assert

function wfAssert(expr)
{
	try
	{
		if (!expr) null();
	}
	catch (ex)
	{
		ex.message = "Assertion failed";
		throw ex;
	}
}

// An actual hash table that allows us to store objects

function WFObjectHashTable(hashfunc) // Note: hashfunc only used for speedup (hashfunc : Object -> Number/String)
{
	this.hashfunc = hashfunc ? hashfunc : function() { return 0; }
	this.bucket = {};
}

WFObjectHashTable.prototype = 
{
	add : function(key, val)
	{
		var hash = this.hashfunc(key);
		if (!this.bucket[hash])
			this.bucket[hash] = [];
		var data = this.bucket[hash];		
		data.push({ key: key, val: val});
	},

	get : function(key)
	{
		var hash = this.hashfunc(key);
		var data = this.bucket[hash];
		if (data)
		{
			for (var i = data.length - 1; i >= 0; i--)
				if (data[i].key === key)
					return data[i].val;
		}
	},

	remove : function(key)
	{
		var hash = this.hashfunc(key);
		var data = this.bucket[hash];
		if (data)
		{
			for (var i = data.length - 1; i >= 0; i--)
				if (data[i].key === key)
				{
					data.splice(i, 1);
					return;
				}
		}
	}
};

// Text writer

function WFTextWriter()
{
	this.data = [];
}

WFTextWriter.prototype =
{
	write : function(str)
	{
		this.data.push(str);
	},

	readAll : function()
	{
		var result = this.data.join("");
		this.data = [];
		return result;
	}
};

// Text reader

function WFTextReader()
{
	this.text = "";
	this.position = 0;
	this.positionAfterLastMatch = 0;
	this.matchedUntilEnd = false;
}

WFTextReader.prototype =
{
	write : function(str)
	{
		this.text += str;
		this.matchedUntilEnd = false;
	},

	match : function(re)
	{
		var result = this.text.substr(this.positionAfterLastMatch).match(re);
		if (result)
		{
			this.matchUntilPos(this.positionAfterLastMatch + result.index + result[0].length);
		}
		else
		{
			this.position = this.text.length;
			this.matchedUntilEnd = true;
		}
		return result;
	},

	tryMatch : function(re)
	{
		var result = this.text.substr(this.positionAfterLastMatch).match(re);
		if (result)
			this.matchUntilPos(this.positionAfterLastMatch + result.index + result[0].length);
		return result;
	},

	matchSimple : function(re)
	{
		return this.match(re);
	},

	testMatch : function(re)
	{		
		return re.test(this.text.substr(this.positionAfterLastMatch));
	},

	matchUntilPos : function(pos)
	{
		this.positionAfterLastMatch = pos;
		if (pos > this.position)
			this.position = pos;
	},

	findFirst : function(values)
	{
		var minPos;
		var minWhich = -1;

		for (var i = 0; i < values.length; i++)
		{
			if (typeof values[i] == "string")
				var pos = this.text.indexOf(values[i], this.positionAfterLastMatch);
			else
				var pos = values[i](this.text, this.positionAfterLastMatch, minPos ? minPos : this.text.length);

			if (pos >= 0)
			{
				if (minWhich < 0 || pos < minPos)
				{
					minWhich = i;
					minPos = pos;
				}
			}
		}

		return { which: minWhich, index: minPos };
	},

	getText : function(start, length)
	{
		//alert(start + "--" + length);
		return this.text.substr(start, length);
	}
};



// Run processes

/*
function wf_runProcess(process, args)
{
	try
	{
		var file = wf_getFile(process);
		var process = Components.classes["@mozilla.org/process/util;1"].createInstance(Components.interfaces.nsIProcess);
		process.init(file);
		process.run(false, args, args.length);
	}
	catch (ex)
	{
		alert(ex);
	}
}*/

