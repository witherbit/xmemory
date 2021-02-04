# xmemory
**Disclaimer: the author is not responsible for any possible side effects associated with the use of "xmemory", as well as for the use of "xmemory" in illegal and other cases.**
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
