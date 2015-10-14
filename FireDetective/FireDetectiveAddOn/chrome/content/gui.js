function WFGuiImpl()
{
}

WFGuiImpl.prototype =
{
	setMessage : function(message, isError)
	{
		document.getElementById('firedetective_message_label').value = message;
		document.getElementById('firedetective_message_label').setAttribute('errorflag', isError ? 'true' : '');
	},

	enableButtons : function(start, stop)
	{
		document.getElementById('firedetective_start_button').disabled = start ? '' : 'true';
		document.getElementById('firedetective_stop_button').disabled = stop ? '' : 'true';
	},

	setStateInitial : function()
	{
		this.enableButtons(false, false);
		this.setMessage("Trace consumer not connected.", true);
	},
	
	setStateConnected : function()
	{
		this.enableButtons(true, false);
		this.setMessage('Trace consumer is recording.');
	},

	/*
	setStateIsRecording : function()
	{
		this.enableButtons(false, true);
		this.setMessage("Now tracing!");
	},

	setStateSynchronizing : function()
	{
		this.enableButtons(false, false);
		this.setMessage("Synchronizing...");
	},*/

	setStateMarked : function()
	{
		this.enableButtons(true, true);
		this.setMessage("Trace consumer is recording a marked section.");
	}
}

var WFGui = new WFGuiImpl();

function firedetective_start_button_click()
{
	WFGui.setStateMarked();
	WFService.consumer.startMark();
}

function firedetective_stop_button_click()
{
	WFGui.setStateConnected();
	WFService.consumer.stopMark();
}

/*function firedetective_connect_server_button_click()
{
	prompt('Url of the server side tracer? (note: not the web application itself)');
	document.getElementById('firedetective_connect_server_button').checked = true;
}

function firedetective_rename_button_click()
{
	alert("d");
}

function firedetective_open_button_click()
{
	alert("e");
}

function firedetective_open_folder_button_click()
{
}*/
