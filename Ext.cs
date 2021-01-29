using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    public static class Ext
    {
		public static ProcessModule GetModule(this Process process, string moduleName)
		{
			bool flag = process == null;
			if (flag)
			{
				throw new ArgumentNullException("process");
			}
			bool flag2 = string.IsNullOrWhiteSpace(moduleName);
			if (flag2)
			{
				throw new ArgumentNullException("moduleName");
			}
			return process.Modules.Cast<ProcessModule>().FirstOrDefault((ProcessModule mdl) => mdl.ModuleName == moduleName);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002CB4 File Offset: 0x00000EB4
		public static Memory GetMemory(this Process process, string moduleName, MemoryPermissions permissions = MemoryPermissions.All)
		{
			return new Memory(process, moduleName, permissions);
		}
	}
}
