echo Getting text files to desktop

copy "C:\vagrant\files\Readme-FireDetective.txt" "%userprofile%\desktop\Readme-FireDetective.txt"
copy "C:\vagrant\files\Installation.txt" "%userprofile%\desktop\Installation.txt"
copy "C:\vagrant\files\Licenses.txt" "%userprofile%\desktop\Licenses.txt"

echo Creating YouTube video shortcut

(
echo [InternetShortcut]
echo URL=http://tiny.cc/firedetective
) >"%userprofile%\desktop\FireDetective Demo Video.url"