using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Benchmaker
{
    /// <summary>
    /// Benchmark.
    /// </summary>
    public class Benchmark
    {
        static Benchmark()
        {
            var _seed = Environment.TickCount;
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
        }

        static private string None = "[none]";

        static private string Round(long value)
        {
            var _duration = value.ToString();
            if (_duration.Length == 2)
            {
                if (int.Parse(_duration[1].ToString()) < 5) { _duration = $"{ _duration[0] }0"; }
                else { _duration = $"{ int.Parse(_duration[0].ToString()) + 1 }0"; }
            }
            else if (_duration.Length > 2) { _duration = $"{ _duration[0] }{ _duration[1] }".PadRight(_duration.Length, '0'); }
            return Regex.Replace(int.Parse(_duration).ToString("N0"), "[^0-9]", ".");
        }

        static private long Average(IEnumerable<long> enumerable)
        {
            var _list = enumerable.ToList();
            //var _average = Convert.ToInt64(_list.Average());
            //var _count = 3;// Convert.ToInt32(_list.Count * 0.6);
            //while (_list.Count > _count)
            //{
            //    var _max = 0;
            //    for (var _index = 0; _index < _list.Count; _index++) { if (_list[_index] - _average > _list[_max] - _average) { _max = _index; } }
            //    _list.RemoveAt(_max);
            //}
            return Convert.ToInt64(_list.Average());
        }

        private Dictionary<string, Func<Action>> m_Dictionary;

        /// <summary>
        /// Create a benchmark.
        /// </summary>
        /// <param name="none">Initializer for reference action</param>
        public Benchmark(Func<Action> none)
        {
            this.m_Dictionary = new Dictionary<string, Func<Action>>();
            this.m_Dictionary.Add(Benchmark.None, none);
            RuntimeHelpers.PrepareDelegate(none);
        }

        /// <summary>
        /// Add an alternative action.
        /// </summary>
        /// <param name="name">Name of alternative action</param>
        /// <param name="alternative">Initializer for alternative action</param>
        public void Add(string name, Func<Action> alternative)
        {
            this.m_Dictionary.Add(name, alternative);
            RuntimeHelpers.PrepareDelegate(alternative);
        }

        /// <summary>
        /// Run benchmark and log into console.
        /// </summary>
        /// <returns>Dashboard</returns>
        public Dictionary<string, int> Run()
        {
            return this.Run(Console.WriteLine);
        }

        /// <summary>
        /// Run benchmark.
        /// </summary>
        /// <param name="log">Logger</param>
        /// <returns>Dashboard</returns>
        public Dictionary<string, int> Run(Action<string> log)
        {
            var _log = log != null;
            var _stopwatch = new Stopwatch();
            var _activation = this.m_Dictionary[Benchmark.None];
            var _action = null as Action;
            if (_log) { log("Benchmark"); }
            var _list = new List<KeyValuePair<string, long>>();
            var _array = this.m_Dictionary.ToArray();
            var _buffer = new KeyValuePair<string, Action>[_array.Length];
            if (_log) { log("    Activation : Ticks"); }
            for (var _loop = 0; _loop < _array.Length; _loop++)
            {
                var _name = _array[_loop].Key;
                var _activate = _array[_loop].Value;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                _stopwatch.Restart();
                _action = _activate();
                _stopwatch.Stop();
                _buffer[_loop] = new KeyValuePair<string, Action>(_name, _action);
                if (_log) { log($"        { _name } = { Benchmark.Round(_stopwatch.ElapsedTicks) }"); }
            }
            if (_log) { log("    Warmup : Ticks"); }
            foreach (var _item in _buffer)
            {
                _action = _item.Value;
                RuntimeHelpers.PrepareDelegate(_action);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                _stopwatch.Restart();
                _action();
                _stopwatch.Stop();
                if (_log) { log($"        { _item.Key } = { Benchmark.Round(_stopwatch.ElapsedTicks) }"); }
            }
            if (_log) { log("    Loop : iteration / second"); }
            _action = _activation();
            var _sample = 100;
            _action();
            while (true)
            {
                var _index = 0;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                _stopwatch.Restart();
                while (_index++ < _sample) { _action(); }
                _stopwatch.Stop();
                if (_stopwatch.ElapsedMilliseconds < 80) { _sample = _sample * 10; }
                else { break; }
            }
            var _sampling = _buffer.ToDictionary(_Item => _Item.Key, _Item => new List<long>());
            for (var _loop = 0; _loop < 3; _loop++)
            {
                foreach (var _item in _buffer)
                {
                    var _index = 0;
                    _action = _item.Value;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    _stopwatch.Restart();
                    while (_index++ < 10) { _action(); }
                    _stopwatch.Stop();
                    _sampling[_item.Key].Add(_stopwatch.ElapsedTicks);
                }
            }
            var _balancing = _sampling.ToDictionary(_Item => _Item.Key, _Item => Convert.ToInt64(_Item.Value.Average()));
            var _authority = _balancing[Benchmark.None];
            _balancing = _balancing.ToDictionary(_Item => _Item.Key, _Item => Convert.ToInt64(_sample * _authority / _Item.Value));
            var _random = new Random();
            for (var _loop = 0; _loop < 6; _loop++)
            {
                if (_log) { log($"        [{ _loop + 1 }]"); }
                var _randomly = _buffer.ToArray();
                for (var _index = _randomly.Length - 1; _index > 0; --_index)
                {
                    var _position = _random.Next(_index + 1);
                    var _item = _randomly[_index];
                    _randomly[_index] = _randomly[_position];
                    _randomly[_position] = _item;
                }
                foreach (var _item in _randomly)
                {
                    var _index = 0L;
                    var _iteration = _balancing[_item.Key];
                    _action = _item.Value;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    _stopwatch.Restart();
                    while (_index++ < _iteration) { _action(); }
                    _stopwatch.Stop();
                    if (_log) { log($"            { _item.Key } = { Benchmark.Round(Convert.ToInt64(1000 * _iteration / _stopwatch.ElapsedMilliseconds)) }"); }
                    _list.Add(new KeyValuePair<string, long>(_item.Key, Convert.ToInt64(_sample * _stopwatch.ElapsedTicks / _iteration)));
                }
            }
            var _dashboard = _list.GroupBy(_Measure => _Measure.Key, _Measure => _Measure.Value).Select(_Measure => new { Name = _Measure.Key, Duration = Benchmark.Average(_Measure) }).ToArray();
            var _native = _dashboard.Single(_Measure => _Measure.Name == Benchmark.None).Duration;
            var _dictionary = _dashboard.ToDictionary(_Measure => _Measure.Name, _Measure => Convert.ToInt64((_Measure.Duration * 100) / _native));
            var _display = _dictionary.OrderBy(_Measure => _Measure.Value * (_Measure.Key == Benchmark.None ? 0 : 1)).ThenBy(_Measure => _Measure.Key).Select(_Measure => new { Name = _Measure.Key, Duration = Benchmark.Round(_Measure.Value) }).ToArray();
            if (_log)
            {
                var _max = _dictionary.Select(_Measure => _Measure.Key.Length).Max();
                var _length = _display.Select(_Measure => _Measure.Duration.Length).Max();
                var _size = _display.Count().ToString().Length;
                var _line = new string('=', _max + _length + 8 + _size);
                var _index = -1;
                Console.WriteLine();
                Console.WriteLine($" { _line }");
                foreach (var _measure in _display) { Console.WriteLine($" { (++_index == 0 ? new string(' ', _size + 2) : string.Concat("[", _index, "]")) } { _measure.Name.PadLeft(_max, ' ') } : { _measure.Duration.PadLeft(_length) } %"); }
                Console.WriteLine($" { _line }");
            }
            return _display.ToDictionary(_Measure => _Measure.Name, _Measure => int.Parse(Regex.Replace(_Measure.Duration, "[^0-9]", "")));
        }
    }
}
