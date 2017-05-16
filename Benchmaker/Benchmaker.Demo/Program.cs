using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benchmaker.Demo
{
    class Program
    {
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

            //Add ConstructorInfo.Invoke as alternative
            _benchmark.Add("ConstructorInfo", () =>
            {
                var _constructor = typeof(object).GetConstructor(Type.EmptyTypes);
                var _arguments = new object[0];
                return new Action(() => { _constructor.Invoke(_arguments); });
            });

            //Run benchmark.
            _benchmark.Run();
        }
    }
}
