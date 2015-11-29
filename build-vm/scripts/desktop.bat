echo Getting text files to desktop

@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/SoftwareEngineeringToolDemos/ICSE-2011-FireDetective/raw/master/build-vm/files/Readme%20-%20FireDetective.txt','%userprofile%\desktop\Readme - FireDetective.txt');
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/SoftwareEngineeringToolDemos/ICSE-2011-FireDetective/raw/master/build-vm/files/Installation.txt','%userprofile%\desktop\Installation.txt');
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/SoftwareEngineeringToolDemos/ICSE-2011-FireDetective/raw/master/build-vm/files/Licenses.txt','%userprofile%\desktop\Licenses.txt');

echo Creating YouTube video shortcut

(
echo [InternetShortcut]
echo URL=http://tiny.cc/firedetective
) >"%userprofile%\desktop\PAT-RTS Demo Video.url"