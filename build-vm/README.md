# Instructions for building VM
This folder contains the files needed to build a virtual machine for FireDetective using Vagrant.
***

#### Installation Steps

1. Download and install [Vagrant](https://www.vagrantup.com/downloads.html).
2. Download and install [VirtualBox](https://www.virtualbox.org/wiki/Downloads).
3. Clone [this repository](https://github.com/SoftwareEngineeringToolDemos/ICSE-2011-FireDetective) to your system.
4. Open command prompt/terminal and navigate to the _build-vm_ directory.
5. Run the command "_vagrant up_" to set up and launch the VM. A VirtualBox VM should be launched, as the GUI option has been enabled in Vagrant.
6. For the first launch, allow the provisioning tasks to complete execution. These will install the required software for FireDetective.

#### Provisioning

The following software are installed onto a Windows 7 box:

1. [Chocolatey](https://chocolatey.org/), a package manager for Windows
2. [Mozilla Firefox](https://www.mozilla.org/en-US/firefox/new/)
3. [7zip](http://www.7-zip.org/)
4. [The FireDetective Plug-in](http://swerl.tudelft.nl/bin/view/Main/FireDetective)
5. [Java EE5 SDK](http://www.oracle.com/technetwork/java/javaee/overview/index.html)

#### Windows Box

The box image used is a Windows 7 32-bit image from [modernIE/w7-ie11](https://atlas.hashicorp.com/modernIE/boxes/w7-ie11). The license for the OS is valid till 01/20/2016. The user credentials for the VM are (note that these are not required to access the system):

* Username: IEUser
* Password: Passw0rd!

#### References

* [Microsoft MSIExec documentation](https://technet.microsoft.com/en-us/library/cc759262%28v=ws.10%29.aspx)
* [3 ways to download files with PowerShell](https://blog.jourdant.me/3-ways-to-download-files-with-powershell/)
* [Vagrant Provision Reboot Plugin](https://github.com/exratione/vagrant-provision-reboot)
* [Making a shortcut with CMD](http://superuser.com/questions/392061/how-to-make-a-shortcut-from-cmd)
