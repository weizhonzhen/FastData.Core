using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace FastUntility.Core.Base
{
    public static class BaseEmit
    {
        public static void Set<T>(T model, string name, object value)
        {
            try
            {
                var type = typeof(T);
                var method = type.GetMethod($"set_{name}");
                if (method == null)
                    return;

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
                var method = type.GetMethod($"set_{name}");
                if (method == null)
                    return;

                var dynamicMethod = new DynamicMethod("SetEmit", null, new[] { type, typeof(object) }, type.Module);
                var iL = dynamicMethod.GetILGenerator();

                var parameter = method.GetParameters()[0];
                if (parameter == null)
                    return;

                Type defType = parameter.ParameterType;
                if (parameter.ParameterType.Name == "Nullable`1" && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    defType = Nullable.GetUnderlyingType(parameter.ParameterType);

                value = Convert.ChangeType(value, defType);
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

                var dyn = dynamicMethod.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>;
                dyn(model, value);
            }
            catch (Exception ex) { }
        }
    }
}
