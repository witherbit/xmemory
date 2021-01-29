# xmemory
**Disclaimer: the author is not responsible for possible side effects associated with the use of xmemory**
____
Connect System.Diagnostics:
```c#
using System.Diagnostics;
```
And initialize the Memory class
```c#
Memory _memory = new Memory(processname);
```
Using ProcessModule
```c#
ProcessModule _module = _memory.Process.GetModule(modulename);
```
