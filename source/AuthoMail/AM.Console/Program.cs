using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using(IDemo d = new Demo())
            {
                var list = d.GetData();

                list.ForEach(item => System.Console.WriteLine(item));
            }
        }
    }
}
