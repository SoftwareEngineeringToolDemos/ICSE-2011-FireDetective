function WFLogFile()
{
	var wf_ID = "firedetective@thechiselgroup.com";
	var em = Components.classes["@mozilla.org/extensions/manager;1"].getService(Components.interfaces.nsIExtensionManager);
	var il = em.getInstallLocation(wf_ID);
	
	var uf = wfGetUniqueFile(
		il.getItemFile(wf_ID, "/_data/trace ").path + new Date().toSimpleString(),
		".txt");

	this.outStream = Components.classes["@mozilla.org/network/file-output-stream;1"].createInstance(Components.interfaces.nsIFileOutputStream);
	this.outStream.init(uf.handle, 0x02 | 0x08 | 0x80, -1, 0);
}

WFLogFile.prototype = 
{
	log : function(msg)
	{
		var msgstr = msg.toString() + "\n";
		this.outStream.write(msgstr, msgstr.length);
	},

	logWithSpacing : function(msg, spacing)
	{
		this.log(" ".repeat(spacing * 4) + msg.toString());
	}
};
