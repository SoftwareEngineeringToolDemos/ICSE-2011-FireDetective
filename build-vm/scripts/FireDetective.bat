echo Getting 7zip

%ALLUSERSPROFILE%\chocolatey\bin\choco install 7zip --force -y

echo Downloading and installing FireDetective

if not exist "C:\Installer" mkdir "C:\Installer"

@powershell -NoProfile -ExecutionPolicy unrestricted -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/SoftwareEngineeringToolDemos/ICSE-2011-FireDetective/raw/master/build-vm/files/FireDetective0.1.zip','C:\Installer\FireDetective0.1.zip');

7z x "D:\SE\clone\ICSE-2011-FireDetective\build-vm\files\FireDetective0.1.zip"

