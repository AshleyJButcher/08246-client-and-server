using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhereIsServer
{
    class Person
    {
        private string name;
        private string location;

        public Person(string name, string location)
        {
            SetLocation(location);
            SetName(name);
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string inputstring)
        {
            name = inputstring;
        }

        public string GetLocation()
        {
            return location;
        }

        public void SetLocation(string inputstring)
        {
            location = inputstring;
        }
    }
}
