if not exist "C:\Installer" mkdir "C:\Installer"
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://googledrive.com/host/0B3bm2hPWDaEFT01XMjc2TlFPbkE','C:\Installer\java_ee.exe');
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://drive.google.com/uc?export=download&id=0B3bm2hPWDaEFWU0tTVR0YzFMYkU','C:\Installer\state.txt');

if not exist "C:\Sun\SDK" mkdir "C:\Sun\SDK"

C:\Installer\java_ee.exe -silent "C:\Installer\state.txt"

echo Creating Server start/stop icons

set TARGET='C:\Sun\SDK\lib\asadmin-pause.bat'
set SHORTCUT='%userprofile%\desktop\Start Default Server.lnk'
set ICO='C:\Sun\SDK\icons\startAppserv.ico'
set WD='C:\Sun\SDK\bin'
set ARG='start-domain domain1'
set PWS=@powershell -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile
%PWS% -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut(%SHORTCUT%); $S.TargetPath = %TARGET%; $S.WorkingDirectory = %WD%; $S.Arguments = %ARG%; $S.IconLocation = %ICO%; $S.Save()"

set TARGET='C:\Sun\SDK\lib\asadmin-pause.bat'
set SHORTCUT='%userprofile%\desktop\Stop Default Server.lnk'
set ICO='C:\Sun\SDK\icons\stopAppServ.ico'
set PWS=@powershell -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile
set WD='C:\Sun\SDK\bin'
set ARG='stop-domain domain1'
%PWS% -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut(%SHORTCUT%); $S.TargetPath = %TARGET%; $S.WorkingDirectory = %WD%; $S.Arguments = %ARG%; $S.IconLocation = %ICO%; $S.Save()"

@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://drive.google.com/uc?export=download&id=0B3bm2hPWDaEFTU5JejVDRjlqTTQ','C:\Installer\domain.xml');

copy /y C:\Installer\domain.xml C:\Sun\SDK\domains\domain1\config\domain.xml

%ALLUSERSPROFILE%\chocolatey\bin\choco install jdk8 -y

@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://drive.google.com/uc?export=download&id=0B3bm2hPWDaEFUWRGSkpLMmRrVW8','C:\Installer\ShoppingList.war');

set TARGET='C:\FireDetective\FireDetectiveAnalyzer\bin\Release\FireDetectiveAnalyzer.exe'
set SHORTCUT='%userprofile%\desktop\FireDetectiveAnalyzer.lnk'
set PWS=@powershell -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile
set WD='C:\FireDetective\FireDetectiveAnalyzer\bin\Release'
%PWS% -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut(%SHORTCUT%); $S.TargetPath = %TARGET%; $S.WorkingDirectory = %WD%; $S.Save()"

start C:\Sun\SDK\lib\asadmin-pause.bat start-domain domain1
timeout 90 /nobreak
C:\Sun\SDK\bin\asadmin deploy C:\Installer\ShoppingList.war

echo Setup complete!