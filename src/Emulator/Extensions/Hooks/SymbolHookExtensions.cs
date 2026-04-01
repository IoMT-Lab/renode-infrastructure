using System;

using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Peripherals.CPU;

namespace Antmicro.Renode.Hooks
{
    public static class SymbolHookExtensions
    {
        private static readonly CpuAddressHook pauseHook = (cpu, addr) =>
        {
            cpu.Log(LogLevel.Info, "Pausing execution at symbol hook for address 0x{0:X}", addr);
            cpu.Bus.Machine.PauseAndRequestEmulationPause();
        };

        public static void AddPauseHookAtSymbol(this IBusController sysbus, string symbol)
        {
            sysbus.AddHookAtSymbol(symbol, pauseHook);
        }

        public static void RemovePauseHookAtSymbol(this IBusController sysbus, string symbol)
        {
            sysbus.RemoveHookAtSymbol(symbol, pauseHook);
        }

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