using System;
using System.Reflection;
using Scalar.AspNetCore;

class Program {
    static void Main() {
        var t = typeof(ScalarOptions);
        foreach(var p in t.GetProperties()) {
            Console.WriteLine($"Type: {p.PropertyType.Name}, Name: {p.Name}");
        }
    }
}
