# AO2Sharp
 Attorney Online 2 Server Implementation in C# .NET Core

# Progress
These packets are implemented:

- [X] HI
- [X] ID
- [X] askchaa
- [X] RC
- [X] RE
- [X] RM
- [X] RD
- [X] PW
- [X] CharsCheck
- [X] CC
- [X] MS
- [X] MC
- [ ] RT
- [X] CT
- [X] HP
- [ ] PE
- [ ] DE
- [ ] EE
- [ ] SETCASE
- [ ] CASEA
- [ ] ZZ
- [X] CH

# TODO
Some shit that needs to get done

- [ ] Become more thread safe (Many things are very prone to multithreaded errors)
- [ ] Plugins
- [X] Areas
- [X] Commands (Some commands are in but need more)
- [X] Logging architecture
- [X] Database
- [ ] More shit lol

# Plugins

I plan to have a system where you build a class library with a reference to AO2Sharp.dll, and make a class that inherits from PluginBase.

This class will have overrides for some server functions, and it's constructor will be called after server initialization. It'll also let you
define more packet handlers and commands, and even override existing ones. Still trying to brainstorm how this will work and figure out
how to actually execute it because im not super big brained at reflection.