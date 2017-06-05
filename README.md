[![NuGet](https://img.shields.io/nuget/v/benchmaker.svg)](https://www.nuget.org/packages/Benchmaker)
# Benchmaker

Benchmark .NET code source to compare which is more efficient. 

## Example

### Code

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
    
### Output

    Benchmark
        Activation : Ticks
            [none] = 30
            Activator = 20
            Generic Activator = 20
            ConstructorInfo = 180
            Lambda = 20
            Expression = 14.000
            FormatterServices = 30
            DynamicMethod = 730
        Warmup : Ticks
            [none] = 10
            Activator = 90
            Generic Activator = 620
            ConstructorInfo = 30
            Lambda = 550
            Expression = 40
            FormatterServices = 270
            DynamicMethod = 20
        Loop : iteration / second
            [1]
                Expression = 38.000.000
                Lambda = 46.000.000
                FormatterServices = 6.000.000
                ConstructorInfo = 2.400.000
                [none] = 110.000.000
                Generic Activator = 5.400.000
                DynamicMethod = 48.000.000
                Activator = 8.100.000
            [2]
                DynamicMethod = 64.000.000
                ConstructorInfo = 3.100.000
                Activator = 8.500.000
                [none] = 150.000.000
                Lambda = 61.000.000
                FormatterServices = 7.200.000
                Expression = 64.000.000
                Generic Activator = 7.000.000
            [3]
                Generic Activator = 7.900.000
                ConstructorInfo = 3.600.000
                Lambda = 74.000.000
                Expression = 75.000.000
                FormatterServices = 9.100.000
                Activator = 10.000.000
                [none] = 170.000.000
                DynamicMethod = 76.000.000
            [4]
                ConstructorInfo = 3.500.000
                Generic Activator = 8.400.000
                [none] = 190.000.000
                Activator = 11.000.000
                FormatterServices = 10.000.000
                DynamicMethod = 83.000.000
                Lambda = 79.000.000
                Expression = 83.000.000
            [5]
                ConstructorInfo = 4.100.000
                Generic Activator = 9.200.000
                DynamicMethod = 83.000.000
                Activator = 11.000.000
                Lambda = 79.000.000
                FormatterServices = 10.000.000
                Expression = 87.000.000
                [none] = 220.000.000
            [6]
                Activator = 13.000.000
                FormatterServices = 11.000.000
                [none] = 220.000.000
                ConstructorInfo = 4.600.000
                DynamicMethod = 90.000.000
                Lambda = 85.000.000
                Generic Activator = 10.000.000
                Expression = 92.000.000
    
     ===============================
                    [none] :   100 %
     [1]     DynamicMethod :   240 %
     [2]        Expression :   240 %
     [3]            Lambda :   260 %
     [4]         Activator : 1.700 %
     [5] FormatterServices : 2.000 %
     [6] Generic Activator : 2.200 %
     [7]   ConstructorInfo : 4.900 %
     ===============================
    Appuyez sur une touche pour continuer...

## FAQ

_**- How much iteration are used in loop to measure perdormance?**_
_Sampling is done on reference action to choose adequat iteration to keep a good measure without waiting so long_

_**- What is the display number in log?**_
_Display number is an execution time in ticks for activation and warmup, then iteraton per second for loops.
