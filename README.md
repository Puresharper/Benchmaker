# Benchmaker

Benchmark .NET code source to compare which is more efficient.

_nuget : https://www.nuget.org/packages/Benchmaker_

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
        Activation
            [none] = 0
            Activator = 0
            ConstructorInfo = 0
            Lambda = 0
            Expression = 1
            FormatterServices = 0
            DynamicMethod = 0
        Warmup
            [none] = 0
            Activator = 0
            ConstructorInfo = 0
            Lambda = 0
            Expression = 0
            FormatterServices = 0
            DynamicMethod = 0
        Loop
            [1]
                ConstructorInfo = 16571
                DynamicMethod = 556
                Activator = 5150
                Expression = 548
                FormatterServices = 6665
                Lambda = 553
                [none] = 380
            [2]
                [none] = 379
                Expression = 600
                ConstructorInfo = 18815
                Activator = 6425
                Lambda = 570
                FormatterServices = 7127
                DynamicMethod = 558
            [3]
                [none] = 382
                Lambda = 562
                Expression = 554
                FormatterServices = 6157
                DynamicMethod = 558
                Activator = 5150
                ConstructorInfo = 16604

     ===============================
                    [none] :   100 %
     [1]     DynamicMethod :   140 %
     [2]        Expression :   140 %
     [3]            Lambda :   140 %
     [4]         Activator : 1 400 %
     [5] FormatterServices : 1 700 %
     [6]   ConstructorInfo : 4 500 %
     ===============================
    Press any key to continue...

## FAQ

_**- How much iteration are used in loop to measure perdormance?**_
_Sampling is done on reference action to choose adequat iteration to keep a good measure without waiting so long_

_**- What is the display number in log?**_
_Display number is an execution time in ms_
