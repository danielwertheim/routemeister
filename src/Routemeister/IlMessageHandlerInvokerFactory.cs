using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Routemeister
{
    internal static class IlMessageHandlerInvokerFactory
    {
        internal static MessageHandlerInvoker GetMethodInvoker(MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(
                $"{methodInfo.Name}@{methodInfo.DeclaringType.FullName}",
                typeof(Task),
                new[] { typeof(object), typeof(object) },
                methodInfo.DeclaringType?.Module ?? methodInfo.Module);
            var il = dynamicMethod.GetILGenerator();

            var paramTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            var locals = new LocalBuilder[paramTypes.Length];
            locals[0] = il.DeclareLocal(paramTypes[0]);

            il.Emit(OpCodes.Ldarg_1); //Message
            il.Emit(OpCodes.Castclass, paramTypes[0]); //Cast object to Message-type
            il.Emit(OpCodes.Stloc, locals[0]); //Load message into variable

            il.Emit(OpCodes.Ldarg_0); //Load Message handler container (the instace of the class holding the method)
            il.Emit(OpCodes.Ldloc, locals[0]); //Loads variable with Message

            il.EmitCall(OpCodes.Call, methodInfo, null);
            il.Emit(OpCodes.Ret);

            return (MessageHandlerInvoker)dynamicMethod.CreateDelegate(typeof(MessageHandlerInvoker));
        }
    }
}