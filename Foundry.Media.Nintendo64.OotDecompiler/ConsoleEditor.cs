using System;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal enum EInputBlockResult
    {
        Success,
        Failed,
        Back
    }

    internal static class ConsoleEditor
    {
        public static Func<ValueTask<EInputBlockResult>> BackTask { get; } = new Func<ValueTask<EInputBlockResult>>(() => new ValueTask<EInputBlockResult>(EInputBlockResult.Back));
        public static Func<ValueTask<EInputBlockResult>> SuccessTask { get; } = new Func<ValueTask<EInputBlockResult>>(() => new ValueTask<EInputBlockResult>(EInputBlockResult.Success));
        public static Func<ValueTask<EInputBlockResult>> FailedTask { get; } = new Func<ValueTask<EInputBlockResult>>(() => new ValueTask<EInputBlockResult>(EInputBlockResult.Failed));

        private static bool BackTracker;

        public static async Task<EInputBlockResult> RequestInputBlockAsync(string title, params (string Title, Func<ValueTask<EInputBlockResult>> Callback)[] callbacks)
        {
            int cursorTop = Console.CursorTop;
            while (true)
            {
                Console.WriteLine(title);
                Console.WriteLine(new string('-', Math.Min(Console.BufferWidth, title.Length)));
                for (int i = 0; i < callbacks.Length; ++i)
                {
                    Console.WriteLine($"{i + 1}. {callbacks[i].Title}");
                }

                int callbackIndex = (int)Console.ReadKey().Key - (int)ConsoleKey.D1;
                if (callbackIndex < 0 || callbackIndex >= callbacks.Length)
                {
                    EraseLines(cursorTop + 1);
                    continue;
                }

                EraseLine();
                Console.WriteLine($"[{callbackIndex + 1}]");
                Console.WriteLine();

                var callback = callbacks[callbackIndex];
                var result = await callback.Callback();
                switch (result)
                {
                    case EInputBlockResult.Back:
                        EraseLines(cursorTop + 1);
                        if (!BackTracker)
                        {
                            BackTracker = true;
                            return result;
                        }
                        else
                        {
                            BackTracker = false;
                            continue;
                        }
                    case EInputBlockResult.Success:
                    case EInputBlockResult.Failed:
                        return result;

                }
            }
        }

        public static void EraseLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        public static void EraseLines(int toCursorTop)
        {
            while (Console.CursorTop >= toCursorTop)
            {
                EraseLine();
                --Console.CursorTop;
            }
        }

        public static void ErasePreviousLine()
        {
            EraseLine();
            --Console.CursorTop;
            EraseLine();
        }
    }
}
