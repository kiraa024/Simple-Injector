# Simple DLL Injector

This is a basic DLL injector written in C#. It uses standard Windows APIs to inject a DLL into a target process using LoadLibrary. This tool is intended for educational and testing purposes only.

Features
- Opens a target process by PID
- Allocates memory in the target process
- Writes the DLL path into the target process
- Uses CreateRemoteThread to call LoadLibraryA
- Logs each step to the console

Requirements
- Windows
- .NET Framework
- A DLL compiled for the target process architecture (x86 or x64)

Usage
1. Build the project in Visual Studio.
2. Run the injector from the command line:
   SimpleInjector.exe <PID> <FullPathToDLL>
3. Confirm that the injector prints each step and reports success.

Example:
SimpleInjector.exe 1234 C:\MyDLLs\Test.dll

Demo Video:
https://github.com/user-attachments/assets/8d8cdb4c-2289-4ba7-9dab-5a149fa58b7e

Notes
- The target process must be running.
- The injector must match the process architecture (32‑bit vs 64‑bit).
- This does not support injecting into protected or anti‑cheat environments.
- Use responsibly on applications you own or have permission to test.

Inspiration and Credits
This project was inspired by and adapted from common DLL injection techniques, including code and concepts found in:
- https://github.com/ihack4falafel/DLL-Injection

Disclaimer
For educational purposes only. The author is not responsible for misuse.
