using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Routemeister
{
    internal static class ActionConverter
    {
        private static readonly MethodInfo Method;

        static ActionConverter()
        {
            Method = typeof(ActionConverter).GetMethod(
                "Convert",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod);
        }

        internal static MethodInfo MakeGenericMethodFor(Type type)
        {
            return Method.MakeGenericMethod(type);
        }

        // ReSharper disable once UnusedMember.Local
        private static Func<object, Task> Convert<T>(Func<T, Task> action)
        {
            if (action == null) return null;

            return i => action((T)i);
        }
    }
}