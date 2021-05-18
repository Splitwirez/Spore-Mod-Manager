# Spore Mod Manager

## This software is presently UNDER CONSTRUCTION, and is not stable. Do not install it yet, unless you're seeking to contribute to it.
(This readme is also still under construction)
<br><br>

### What is the Spore Mod Manager?
The **Spore Mod Manager** is a .NET-based mod manager for the game [Spore](http://www.spore.com/), as the name implies. It aims to be as user-friendly as possible, and to be more maintainable than its predecessor, the [Spore ModAPI Launcher Kit](http://davoonline.com/sporemodder/rob55rod/ModAPI/Public/). Upon its completion and release, it will be rolled out to all existing Spore ModAPI Launcher Kit users as an optional update, and will be offered to new users in place of the Spore ModAPI Launcher Kit.
<br><br>

### What does the Spore Mod Manager run on?
The Spore Mod Manager is built to run on any PC running either:
- Windows 7 or newer
- Linux with WINE 6.0 or newer

If you somehow manage to get it up and running on anything else, I'd love to know.
<br><br>

### What language is the Spore Mod Manager written in?
The Spore Mod Manager is written in C#, with a side of WPF XAML sprinkled in, all targeting .NET Core 3.1 (specifically 3.1.12).
<br><br>

### Why are the Test builds on the GitHub Releases page not marked as Pre-Release?
This is because the Spore Mod Manager's automatic update functionality does not take releases marked as Pre-Release into consideration, and should not be interpreted as a sign of stability sufficient for general use. This will not continue once the Spore Mod Manager is stable and released to the public, for obvious reasons.
<br><br>

### Why run under WINE instead of running natively on Linux?
Before you try to explain to me why I should build software that runs natively on Linux instead of relying on WINE, I'd like to let you know that everything else I am actively working on already aims to do so wherever possible. The Spore Mod Manager differs in this regard, predominantly because injecting compiled Windows code (e.g. from mods) into a closed-source Windows program (Spore) is a task most simply performed by another Windows program (the Spore Mod Manager).
