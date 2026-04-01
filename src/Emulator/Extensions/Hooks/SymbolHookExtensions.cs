using System;

using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Peripherals.CPU;

namespace Antmicro.Renode.Hooks
{
    public static class SymbolHookExtensions
    {
        public static void AddHookAtSymbol(this IBusController sysbus, string symbol, CpuAddressHook hook)
        {
            sysbus.ApplyAtSymbol(symbol, (cpu, addr) => cpu.AddHook(addr, hook));
        }

        public static void RemoveHookAtSymbol(this IBusController sysbus, string symbol, CpuAddressHook hook)
        {
            sysbus.ApplyAtSymbol(symbol, (cpu, addr) => cpu.RemoveHook(addr, hook, true));
        }

        public static void ApplyAtSymbol(this IBusController sysbus, string symbol, Action<ICPUWithHooks, ulong> action)
        {
            if(sysbus.TryGetAllSymbolAddresses(symbol, out var symbolAddresses))
            {
                foreach(var cpu in sysbus.GetCPUs())
                {
                    if(cpu is ICPUWithHooks)
                    {
                        foreach(var address in symbolAddresses)
                        {
                            action((ICPUWithHooks)cpu, address);
                        }
                    }
                }
            } else
            {
                Logger.Log(LogLevel.Warning, $"Symbol '{symbol}' not found in any of the CPUs. No hooks will be modified.");
            }
        }
    }
}