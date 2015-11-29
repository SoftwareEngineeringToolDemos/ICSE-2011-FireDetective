echo Getting 7zip

@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/SoftwareEngineeringToolDemos/ICSE-2011-FireDetective/raw/master/build-vm/files/7z.msi','C:\Installer\7z.msi');
msiexec /i C:\Installer\7z.msi /qn

echo Downloading and installing FireDetective

if not exist "C:\Installer" mkdir "C:\Installer"

@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/SoftwareEngineeringToolDemos/ICSE-2011-FireDetective/raw/master/build-vm/files/FireDetective0.1.zip','C:\Installer\FireDetective0.1.zip');

set PATH=%PATH%;C:\Program Files\7-Zip\
7z x "C:\Installer\FireDetective0.1.zip" -oc:\FireDetective

echo Creating desktop shortcut to FireDetective

set TARGET='C:\FireDetective'
set SHORTCUT='%userprofile%\desktop\FireDetective.lnk'
set PWS=@powershell -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile

%PWS% -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut(%SHORTCUT%); $S.TargetPath = %TARGET%; $S.Save()"

C:\Program Files\Mozilla Firefox\firefox.exe

echo Installing addon

cd %APPDATA%\Mozilla\Firefox\Profiles\
cd *.default
if not exist extensions mkdir extensions
cd extensions
echo|set /p="C:\FireDetective\FireDetectiveAddOn\">firedetective@thechiselgroup.com

echo Updating Firefox preferences

cd ..
taskkill /f /im firefox.exe /fi "memusage gt 2"
echo user_pref("network.http.use-cache", false);>>prefs.js

echo Addon has been set up in Firefox
