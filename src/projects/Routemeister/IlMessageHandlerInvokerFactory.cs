using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Routemeister
{
    internal static class IlMessageHandlerInvokerFactory
    {
        private static readonly Type TaskType = typeof(Task);
        private static readonly Type GenericTaskType = typeof(Task<>);
        private static readonly Type VoidType = typeof(void);

        internal static MessageHandlerInvoker GetMethodInvoker(MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType == VoidType)
                return GetVoidMethodInvoker(methodInfo);

            if (methodInfo.ReturnType == TaskType)
                return GetAsyncMethodInvoker(methodInfo);

            if (methodInfo.ReturnType.GetTypeInfo().IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == GenericTaskType)
                return GetAsyncMethodInvoker(methodInfo);

            return GetObjectMethodInvoker(methodInfo);
        }

        private static MessageHandlerInvoker GetAsyncMethodInvoker(MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(
                $"{methodInfo.Name}@{methodInfo.DeclaringType.FullName}",
                typeof(Task),
                new[] { typeof(object), typeof(object) },
                methodInfo.DeclaringType?.GetTypeInfo().Module ?? methodInfo.Module);
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

        private static MessageHandlerInvoker GetVoidMethodInvoker(MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(
                $"{methodInfo.Name}@{methodInfo.DeclaringType.FullName}",
                typeof(object),
                new[] { typeof(object), typeof(object) },
                methodInfo.DeclaringType?.GetTypeInfo().Module ?? methodInfo.Module);
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

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            return (MessageHandlerInvoker)dynamicMethod.CreateDelegate(typeof(MessageHandlerInvoker));
        }

        private static MessageHandlerInvoker GetObjectMethodInvoker(MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(
                $"{methodInfo.Name}@{methodInfo.DeclaringType.FullName}",
                typeof(object),
                new[] { typeof(object), typeof(object) },
                methodInfo.DeclaringType?.GetTypeInfo().Module ?? methodInfo.Module);
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