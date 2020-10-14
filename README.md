# AO2Sharp
Attorney Online 2 Server Implementation in C# .NET Core
 
# What is it?
After having ran an Attorney Online server, I despise the current experience of maintaining an active server
for this game. So I thought it would be fun to try and make one myself, to see if I can do something better.

# Prerequisites
Windows:

 - All you need is the [.NET Core 3.1 Runtime](https://cutt.ly/netcore31) installed.

Linux/Unix:
 
 - Determine how to install the dotnet core runtime on your distribution: 
 [here](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu) 
 or [here](https://wiki.archlinux.org/index.php/.NET_Core#Installation) are a few.

# Usage
To run the most basic possible server, all you have to do is download the latest 
[Release](https://github.com/ElijahZAwesome/AO2Sharp/releases/).
It will make the needed config files for you and set up defaults for you.

Double click or run this in the console (currently no command line arguments):

```
AO2Sharp
```

# Plugins (Actual wiki coming soon)
Surprisingly, this thing supports plugins! To make one, just make a new .Net Core 
Class Library that targets 'Core 3.1, and reference the `AO2Sharp.Plugins.API` 
project of this repository, or install this 
[NuGet package](https://www.nuget.org/packages/AO2Sharp.Plugins.API/).

Then, make a class that inherits from `Plugin`, and implement it's required members.
You should now have a barebones working plugin! Build it and put the .dll in the `Plugins`
folder of AO2Sharp, and restart the server.

# Progress
These packets are not implemented:

- [ ] SETCASE
- [ ] CASEA
- [ ] Slow loading packets (needed because client is bugged lol)

# TODO
Some shit that needs to get done

- [ ] Clean up some of this shit code
- [ ] Become more thread safe (Maybe?)
- [X] Plugins
- [X] Areas
- [X] Commands (Some commands are in but need more)
- [X] Logging architecture
- [X] Database
- [ ] Implement standard commands
- [ ] Test/Stress test this bitch
- [ ] Document server
- [ ] Document Plugin API