using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GoogleHashcode {
    public class Program {
        static void Main(string[] args) {
            var lines = Read_file();
            var streetList = new List<Street>();
            var carList = new List<Car>();
            var timeLimit = 0;
            var intersections = 0;
            var streets = 0;
            var cars = 0;
            var points = 0;

            var firstLine = lines.First().Split(' ').Select(x => Convert.ToInt32(x)).ToList();
            timeLimit = firstLine[0];
            intersections = firstLine[1];
            streets = firstLine[2];
            cars = firstLine[3];
            points = firstLine[4];

            foreach (var line in lines.Skip(1).Take(streets)) {
                var splitted = line.Split(' ');
                streetList.Add(new Street(Convert.ToInt32(splitted[0]), Convert.ToInt32(splitted[1]), splitted[2], Convert.ToInt32(splitted[3])));
            }

            foreach (var line in lines.Skip(1+streets).Take(cars))
            {
                var splitted = line.Split(' ');

                var car = new Car(int.Parse(splitted[0]));
                foreach (var number in splitted.Skip(1))
                {
                    car.Streets.Add(number);
                }
                carList.Add(car);
            }
            var Time = 0;

            carList.OrderBy(x => x.AmountOfStreets);

            while (Time < timeLimit) {

            }
        }
        public static IEnumerable<string> Read_file()
        {
            var resources = Assembly.GetCallingAssembly().GetManifestResourceNames().ToList();
            using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resources.Single(x => x.EndsWith("NewFile1.txt")));
            using var reader = new StreamReader(stream ?? throw new InvalidOperationException());
            return reader.ReadAllLines().ToArray();
        }   

        
    }

    public static class Helpers
    {
        public static IEnumerable<string> ReadAllLines(this StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }

    public class Car
    {
        public int AmountOfStreets { get; set; }
        public List<string> Streets { get; set; } = new List<string>();

        public Car(int amountOfStreets) {
            AmountOfStreets = amountOfStreets;
        }
    }

    public class StreetLight {
        bool IsOn { get; set; } = false;

        public void Switch() {
            IsOn = !IsOn;
        }
    }

    public class Street
    {
        public int BeginIntersection { get; set; }
        public int EndIntersection { get; set; }
        public string StreetName { get;   set; }
        public int Duration { get;   set; }
        public StreetLight Light { get; set; } = new StreetLight();

        public Street(int beginIntersection, int endIntersection, string streetName, int duration) {
            BeginIntersection = beginIntersection;
            EndIntersection = endIntersection;
            StreetName = streetName;
            Duration = duration;
        }
    }
}


