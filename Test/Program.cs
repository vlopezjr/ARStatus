using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARStatus;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
            Console.ReadLine();
        }

        private void Run()
        {
            CalculateARStatus calcStatus = new CalculateARStatus();

            //calcStatus.Update(19440, AccountStatus.ManualHold);


            //calcStatus.UpdateAll();
            calcStatus.Load(19438);

            string html = calcStatus.GetCallingData("11/2/18", "11/3/18");

        }
    }
}