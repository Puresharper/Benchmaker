using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;

namespace Benchmaker.Demo
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            //Create benchmark with object parameterless constructor call as reference action.
            var _benchmark = new Benchmark(() => new Action(() => new object()));

            //Add Activator.CreateInstance call as alternative.
            _benchmark.Add("Activator", () =>
            {
                var _type = typeof(object);
                return new Action(() => { Activator.CreateInstance(_type); });
            });

            //Add Activator.CreateInstance call as alternative.
            _benchmark.Add("Generic Activator", () =>
            {
                return new Action(() => { Activator.CreateInstance<object>(); });
            });

            //Add ConstructorInfo.Invoke as alternative
            _benchmark.Add("ConstructorInfo", () =>
            {
                var _constructor = typeof(object).GetConstructor(Type.EmptyTypes);
                var _arguments = new object[0];
                return new Action(() => { _constructor.Invoke(_arguments); });
            });

            //Add Lambda as alternative
            _benchmark.Add("Lambda", () =>
            {
                var _activate = new Func<object>(() => new object());
                return new Action(() => { _activate(); });
            });

            //Add Lambda Expression as alternative
            _benchmark.Add("Expression", () =>
            {
                var _activate = Expression.Lambda<Func<object>>(Expression.New(typeof(object))).Compile();
                return new Action(() => { _activate(); });
            });

            //Add FormatterServices.GetUninitializedObject as alternative
            _benchmark.Add("FormatterServices", () =>
            {
                var _type = typeof(object);
                return new Action(() => { FormatterServices.GetUninitializedObject(_type); });
            });

            //Add DynamicMethod as alternative
            _benchmark.Add("DynamicMethod", () =>
            {
                var _type = typeof(object);
                var _method = new DynamicMethod(string.Empty, _type, new Type[] { _type }, _type, true);
                var _body = _method.GetILGenerator();
                _body.Emit(OpCodes.Newobj, _type.GetConstructor(Type.EmptyTypes));
                _body.Emit(OpCodes.Ret);
                var _activate = _method.CreateDelegate(typeof(Func<object>), null) as Func<object>;
                return new Action(() => { _activate(); });
            });


            //Run benchmark.
            _benchmark.Run();
        }
    }
}
