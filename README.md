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

    //Run benchmark.
    _benchmark.Run();
    
### Output

    Benchmark
        Activation
            [none] = 0
            Activator = 0
            ConstructorInfo = 1
        Warmup
            [none] = 0
            Activator = 0
            ConstructorInfo = 0
        Loop
            [1]
                [none] = 62
                Activator = 1188
                ConstructorInfo = 3268
            [2]
                Activator = 990
                ConstructorInfo = 3230
                [none] = 62
            [3]
                ConstructorInfo = 2926
                [none] = 53
                Activator = 792

     =============================
                  [none] :   100 %
     [1]       Activator : 1 800 %
     [2] ConstructorInfo : 5 200 %
     =============================
    Press any key to continue...
