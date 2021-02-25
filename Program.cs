using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GoogleHashcode {
    public class Program {
        static void Main(string[] args) {
            var lines = Read_file();
            var steps = new List<Step>();
            var streetList = new List<Street>();
            var instersectionList = new List<InterSection>();
            var carList = new List<Car>();
            var timeLimit = 0;
            var intersections = 0;
            var streets = 0;
            var cars = 0;
            var points = 0;

            #region input
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

                var car = new Car(int.Parse(splitted[0]), 0);
                foreach (var number in splitted.Skip(1))
                {
                    car.Streets.Add(number);
                }
                carList.Add(car);
                var street = streetList.Single(x => x.StreetName == car.Streets.First());
                street.Queue.Add(new StreetCar(car,0));
            }
            #endregion

            var time = 0;
            var firstCar = carList.OrderBy(x => x.AmountOfStreets).First();
            List<string> cwLines = new List<string>();
            while (time < timeLimit) {
                var interSectionsWith1ToMany = from street in streetList
                                    group street by street.EndIntersection into grp
                                    where grp.Count()==1
                                    select grp.Key;
                var otherInterSections = (from street in streetList
                                         group street by street.EndIntersection into grp
                                         select grp.Key).ToList().Except(interSectionsWith1ToMany);

                var intersectionNumbers = interSectionsWith1ToMany.Count() + otherInterSections.Count();
                cwLines.Add(intersectionNumbers.ToString());
                foreach (var item in interSectionsWith1ToMany)
                {
                    cwLines.Add(item.ToString());
                    cwLines.Add("1");
                    var street = streetList.Single(x => x.EndIntersection == item);
                    cwLines.Add($"{street.StreetName} 1");
                }
                foreach (var item in otherInterSections)
                {
                    cwLines.Add(item.ToString());
                    cwLines.Add(streetList.Where(x => x.EndIntersection == item).Count().ToString());
                    foreach (var street in streetList.Where(x => x.EndIntersection == item))
                    {
                        cwLines.Add($"{street.StreetName} 1");
                    }
                }
                time = timeLimit;
            }

            
            using StreamWriter file = new StreamWriter("WriteLines2.txt");

            foreach (string line in cwLines)
            {
                file.WriteLine(line);
            }

        }
          
        public static void Move(List<Street> streets)
        {
            foreach(var street in streets)
            {
                StreetCar carToMove = null;
                if (street.Light.IsOn)
                {
                    carToMove = street.Queue.Where(x =>
                    x.DurationOnStreet == 0 &&
                    x.Car.Index != x.Car.Streets.Count() - 1).FirstOrDefault();

                }

                var otherCars = carToMove != null ? street.Queue.Where(x => x.Car.Guid != carToMove.Car.Guid) : street.Queue;
                foreach (var car in otherCars)
                {
                    car.Step();
                }
                if(carToMove != null)
                {
                    var currentStreet = carToMove.Car.Streets.Single(x => x == street.StreetName);
                    var nextStreetName = carToMove.Car.Streets[carToMove.Car.Index + 1];
                    var nextStreet = streets.Single(x => x.StreetName == nextStreetName);
                    var streetCar = new StreetCar(carToMove.Car, nextStreet.Duration);
                    streetCar.MovedThisTurn = true;
                    street.Queue.Remove(carToMove);
                    nextStreet.Queue.Add(streetCar);
                }
                street.Light.Step();
            }
        }

        public static void SetMovedThisTurnFalse(List<Street> streets)
        {
            foreach (var street in streets)
            {
                foreach (var car in street.Queue)
                {
                    car.MovedThisTurn = false;
                }
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
        public Guid Guid { get; set; }
        public int AmountOfStreets { get; set; }
        public int Index { get; set; }
        public List<string> Streets { get; set; } = new List<string>();

        public Car(int amountOfStreets, int index) {
            Guid = Guid.NewGuid();
            Index = index;
            AmountOfStreets = amountOfStreets;
        }
    }

    public class StreetLight {
        public bool IsOn { get; set; } = false;
        public int Duration { get; set; }

        public void Switch(int duration) {
            IsOn = !IsOn;
            Duration = duration;
        }

        public void Step()
        {
            if (Duration > 0) Duration--;
            if (Duration == 0 && IsOn) Switch(0);
        }
    }

    public class Street
    {
        public int BeginIntersection { get; set; }
        public int EndIntersection { get; set; }
        public string StreetName { get;   set; }
        public int Duration { get;   set; }
        public List<StreetCar> Queue { get; set; } = new List<StreetCar>();
        public StreetLight Light { get; set; } = new StreetLight();

        public Street(int beginIntersection, int endIntersection, string streetName, int duration) {
            BeginIntersection = beginIntersection;
            EndIntersection = endIntersection;
            StreetName = streetName;
            Duration = duration;
        }
    }

    public class StreetCar
    {
        public Car Car { get; set; }
        public bool MovedThisTurn { get; set; }
        public int DurationOnStreet { get; set; }
        public StreetCar(Car car, int streetDuration)
        {
            Car = car;
            DurationOnStreet = streetDuration;
        }
        public void Step()
        {
            if (!MovedThisTurn)
            {
                if (DurationOnStreet > 0) DurationOnStreet--;
                MovedThisTurn = true;
            }
        }
    }

    public class Step
    {
        public string StreetName { get; set; }
        public int Duration { get; set; }
    }

    public class InterSection
    {
        public List<Street> Incoming { get; set; }
        public List<Street> Output { get; set; }
    }
}


