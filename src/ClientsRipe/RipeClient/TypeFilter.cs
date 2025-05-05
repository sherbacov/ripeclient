using System;

namespace ClientsRipe
{
    [Flags]
    public enum TypeFilter : short
    {
        None = 0,
        Autnum = 1,
        Inetnum = 2,
        Inetnum6 = 4,
        Route = 8,
        Route6 = 16,
        Person = 32
    }
}
