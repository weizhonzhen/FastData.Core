using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using FastUntility.Core.Cache;
using System.Collections.Concurrent;

namespace FastUntility.Core.Base
{
    public static class BaseEmit
    {
        private static ConcurrentDictionary<string, MethodInfo> cache = new ConcurrentDictionary<string, MethodInfo>();

        private static MethodInfo Get(string key)
        {
            if (cache.ContainsKey(key))
                return cache[key];
            else
            {
                return null;
            }
        }

        private static void Set(string key, MethodInfo method)
        {
            if (!cache.ContainsKey(key))
            {
                cache.TryAdd(key, method);
            }
            else
            {
                cache.TryRemove(key, out _);
                cache.TryAdd(key, method);
            }
        }

        public static void Set<T>(T model, string name, object value)
        {
            try
            {
                var type = typeof(T);
                var key = $"set_{name}_{type.FullName}";
                MethodInfo method = Get(key);
                if (method == null)
                {
                    method = type.GetMethod($"set_{name}");
                    if (method == null)
                        return;
                    Set(key, method);
                }

                var dynamicMethod = new DynamicMethod("SetEmit", null, new[] { type, typeof(object) }, type.Module);
                var iL = dynamicMethod.GetILGenerator();

                var parameter = method.GetParameters()[0];
                if (parameter == null)
                    return;

                Type defType = parameter.ParameterType;
                if (parameter.ParameterType.Name == "Nullable`1" && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    defType = Nullable.GetUnderlyingType(parameter.ParameterType);

                var local = iL.DeclareLocal(defType, true);

                iL.Emit(OpCodes.Ldarg_1);

                if (defType.IsValueType)
                    iL.Emit(OpCodes.Unbox_Any, defType);
                else
                    iL.Emit(OpCodes.Castclass, defType);

                iL.Emit(OpCodes.Stloc, local);
                iL.Emit(OpCodes.Ldarg_0);
                iL.Emit(OpCodes.Ldloc, local);
                iL.EmitCall(OpCodes.Callvirt, method, null);
                iL.Emit(OpCodes.Ret);

                var dyn = dynamicMethod.CreateDelegate(typeof(Action<T, object>)) as Action<T, object>;
                dyn(model, Convert.ChangeType(value, defType));
            }
            catch (Exception ex) { }
        }

        public static void Set(object model, string name, object value)
        {
            try
            {
                var type = model.GetType();
                var key = $"set_{name}_{type.FullName}";
                MethodInfo method = Get(key);
                if (method == null)
                {
                    method = type.GetMethod($"set_{name}");
                    if (method == null)
                        return;
                    Set(key, method);
                }

                var parameter = method.GetParameters()[0];
                if (parameter == null)
                    return;

                var defType = parameter.ParameterType;
                if (parameter.ParameterType.Name == "Nullable`1" && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    defType = Nullable.GetUnderlyingType(parameter.ParameterType);

                value = Convert.ChangeType(value, defType);

                Invoke(model, method, new object[] { value });
            }
            catch (Exception ex) { }
        }

        public static object Get<T>(T model, string name)
        {
            if (model == null || name.ToStr() == "")
                return null;

            var type = typeof(T);
            var dynamicMethod = new DynamicMethod("GetEmit", typeof(object), new[] { typeof(object) }, type, true);

            var key = $"get_{name}_{type.FullName}";
            MethodInfo method = Get(key);

            if (method == null)
            {
                method = type.GetMethod($"get_{name}");
                if (method == null)
                    return null;
                Set(key, method);
            }

            var property = type.GetProperty(name);

            var iL = dynamicMethod.GetILGenerator();
            iL.Emit(OpCodes.Ldarg_0);

            if (type.IsValueType)
                iL.Emit(OpCodes.Unbox, type);
            else
                iL.Emit(OpCodes.Castclass, type);

            iL.EmitCall(OpCodes.Callvirt, method, null);

            if (property.PropertyType.IsValueType)
                iL.Emit(OpCodes.Box, property.PropertyType);

            iL.Emit(OpCodes.Ret);
            var dyn = (Func<T, object>)dynamicMethod.CreateDelegate(typeof(Func<T, object>));
            return dyn(model);
        }

        public static object Get(object model, string name)
        {
            if (model == null || name.ToStr() == "")
                return null;

            var type = model.GetType();
            var dynamicMethod = new DynamicMethod("GetEmit", typeof(object), new[] { typeof(object) }, type, true);
            var key = $"get_{name}_{type.FullName}";
            MethodInfo method = Get(key);

            if (method == null)
            {
                method = type.GetMethod($"get_{name}");
                if (method == null)
                    return null;
                Set(key, method);
            }

            var property = type.GetProperty(name);

            var iL = dynamicMethod.GetILGenerator();
            iL.Emit(OpCodes.Ldarg_0);

            if (type.IsValueType)
                iL.Emit(OpCodes.Unbox, type);
            else
                iL.Emit(OpCodes.Castclass, type);

            iL.EmitCall(OpCodes.Callvirt, method, null);

            if (property.PropertyType.IsValueType)
                iL.Emit(OpCodes.Box, property.PropertyType);

            iL.Emit(OpCodes.Ret);
            var dyn = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            return dyn(model);
        }

        private delegate object EmitInvoke(object target, object[] paramters);

        public static object Invoke(object model, MethodInfo methodInfo, object[] param)
        {
            if (methodInfo == null || model == null)
                return null;
            try
            {
                var dynamicMethod = new DynamicMethod("InvokeEmit", typeof(object), new Type[] { typeof(object), typeof(object[]) }, typeof(EmitInvoke).Module);
                var iL = dynamicMethod.GetILGenerator();
                var info = methodInfo.GetParameters();
                var type = new Type[info.Length];
                var local = new LocalBuilder[type.Length];

                for (int i = 0; i < info.Length; i++)
                {
                    if (info[i].ParameterType.IsByRef)
                        type[i] = info[i].ParameterType.GetElementType();
                    else
                        type[i] = info[i].ParameterType;
                }

                for (int i = 0; i < type.Length; i++)
                {
                    local[i] = iL.DeclareLocal(type[i], true);
                }

                for (int i = 0; i < type.Length; i++)
                {
                    iL.Emit(OpCodes.Ldarg_1);
                    iL.Emit(OpCodes.Ldc_I4, i);
                    iL.Emit(OpCodes.Ldelem_Ref);
                    if (type[i].IsValueType)
                        iL.Emit(OpCodes.Unbox_Any, type[i]);
                    else
                        iL.Emit(OpCodes.Castclass, type[i]);
                    iL.Emit(OpCodes.Stloc, local[i]);
                }

                if (!methodInfo.IsStatic)
                    iL.Emit(OpCodes.Ldarg_0);

                for (int i = 0; i < type.Length; i++)
                {
                    if (info[i].ParameterType.IsByRef)
                        iL.Emit(OpCodes.Ldloca_S, local[i]);
                    else
                        iL.Emit(OpCodes.Ldloc, local[i]);
                }

                if (methodInfo.IsStatic)
                    iL.EmitCall(OpCodes.Call, methodInfo, null);
                else
                    iL.EmitCall(OpCodes.Callvirt, methodInfo, null);

                if (methodInfo.ReturnType == typeof(void))
                    iL.Emit(OpCodes.Ldnull);
                else if (methodInfo.ReturnType.IsValueType)
                    iL.Emit(OpCodes.Box, methodInfo.ReturnType);


                for (int i = 0; i < type.Length; i++)
                {
                    if (info[i].ParameterType.IsByRef)
                    {
                        iL.Emit(OpCodes.Ldarg_1);
                        iL.Emit(OpCodes.Ldc_I4, i);
                        iL.Emit(OpCodes.Ldloc, local[i]);
                        if (local[i].LocalType.IsValueType)
                            iL.Emit(OpCodes.Box, local[i].LocalType);
                        iL.Emit(OpCodes.Stelem_Ref);
                    }
                }

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(EmitInvoke)) as EmitInvoke;
                return dyn(model, param);
            }
            catch (Exception ex)
            {
                return methodInfo.Invoke(model, param);
            }
        }
    }
}