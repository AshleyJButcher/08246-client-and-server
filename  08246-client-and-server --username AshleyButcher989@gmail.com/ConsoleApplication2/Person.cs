namespace WhereIsServer
{
    class Person
    {
        private string _name;
        private string _location;

        public Person(string name, string location)
        {
            SetLocation(location);
            SetName(name);
        }

        public string GetName()
        {
            return _name;
        }

        public void SetName(string inputstring)
        {
            _name = inputstring;
        }

        public string GetLocation()
        {
            return _location;
        }

        public void SetLocation(string inputstring)
        {
            _location = inputstring;
        }
    }
}
