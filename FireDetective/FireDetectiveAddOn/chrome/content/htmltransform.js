
function WFHtmlTransformer(textInput, textOutput, injectScript)
{
	this.input = textInput;
	this.output = textOutput;
	this.injectScript = "<script> /* BEGIN INJECTED SCRIPT */ " + injectScript + " /* END INJECTED SCRIPT */ </script>";
	this.injectScriptDone = false;
}

WFHtmlTransformer.prototype =
{
	transform : function(logger)
	{
		//var pos = this.input.position;
		//this.input.matchUntilPos(this.input.text.length);
		//var newPos = this.input.position;
		//this.output.write(this.input.getText(pos, newPos - pos));

		while (!this.input.matchedUntilEnd)
		{
			//logger.logCustom("POS: " + this.input.positionAfterLastMatch);
			//logger.logCustom(this.inComment + " " + this.inScript + " " + this.inString + " " + this.inStringAlt + 
			var pos = this.input.position;
			var inject, injectBefore;

			if (this.inComment)
			{
				if (this.input.matchSimple(/-->/))
					this.inComment = false;
			}
			else if (this.inScript)
			{
				if (this.input.matchSimple(/<\/script>/i))
					this.inScript = false;
			}
			else if (this.inString)
			{
				if (this.input.matchSimple(/"/))
					this.inString = false;
			}
			else if (this.inStringAlt)
			{
				if (this.input.matchSimple(/'/))
					this.inStringAlt = false;
			}
			else if (this.inTag)
			{
				if (!this.ignoreNextOnAttr)
					var info = this.input.findFirst([">", '"', "'", function(s, begin, end) { var result = s.substring(begin - 1, end).search(/\Won/i); return (result >= 0) ? (result + begin) : -1; } ]);
				else
					var info = this.input.findFirst([">", '"', "'"]);
				this.ignoreNextOnAttr = false;
				//alert(this.input.text.substr(this.input.positionAfterLastMatch));
				//wfAlertObjectDetails(info);

				if (info.which == 0)
				{
					this.input.matchSimple(/>/);
					if (this.inHeadTag && !this.injectScriptDone)
					{
						inject = this.injectScript;
						this.injectScriptDone = true;
					}
					this.inTag = this.inHeadTag = false;
				}
				else if (info.which == 1)
				{
					this.input.matchSimple(/"/);
					this.inString = true;
				}
				else if (info.which == 2)
				{
					this.input.matchSimple(/'/);
					this.inStringAlt = true;
				}
				else if (info.which >= 3)
				{
					this.input.matchUntilPos(info.index);

					var m = this.input.tryMatch(/^(on[a-z]+)(\s*=\s*)("[^"]*"|'[^']*')/i);
					if (m)
					{
						var attrName = m[1];
						var begin = info.index + m[1].length + m[2].length + 1;
						var end = info.index + m[0].length - 1;
						inject = " wf_posinfo_for_" + attrName + "='" + begin + ";" + end + "'";
					}
					else
					{
						if (this.input.testMatch(/^on[a-z]+\s*=\s*("|')[^\n]*$/i) || !this.input.testMatch(/>/))
						{
							// There might still be a chance to detect the property after new content has been read. So skip and revisit later.
							this.input.match(/^a/); // We know the first letter is "o", so this will cause the match to fail and let us break out of the while (bit of a hack, maybe cleanup).
						}
						else
						{
							// Well this is weird, move on.
							this.ignoreNextOnAttr = true;
						}
					}
				}
				else
				{
					this.input.matchSimple(">");
				}
			}
			else
			{
				var m = this.input.match(/<((!--)|(script(\s|>))|(head(\s|>))|([^> \f\n\r\t\v\u00A0\u2028\u2029]+(\s|>)))/i);
				if (m)
				{
					this.inComment = !!m[2];
					this.inScript = !!m[3];
					this.inHeadTag = !!m[5];
					this.inTag = !!m[5] || !!m[7];

					if (this.inScript && !this.injectScriptDone)
					{
						injectBefore = this.injectScript;
						this.injectScriptDone = true;
					}

					var lastCh = m[0][m[0].length - 1];
					if (this.inTag && lastCh == ">")
					{
						if (this.inHeadTag && !this.injectScriptDone)
						{
							inject = this.injectScript;
							this.injectScriptDone = true;
						}
						this.inTag = this.inHeadTag = false;
					}
				}
			}

			if (injectBefore)
			{
				this.output.write(injectBefore);
				injectBefore = undefined;
			}

			var newPos = this.input.position;
			this.output.write(this.input.getText(pos, newPos - pos));

			if (inject)
			{
				this.output.write(inject);
				inject = undefined;
			}
		}
	}

};

/*


ss.write( data );

ht.transform();

sb.readAll();

*/
