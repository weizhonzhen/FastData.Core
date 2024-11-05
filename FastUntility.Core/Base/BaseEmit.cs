using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace FastUntility.Core.Base
{
    public static class BaseEmit
    {
        private static ConcurrentDictionary<string, MethodInfo> cache = new ConcurrentDictionary<string, MethodInfo>();
        private static ConcurrentDictionary<string, DynamicMethod> cacheDyn = new ConcurrentDictionary<string, DynamicMethod>();

        private static MethodInfo GetMethod(string key)
        {
            if (cache.ContainsKey(key))
                return cache[key];
            else
            {
                return null;
            }
        }

        private static void SetMethod(string key, MethodInfo method)
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

        private static void SetDyn(string key, DynamicMethod dyn)
        {
            if (!cacheDyn.ContainsKey(key))
            {
                cacheDyn.TryAdd(key, dyn);
            }
            else
            {
                cacheDyn.TryRemove(key, out _);
                cacheDyn.TryAdd(key, dyn);
            }
        }

        private static DynamicMethod GetDyn(string key)
        {
            if (cacheDyn.ContainsKey(key))
                return cacheDyn[key];
            else
            {
                return null;
            }
        }

        public static void Set<T>(T model, string name, object value)
        {
            try
            {
                var type = typeof(T);
                var key = $"set_{name}_{type.FullName}";
                MethodInfo method = GetMethod(key);
                if (method == null)
                {
                    method = type.GetMethod($"set_{name}");
                    if (method == null)
                        return;
                    SetMethod(key, method);
                }

                var dynKey = $"SetEmit_{type}_T_{type.Module}";
                var dynamicMethod = GetDyn(dynKey);
                if (dynamicMethod == null)
                {
                    dynamicMethod = new DynamicMethod("SetEmit", null, new[] { type, typeof(object) }, type.Module);
                    SetDyn(dynKey, dynamicMethod);
                }

                var iL = dynamicMethod.GetILGenerator();

                var parameter = method.GetParameters()[0];
                if (parameter == null)
                    return;

                Type defType = parameter.ParameterType;
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
                if (parameter.ParameterType.Name == "Nullable`1" && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    dyn(model, value);
                else
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
                MethodInfo method = GetMethod(key);
                if (method == null)
                {
                    method = type.GetMethod($"set_{name}");
                    if (method == null)
                        return;
                    SetMethod(key, method);
                }

                var parameter = method.GetParameters()[0];
                if (parameter == null)
                    return;

                Invoke(model, method, new object[] { value });
            }
            catch (Exception ex) { }
        }

        public static void Set<T>(T model, Dictionary<string, object> dic)
        {
            try
            {
                var type = typeof(T);
                var dynamicMethod = new DynamicMethod("SetEmit", null, new[] { type, typeof(object) }, type.Module);

                var iL = dynamicMethod.GetILGenerator();

                foreach (var item in dic)
                {
                    var key = $"set_{item.Key}_{type.FullName}";
                    MethodInfo method = GetMethod(key);
                    if (method == null)
                    {
                        method = type.GetMethod($"set_{item.Key}");
                        if (method == null)
                            return;
                        SetMethod(key, method);
                    }
                    var parameter = method.GetParameters()[0];
                    if (parameter == null)
                        return;

                    Type defType = parameter.ParameterType;
                    var local = iL.DeclareLocal(defType, true);

                    ExecIlType(iL, local, defType, item, method);
                }

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(Action<T, object>)) as Action<T, object>;
                dyn(model, dic);
            }
            catch (Exception ex) { }
        }

        public static void Set(object model, Dictionary<string, object> dic)
        {
            try
            {
                var type = model.GetType();
                var dynamicMethod = new DynamicMethod("SetEmit", null, new[] { type, typeof(object) }, type.Module);

                var iL = dynamicMethod.GetILGenerator();

                foreach (var item in dic)
                {
                    var key = $"set_{item.Key}_{type.FullName}";
                    MethodInfo method = GetMethod(key);
                    if (method == null)
                    {
                        method = type.GetMethod($"set_{item.Key}");
                        if (method == null)
                            return;
                        SetMethod(key, method);
                    }
                    var parameter = method.GetParameters()[0];
                    if (parameter == null)
                        return;

                    Type defType = parameter.ParameterType;
                    var local = iL.DeclareLocal(defType, true);

                    ExecIlType(iL, local, defType, item, method);
                }

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>;
                dyn(model, dic);
            }
            catch (Exception ex) { }
        }

        public static void Set<T>(List<T> list, List<Dictionary<string, object>> dics)
        {
            try
            {
                var listType = typeof(List<T>);
                var type = typeof(T);

                var dynKey = $"SetEmit_{listType}_{listType.Module}";
                var dynamicMethod = GetDyn(dynKey);
                if (dynamicMethod == null)
                {
                    dynamicMethod = new DynamicMethod("SetEmit", null, new[] { listType, typeof(List<Dictionary<string, object>>) }, type.Module);
                    SetDyn(dynKey, dynamicMethod);
                }
                var iL = dynamicMethod.GetILGenerator();

                var addKey = $"Add_{listType.FullName}";
                MethodInfo addMethod = GetMethod(addKey);
                if (addMethod == null)
                {
                    addMethod = listType.GetMethod("Add");
                    if (addMethod == null)
                        return;
                    SetMethod(addKey, addMethod);
                }

                dics.ForEach(dic =>
                {
                    var model = iL.DeclareLocal(typeof(T));
                    iL.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                    iL.Emit(OpCodes.Stloc, model);

                    foreach (var item in dic)
                    {
                        var key = $"set_{item.Key}_{type.FullName}";
                        MethodInfo method = GetMethod(key);
                        if (method == null)
                        {
                            method = type.GetMethod($"set_{item.Key}");
                            if (method == null)
                                return;
                            SetMethod(key, method);
                        }
                        var parameter = method.GetParameters()[0];
                        if (parameter == null)
                            return;

                        var defType = parameter.ParameterType;

                        if (defType == typeof(bool) || defType == typeof(bool?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(bool?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                if (item.Value.ToStr().ToInt(9) == 0)
                                    iL.Emit(OpCodes.Ldc_I4_0);
                                else
                                    iL.Emit(OpCodes.Ldc_I4_1);

                                if (defType == typeof(bool))
                                    iL.Emit(OpCodes.Box, typeof(bool));
                                else
                                    iL.Emit(OpCodes.Newobj, typeof(bool?).GetConstructor(new Type[] { typeof(bool) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(string) || defType == typeof(String))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            iL.Emit(OpCodes.Ldstr, item.Value.ToStr());
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if ((defType == typeof(decimal)) || defType == typeof(decimal?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null)
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_R8, item.Value.ToStr().ToDouble(0));
                                iL.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor(new Type[] { typeof(double) }));

                                if (defType == typeof(decimal?))
                                    iL.Emit(OpCodes.Newobj, typeof(decimal?).GetConstructor(new[] { typeof(decimal) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(DateTime) || defType == typeof(DateTime?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(DateTime?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I8, item.Value.ToDate().Value.Ticks);
                                iL.Emit(OpCodes.Newobj, typeof(DateTime).GetConstructor(new Type[] { typeof(long) }));
                                if (defType == typeof(DateTime?))
                                    iL.Emit(OpCodes.Newobj, typeof(DateTime?).GetConstructor(new Type[] { typeof(DateTime) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(int) || defType == typeof(byte) || defType == typeof(int?) || defType == typeof(byte?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && (defType == typeof(byte?) || defType == typeof(int?)))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I4, item.Value.ToStr().ToInt(0));
                                if (defType == typeof(int?))
                                    iL.Emit(OpCodes.Newobj, typeof(int?).GetConstructor(new Type[] { typeof(int) }));

                                if (defType == typeof(byte?))
                                    iL.Emit(OpCodes.Newobj, typeof(byte?).GetConstructor(new Type[] { typeof(byte) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(long) || defType == typeof(long?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(long?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I8, item.Value.ToStr().ToLong(0));
                                if (defType == typeof(long?))
                                    iL.Emit(OpCodes.Newobj, typeof(long?).GetConstructor(new Type[] { typeof(long) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(double) || defType == typeof(double?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(double?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_R8, item.Value.ToStr().ToDouble(0));

                                if (defType == typeof(double?))
                                    iL.Emit(OpCodes.Newobj, typeof(double?).GetConstructor(new Type[] { typeof(double) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(sbyte) || defType == typeof(short))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            iL.Emit(OpCodes.Ldc_I4_S, item.Value.ToStr().ToInt16(0));
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(sbyte?) || defType == typeof(short?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(short?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I4_S, 1);
                                if (defType == typeof(sbyte?))
                                    iL.Emit(OpCodes.Newobj, typeof(sbyte?).GetConstructor(new Type[] { typeof(sbyte) }));
                                if (defType == typeof(short?))
                                    iL.Emit(OpCodes.Newobj, typeof(short?).GetConstructor(new Type[] { typeof(short) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(float) || defType == typeof(float?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(float?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_R4, item.Value.ToStr().ToFloat(0));
                                if (defType == typeof(float?))
                                    iL.Emit(OpCodes.Newobj, typeof(float?).GetConstructor(new Type[] { typeof(float) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(TimeSpan) || defType == typeof(TimeSpan?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(TimeSpan?))
                                iL.Emit(OpCodes.Ldnull);
                            {
                                iL.Emit(OpCodes.Ldc_I8, item.Value.ToDate().Value.Ticks);
                                iL.Emit(OpCodes.Newobj, typeof(TimeSpan).GetConstructor(new Type[] { typeof(long) }));

                                if (defType == typeof(TimeSpan?))
                                    iL.Emit(OpCodes.Newobj, typeof(TimeSpan?).GetConstructor(new Type[] { typeof(TimeSpan) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                    }

                    iL.Emit(OpCodes.Ldarg_0);
                    iL.Emit(OpCodes.Ldloc, model);
                    iL.EmitCall(OpCodes.Callvirt, addMethod, null);
                });

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(Action<List<T>, List<Dictionary<string, object>>>)) as Action<List<T>, List<Dictionary<string, object>>>;
                dyn(list, dics);
            }
            catch (Exception ex) { }
        }

        public static void Set(Type type, object list, List<Dictionary<string, object>> dics)
        {
            try
            {
                var listType = list.GetType();

                var dynKey = $"SetEmit_{listType}_{listType.Module}";
                var dynamicMethod = GetDyn(dynKey);
                if (dynamicMethod == null)
                {
                    dynamicMethod = new DynamicMethod("SetEmit", null, new[] { typeof(object), typeof(List<Dictionary<string, object>>) }, type.Module);
                    SetDyn(dynKey, dynamicMethod);
                }
                var iL = dynamicMethod.GetILGenerator();

                var addKey = $"Add_{listType.FullName}";
                MethodInfo addMethod = GetMethod(addKey);
                if (addMethod == null)
                {
                    addMethod = listType.GetMethod("Add");
                    if (addMethod == null)
                        return;
                    SetMethod(addKey, addMethod);
                }

                dics.ForEach(dic =>
                {
                    var model = iL.DeclareLocal(type);
                    iL.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                    iL.Emit(OpCodes.Stloc, model);

                    foreach (var item in dic)
                    {
                        var key = $"set_{item.Key}_{type.FullName}";
                        MethodInfo method = GetMethod(key);
                        if (method == null)
                        {
                            method = type.GetMethod($"set_{item.Key}");
                            if (method == null)
                                return;
                            SetMethod(key, method);
                        }
                        var parameter = method.GetParameters()[0];
                        if (parameter == null)
                            return;

                        var defType = parameter.ParameterType;

                        if (defType == typeof(bool) || defType == typeof(bool?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(bool?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                if (item.Value.ToStr().ToInt(9) == 0)
                                    iL.Emit(OpCodes.Ldc_I4_0);
                                else
                                    iL.Emit(OpCodes.Ldc_I4_1);

                                if (defType == typeof(bool))
                                    iL.Emit(OpCodes.Box, typeof(bool));
                                else
                                    iL.Emit(OpCodes.Newobj, typeof(bool?).GetConstructor(new Type[] { typeof(bool) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(string) || defType == typeof(String))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            iL.Emit(OpCodes.Ldstr, item.Value.ToStr());
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if ((defType == typeof(decimal)) || defType == typeof(decimal?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null)
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_R8, item.Value.ToStr().ToDouble(0));
                                iL.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor(new Type[] { typeof(double) }));

                                if (defType == typeof(decimal?))
                                    iL.Emit(OpCodes.Newobj, typeof(decimal?).GetConstructor(new[] { typeof(decimal) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(DateTime) || defType == typeof(DateTime?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(DateTime?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I8, item.Value.ToDate().Value.Ticks);
                                iL.Emit(OpCodes.Newobj, typeof(DateTime).GetConstructor(new Type[] { typeof(long) }));
                                if (defType == typeof(DateTime?))
                                    iL.Emit(OpCodes.Newobj, typeof(DateTime?).GetConstructor(new Type[] { typeof(DateTime) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(int) || defType == typeof(byte) || defType == typeof(int?) || defType == typeof(byte?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && (defType == typeof(byte?) || defType == typeof(int?)))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I4, item.Value.ToStr().ToInt(0));
                                if (defType == typeof(int?))
                                    iL.Emit(OpCodes.Newobj, typeof(int?).GetConstructor(new Type[] { typeof(int) }));

                                if (defType == typeof(byte?))
                                    iL.Emit(OpCodes.Newobj, typeof(byte?).GetConstructor(new Type[] { typeof(byte) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(long) || defType == typeof(long?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(long?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I8, item.Value.ToStr().ToLong(0));
                                if (defType == typeof(long?))
                                    iL.Emit(OpCodes.Newobj, typeof(long?).GetConstructor(new Type[] { typeof(long) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(double) || defType == typeof(double?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(double?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_R8, item.Value.ToStr().ToDouble(0));

                                if (defType == typeof(double?))
                                    iL.Emit(OpCodes.Newobj, typeof(double?).GetConstructor(new Type[] { typeof(double) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(sbyte) || defType == typeof(short))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            iL.Emit(OpCodes.Ldc_I4_S, item.Value.ToStr().ToInt16(0));
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(sbyte?) || defType == typeof(short?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(short?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_I4_S, 1);
                                if (defType == typeof(sbyte?))
                                    iL.Emit(OpCodes.Newobj, typeof(sbyte?).GetConstructor(new Type[] { typeof(sbyte) }));
                                if (defType == typeof(short?))
                                    iL.Emit(OpCodes.Newobj, typeof(short?).GetConstructor(new Type[] { typeof(short) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(float) || defType == typeof(float?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(float?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                iL.Emit(OpCodes.Ldc_R4, item.Value.ToStr().ToFloat(0));
                                if (defType == typeof(float?))
                                    iL.Emit(OpCodes.Newobj, typeof(float?).GetConstructor(new Type[] { typeof(float) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                        if (defType == typeof(TimeSpan) || defType == typeof(TimeSpan?))
                        {
                            iL.Emit(OpCodes.Ldloc, model);
                            if (item.Value == null && defType == typeof(TimeSpan?))
                                iL.Emit(OpCodes.Ldnull);
                            {
                                iL.Emit(OpCodes.Ldc_I8, item.Value.ToDate().Value.Ticks);
                                iL.Emit(OpCodes.Newobj, typeof(TimeSpan).GetConstructor(new Type[] { typeof(long) }));

                                if (defType == typeof(TimeSpan?))
                                    iL.Emit(OpCodes.Newobj, typeof(TimeSpan?).GetConstructor(new Type[] { typeof(TimeSpan) }));
                            }
                            iL.EmitCall(OpCodes.Callvirt, method, null);
                        }
                    }

                    iL.Emit(OpCodes.Ldarg_0);
                    iL.Emit(OpCodes.Ldloc, model);
                    iL.EmitCall(OpCodes.Callvirt, addMethod, null);
                });

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(Action<object, List<Dictionary<string, object>>>)) as Action<object, List<Dictionary<string, object>>>;
                dyn(list, dics);
            }
            catch (Exception ex) { }
        }

        private static void ExecIL(ILGenerator iL, LocalBuilder local, MethodInfo method)
        {
            iL.Emit(OpCodes.Stloc, local);
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldloc, local);
            iL.EmitCall(OpCodes.Callvirt, method, null);
        }

        private static void ExecIlType(ILGenerator iL, LocalBuilder local, Type defType, KeyValuePair<string, object> item, MethodInfo method)
        {
            if (defType == typeof(bool) || defType == typeof(bool?))
            {
                if (item.Value == null && defType == typeof(bool?))
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    if (item.Value.ToStr().ToInt(9) == 0)
                        iL.Emit(OpCodes.Ldc_I4_0);
                    else
                        iL.Emit(OpCodes.Ldc_I4_1);

                    if (defType == typeof(bool))
                        iL.Emit(OpCodes.Box, typeof(bool));
                    else
                        iL.Emit(OpCodes.Newobj, typeof(bool?).GetConstructor(new Type[] { typeof(bool) }));
                }
                ExecIL(iL, local, method);
            }

            if ((defType == typeof(decimal)) || defType == typeof(decimal?))
            {
                if (item.Value == null)
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    iL.Emit(OpCodes.Ldc_R8, item.Value.ToStr().ToDouble(0));
                    iL.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor(new Type[] { typeof(double) }));

                    if (defType == typeof(decimal?))
                        iL.Emit(OpCodes.Newobj, typeof(decimal?).GetConstructor(new[] { typeof(decimal) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(DateTime) || defType == typeof(DateTime?))
            {
                if (item.Value == null && defType == typeof(DateTime?))
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    iL.Emit(OpCodes.Ldc_I8, item.Value.ToDate().Value.Ticks);
                    iL.Emit(OpCodes.Newobj, typeof(DateTime).GetConstructor(new Type[] { typeof(long) }));
                    if (defType == typeof(DateTime?))
                        iL.Emit(OpCodes.Newobj, typeof(DateTime?).GetConstructor(new Type[] { typeof(DateTime) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(int) || defType == typeof(byte) || defType == typeof(int?) || defType == typeof(byte?))
            {
                if (item.Value == null && (defType == typeof(byte?) || defType == typeof(int?)))
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    iL.Emit(OpCodes.Ldc_I4, item.Value.ToStr().ToInt(0));
                    if (defType == typeof(int?))
                        iL.Emit(OpCodes.Newobj, typeof(int?).GetConstructor(new Type[] { typeof(int) }));

                    if (defType == typeof(byte?))
                        iL.Emit(OpCodes.Newobj, typeof(byte?).GetConstructor(new Type[] { typeof(byte) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(long) || defType == typeof(long?))
            {
                if (item.Value == null && defType == typeof(long?))
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    iL.Emit(OpCodes.Ldc_I8, item.Value.ToStr().ToLong(0));
                    if (defType == typeof(long?))
                        iL.Emit(OpCodes.Newobj, typeof(long?).GetConstructor(new Type[] { typeof(long) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(double) || defType == typeof(double?))
            {
                if (item.Value == null && defType == typeof(double?))
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    iL.Emit(OpCodes.Ldc_R8, item.Value.ToStr().ToDouble(0));

                    if (defType == typeof(double?))
                        iL.Emit(OpCodes.Newobj, typeof(double?).GetConstructor(new Type[] { typeof(double) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(sbyte) || defType == typeof(short))
            {
                iL.Emit(OpCodes.Ldc_I4_S, item.Value.ToStr().ToInt16(0));
                ExecIL(iL, local, method);
            }

            if (defType == typeof(sbyte?) || defType == typeof(short?))
            {
                if (item.Value == null && defType == typeof(short?))
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    iL.Emit(OpCodes.Ldc_I4_S, 1);
                    if (defType == typeof(sbyte?))
                        iL.Emit(OpCodes.Newobj, typeof(sbyte?).GetConstructor(new Type[] { typeof(sbyte) }));
                    if (defType == typeof(short?))
                        iL.Emit(OpCodes.Newobj, typeof(short?).GetConstructor(new Type[] { typeof(short) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(float) || defType == typeof(float?))
            {
                if (item.Value == null && defType == typeof(float?))
                    iL.Emit(OpCodes.Ldnull);
                else
                {
                    iL.Emit(OpCodes.Ldc_R4, item.Value.ToStr().ToFloat(0));
                    if (defType == typeof(float?))
                        iL.Emit(OpCodes.Newobj, typeof(float?).GetConstructor(new Type[] { typeof(float) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(TimeSpan) || defType == typeof(TimeSpan?))
            {
                if (item.Value == null && defType == typeof(TimeSpan?))
                    iL.Emit(OpCodes.Ldnull);
                {
                    iL.Emit(OpCodes.Ldc_I8, item.Value.ToDate().Value.Ticks);
                    iL.Emit(OpCodes.Newobj, typeof(TimeSpan).GetConstructor(new Type[] { typeof(long) }));

                    if (defType == typeof(TimeSpan?))
                        iL.Emit(OpCodes.Newobj, typeof(TimeSpan?).GetConstructor(new Type[] { typeof(TimeSpan) }));
                }
                ExecIL(iL, local, method);
            }

            if (defType == typeof(string) || defType == typeof(String))
            {
                iL.Emit(OpCodes.Ldstr, item.Value.ToStr());
                ExecIL(iL, local, method);
            }
        }

        public static object Get<T>(T model, string name)
        {
            if (model == null || name.ToStr() == "")
                return null;

            var type = typeof(T);
            var dynKey = $"GetEmit_T_{type}_{type.Module}";
            var dynamicMethod = GetDyn(dynKey);
            if (dynamicMethod == null)
            {
                dynamicMethod = new DynamicMethod("GetEmit", typeof(object), new[] { typeof(object) }, type, true);
                SetDyn(dynKey, dynamicMethod);
            }

            var key = $"get_{name}_{type.FullName}";
            MethodInfo method = GetMethod(key);

            if (method == null)
            {
                method = type.GetMethod($"get_{name}");
                if (method == null)
                    return null;
                SetMethod(key, method);
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

            var dynKey = $"GetEmit_model_{type}_{type.Module}";
            var dynamicMethod = GetDyn(dynKey);
            if (dynamicMethod == null)
            {
                dynamicMethod = new DynamicMethod("GetEmit", typeof(object), new[] { typeof(object) }, type, true);
                SetDyn(dynKey, dynamicMethod);
            }

            var key = $"get_{name}_{type.FullName}";
            MethodInfo method = GetMethod(key);

            if (method == null)
            {
                method = type.GetMethod($"get_{name}");
                if (method == null)
                    return null;
                SetMethod(key, method);
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
                var dynKey = $"InvokeEmit_{typeof(EmitInvoke).Module}";
                var dynamicMethod = GetDyn(dynKey);
                if (dynamicMethod == null)
                {
                    dynamicMethod = new DynamicMethod("InvokeEmit", typeof(object), new Type[] { typeof(object), typeof(object[]) }, typeof(EmitInvoke).Module);
                    SetDyn(dynKey, dynamicMethod);
                }
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