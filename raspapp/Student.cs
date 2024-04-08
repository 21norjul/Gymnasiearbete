using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raspapp
{
    internal class Student
    {
        public string TaggID { get; set; }
        public int Laps { get; set; }
        public List<string> Times { get; set; } = new List<string>();   // List to store multiple times

    }

}
