using System;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.OotDisassembler
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

        public static bool PerformChecklistOperation(string title, Action operation)
        {
            return PerformChecklistOperation(title, () =>
            {
                try
                {
                    operation();

                    return new OperationResult();
                }
                catch (Exception e)
                {
                    return new OperationResult(e);
                }
            });
        }

        public static bool PerformChecklistOperation<T>(string title, Func<T> operation, Predicate<T> validate)
        {
            return PerformChecklistOperation(title, () =>
            {
                try
                {
                    if (!validate(operation()))
                    {
                        throw new Exception("Validation of result failed.");
                    }

                    return new OperationResult();
                }
                catch (Exception e)
                {
                    return new OperationResult(e);
                }
            });
        }

        public static bool PerformChecklistOperation<T>(string title, Func<T> operation, Func<T, Exception?> validate)
        {
            return PerformChecklistOperation(title, () =>
            {
                try
                {
                    var result = operation();
                    var error = validate(result);
                    if (error is not null)
                    {
                        return new OperationResult(error);
                    }

                    return new OperationResult();
                }
                catch (Exception e)
                {
                    return new OperationResult(e);
                }
            });
        }

        public static bool PerformChecklistOperation(string title, Func<OperationResult> operation)
        {
            Console.Write($"[ ] {title}");
            var operationResult = operation();
            EraseLine();

            var foreground = Console.ForegroundColor;

            try
            {
                if (operationResult.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[o] {title}");
                    return true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[x] {title}");
                if (operationResult.Exception is not null)
                {
                    Console.WriteLine(operationResult.Exception);
                }
                else if (!string.IsNullOrWhiteSpace(operationResult.ExceptionMessage))
                {
                    Console.WriteLine(operationResult.ExceptionMessage);
                }

                return false;
            }
            finally
            {
                Console.ForegroundColor = foreground;
            }
        }

        public static Task<bool> PerformChecklistOperationAsync(string title, Func<ValueTask> operation)
        {
            return PerformChecklistOperationAsync(title, async () =>
            {
                try
                {
                    await operation();

                    return new OperationResult();
                }
                catch (Exception e)
                {
                    return new OperationResult(e);
                }
            });
        }

        public static Task<bool> PerformChecklistOperationAsync<T>(string title, Func<ValueTask<T>> operation, Predicate<T> validate)
        {
            return PerformChecklistOperationAsync(title, async () =>
            {
                try
                {
                    if (!validate(await operation()))
                    {
                        throw new Exception("Validation of result failed.");
                    }

                    return new OperationResult();
                }
                catch (Exception e)
                {
                    return new OperationResult(e);
                }
            });
        }

        public static Task<bool> PerformChecklistOperationAsync<T>(string title, Func<ValueTask<T>> operation, Func<T, Exception?> validate)
        {
            return PerformChecklistOperationAsync(title, async () =>
            {
                try
                {
                    var result = await operation();
                    var error = validate(result);
                    if (error is not null)
                    {
                        return new OperationResult(error);
                    }

                    return new OperationResult();
                }
                catch (Exception e)
                {
                    return new OperationResult(e);
                }
            });
        }

        public static async Task<bool> PerformChecklistOperationAsync(string title, Func<ValueTask<OperationResult>> operation)
        {
            Console.Write($"[ ] {title}");
            var operationResult = await operation();
            EraseLine();

            var foreground = Console.ForegroundColor;

            try
            {
                if (operationResult.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[o] {title}");
                    return true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[x] {title}");
                if (operationResult.Exception is not null)
                {
                    Console.WriteLine(operationResult.Exception);
                }
                else if (!string.IsNullOrWhiteSpace(operationResult.ExceptionMessage))
                {
                    Console.WriteLine(operationResult.ExceptionMessage);
                }

                return false;
            }
            finally
            {
                Console.ForegroundColor = foreground;
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
