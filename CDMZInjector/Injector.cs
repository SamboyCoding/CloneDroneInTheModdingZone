using System;
using System.IO;
using System.Linq;
using CDMZ;
using Mono.Cecil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace CDMZInjector
{
    public static class Injector
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Must specify a path");
                Environment.Exit(1);
                return;
            }

            var path = args[0];

            if (!File.Exists(path))
            {
                Console.WriteLine("Path doesn't exist.");
                Environment.Exit(1);
                return;
            }

            var module = ModuleDefinition.ReadModule(path, new ReaderParameters
            {
                ReadWrite = true,
            });
            Console.WriteLine("Module is valid. Trying to locate SplashScreenFlowManager.Awake");

            var targetType = module.Types.FirstOrDefault(type => type.Name == "SplashScreenFlowManager");
            if (targetType == null)
            {
                Console.WriteLine("Couldn't find SplashScreenFlowManager");
                Environment.Exit(2);
                return;
            }

            Console.WriteLine("Found SplashScreenFlowManager");

            var targetMethod = targetType.Methods.FirstOrDefault(method => method.Name == "Awake");
            if (targetMethod == null)
            {
                Console.WriteLine("Couldn't find SplashScreenFlowManager.Start");
                Environment.Exit(3);
                return;
            }

            Console.WriteLine("Found SplashScreenFlowManager.Awake");

            var methodRef =
                module.ImportReference(typeof(ModdingZoneHooks).GetMethod(nameof(ModdingZoneHooks.InitializeAll)));

            if (targetMethod.Body.Instructions[0].Operand is MethodReference alreadyCalledMethod
                && alreadyCalledMethod.DeclaringType.Name == nameof(ModdingZoneHooks)
                && alreadyCalledMethod.Name == nameof(ModdingZoneHooks.InitializeAll))
            {
                Console.WriteLine("Method already patched!");
                Environment.Exit(4);
                return;
            }

            Console.WriteLine("Injecting a call to " + typeof(ModdingZoneHooks) + "." +
                              nameof(ModdingZoneHooks.InitializeAll));
            var prc = targetMethod.Body.GetILProcessor();
            var call = prc.Create(OpCodes.Call, methodRef);
            var first = targetMethod.Body.Instructions[0];

            prc.InsertBefore(first, call);

            module.Write();

            Console.WriteLine("Done.");
        }
    }
}