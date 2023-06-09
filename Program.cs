using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Reflection;

namespace Shelter
{
    interface IEater
    {
        public void Eat(int howMuchGrams);
    }
    [Serializable]
    abstract class Animal : IComparable<Animal>, ICloneable, IEater
    {
        private string name;
        private int _age;
        private double _weight;
        private int index;
        public int Index { get => index; }
        public string Name { get => name; }
        public double weight { get => _weight; set => _weight = value; }
        public int age { get => _age; set => _age = value; }

        public Animal(string name, int age, double weight, int index)
        {
            _age = age;
            _weight = weight;
            this.name = name;
            this.index = index;
        }
        public override string ToString()
        {
            return $"INDEX: {index}, NAME: {name}, AGE: {age}, WEIGHT: {weight}kg";
        }
        public abstract void Speak();
        public abstract object Clone();
        public virtual void Eat(int howMuchGrams)
        {
            weight += howMuchGrams;
        }
        public bool Equals(Animal other)
        {
            if (other == null || this == null)
                return false;
            return index.Equals(other.index);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(age, weight);
        }
        public int CompareTo(Animal other)
        {
            if (other == null || this == null)
                return 1;
            return index.CompareTo(other.index);
        }
        public class ByNameComparer : IComparer<Animal>
        {
            int IComparer<Animal>.Compare(Animal? x, Animal? y)
            {
                if (x == null || y == null)
                    return 1;
                return x.name.CompareTo(y.name);
            }
        }
        public class ByAgeComparer : IComparer<Animal>
        {
            int IComparer<Animal>.Compare(Animal? x, Animal? y)
            {
                if (x == null || y == null)
                    return 1;
                return x.age.CompareTo(y.age);
            }
        }
    }
    class Dog : Animal
    {
        private int runningSpeed;
        public Dog(string name, int age, double weight, int index, int runningSpeed) : base(name, age, weight, index)
        {
            this.runningSpeed = runningSpeed;
        }
        public override void Eat(int howMuchGrams)
        {
            base.Eat(howMuchGrams);
            Console.WriteLine("HAU!");
        }
        public override void Speak()
        {
            Console.WriteLine("Hau hau");
        }
        public override object Clone()
        {
            return new Dog(Name, age, weight, Index, runningSpeed);
        }
        public override string ToString()
        {
            return $"{base.ToString()}, RUNNING SPEED: {runningSpeed}km/h";
        }
    }
    class Cat : Animal
    {
        private int tailLength;
        public Cat(string name, int age, double weight, int index, int tailLength) : base(name, age, weight, index)
        {
            this.tailLength = tailLength;
        }
        public override void Eat(int howMuchGrams)
        {
            base.Eat(howMuchGrams);
            Console.WriteLine("MIAU!");
        }
        public override void Speak()
        {
            Console.WriteLine("Miau miau");
        }
        public override object Clone()
        {
            return new Dog(Name, age, weight, Index, tailLength);
        }
        public override string ToString()
        {
            return $"{base.ToString()}, TAIL LENGTH: {tailLength}cm";
        }
    }
    class Shelter<T> : IEnumerable where T : Animal
    {
        public string Name;
        private List<T> animalDatabase;
        public event AnimalAdoptedEventHandler animalAdopted;
        public Shelter(string name, List<T> animalDatabase)
        {
            this.animalDatabase = animalDatabase;
            Name = name;
        }
        [Obsolete("Use NewSort<u> method instead")]
        public void Sort()
        {
            if (animalDatabase == null || animalDatabase.Count == 0)
            {
                Console.WriteLine("You cannot sort empty or uninitialized list!");
            }
            animalDatabase.Sort();
        }
        public void NewSort<U>(U u) where U : IComparer<T>
        {
            if (animalDatabase == null || animalDatabase.Count == 0)
            {
                Console.WriteLine("You cannot sort empty or uninitialized list!");
            }
            animalDatabase.Sort(u);
        }
        public Animal SearchAnimal(int index)
        {
            if (index < 0)
                throw new ShelterDatabaseException("Index must be greater than zero");
            if (animalDatabase == null || animalDatabase.Count == 0)
            {
                return null;
            }
            foreach (Animal a in animalDatabase)
            {
                if (a.Index.CompareTo(index) == 0)
                    return a;
            }
            return null;
        }
        public void Adopt(Animal pet, string ownerName)
        {
            if (animalDatabase.Contains(pet))
            {
                animalDatabase.Remove((T)pet);
                if (pet != null)
                    animalAdopted(this, new AnimalAdoptedEventArgs(pet, ownerName));
            }
        }
        public void DisplayAnimalsNames()
        {
            Console.WriteLine("Animals names:");
            if (animalDatabase != null)
            {
                foreach (T animal in animalDatabase)
                {
                    Type type = typeof(T);
                    PropertyInfo nameProperty = type.GetProperty("Name");
                    if (nameProperty != null)
                    {
                        string name = (string)nameProperty.GetValue(animal);
                        Console.WriteLine(name);
                    }
                }
            }
        }

        public void Serialize(string fileName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                foreach (Animal a in animalDatabase)
                {
                    bf.Serialize(fs, a);
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine($"Serialization failed. Reason: {e.Message}");
            }
            finally
            {
                fs.Close();
            }
        }
        public List<Animal> Deserialize(string fileName)
        {
            List<Animal> temp = new List<Animal>();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read);
            try
            {
                foreach (Animal a in animalDatabase)
                {
                    temp.Add((Animal)bf.Deserialize(fs));
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine($"Deserialization failed. Reason: {e.Message}");
            }
            finally
            {
                fs.Close();
            }
            return temp;
        }
        public override string ToString()
        {
            IEnumerator en = GetEnumerator();
            en.Reset();
            StringBuilder sb = new StringBuilder(500);
            sb.AppendLine("Animal database:");
            while (en.MoveNext())
            {
                sb.AppendLine(en.Current.ToString());
            }
            return sb.ToString();
        }
        public IEnumerator GetEnumerator()
        {
            return new ShelterEnumerator(animalDatabase);
        }
        private class ShelterEnumerator : IEnumerator
        {
            private List<T> animalDatabase;
            private int currentIndex = -1;
            public ShelterEnumerator(List<T> animalDatabase)
            {
                this.animalDatabase = animalDatabase;
            }
            public object Current { get => animalDatabase[currentIndex]; }
            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < animalDatabase.Count;
            }
            public void Reset()
            {
                currentIndex = -1;
            }
        }
        class ShelterDatabaseException : ArgumentOutOfRangeException
        {
            public ShelterDatabaseException(string message) : base(message)
            {

            }
        }
    }
    delegate void AnimalAdoptedEventHandler(object sender, AnimalAdoptedEventArgs e);
    class AnimalAdoptedEventArgs : EventArgs
    {
        private Animal animal;
        private string ownerName;
        public AnimalAdoptedEventArgs(Animal animal, string ownerName)
        {
            this.animal = animal;
            this.ownerName = ownerName;
        }
        public string OwnerName { get => ownerName; }
        public Animal Animal { get => animal; }
    }
    class InformationBoard
    {
        StringBuilder sb;
        public InformationBoard(StringBuilder sb)
        {
            this.sb = sb;
            sb.AppendLine("INFORMATION BOARD");
        }
        public void InformAboutAdoption(object sender, AnimalAdoptedEventArgs e)
        {
            Shelter<Animal> s = (Shelter<Animal>)sender;
            sb.AppendLine($"Good news! {e.Animal.Name} was adopted by {e.OwnerName} from {s.Name} shelter!");
        }
        public void Read()
        {
            Console.WriteLine(sb.ToString());
        }
        public void AddInfo(string path)
        {
            FileStream fs = File.OpenRead(path);
            StreamReader sr = new StreamReader(fs);
            try
            {
                sb.AppendLine(sr.ReadToEnd());
            }
            catch (IOException e)
            {
                Console.WriteLine($"Failed to add informations. Reason: {e.Message}");
            }
            finally
            {
                fs.Close();
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //Create list of animals
            Dog d1 = new Dog("Odie", 2, 1.3, 1111, 19);
            Dog d2 = new Dog("Goofy", 4, 1.4, 2222, 18);
            Dog d3 = new Dog("Saba", 3, 3.2, 3333, 28);
            Cat c1 = new Cat("Carmel", 4, 1.3, 6666, 32);
            Cat c2 = new Cat("Zoja", 4, 1.4, 7777, 67);
            Cat c3 = new Cat("Doris", 2, 2.2, 8888, 12);
            Cat c4 = new Cat("Amber", 3, 1.7, 9999, 78);
            List<Animal> animals = new List<Animal>();
            animals.Add(d1);
            animals.Add(d2);
            animals.Add(d3);
            animals.Add(c1);
            animals.Add(c2);
            animals.Add(c3);
            animals.Add(c4);

            //Create instance of shelter
            Shelter<Animal> shelter = new Shelter<Animal>("'Four Paws'", animals);

            shelter.DisplayAnimalsNames();

            //Display informations after sort (IComparable and IEnumerable)
            shelter.Sort();

            Console.WriteLine("\nAnimals sorted by age:");
            shelter.NewSort(new Animal.ByAgeComparer());
            Console.WriteLine(shelter);

            Console.WriteLine("Animals sorted by name:");
            shelter.NewSort(new Animal.ByNameComparer());
            Console.WriteLine(shelter);

            //Create instance of information board
            InformationBoard informationBoard = new InformationBoard(new StringBuilder(500));
            shelter.animalAdopted += informationBoard.InformAboutAdoption;

            //Animal adoption
            shelter.Adopt(d1, "Jan Kowalski");
            shelter.Adopt(c3, "Maja Nowak");

            //Display (read) information contained in information board
            informationBoard.Read();
        }
    }
}
