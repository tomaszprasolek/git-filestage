# git-filestage

Simple tool to stage and unstage whole files.

[![Build status](https://prasol.visualstudio.com/git-filestage/_apis/build/status/git-filestage-CI)](https://prasol.visualstudio.com/git-filestage/_build/latest?definitionId=1)
[![](https://img.shields.io/nuget/v/git-filestage.svg)](https://www.nuget.org/packages/git-filestage/)

![](docs\git-filestage.gif)

## Installation

	dotnet tool install -g git-filestage

## Features
- Add whole selected file to staging area.
- Unstage the selected file.
- Checkout selected file to discard made changes.

## Keyboard shortcuts

| Shortcut          | Description                                                                                                                                                                                           |
|-------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| <kbd>↑</kbd>      | Select the previous file.                                                                                                                                                                             |
| <kbd>↓</kbd>      | Select the next file.                                                                                                                                                                                 |
| <kbd>ENTER</kbd>  | - When file is in working directory, will be added to staging area.<br> - When file is in staging area, will be unstaged.<br> - When file is untracked, start tracked file and added to staging area. |
| <kbd>R</kbd>      | Checkout selected file to discard made changes.                                                                                                                                                       |
| <kbd>ESCAPE</kbd> | Close application, return to command line.                                                                                                                                                            |
| <kbd>Q</kbd>      | Close application, return to command line.                                                                                                                                                            |