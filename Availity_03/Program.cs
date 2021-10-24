using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Availity_03
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting App....");

            //path to file in bin/debug path...
            string _path = AppDomain.CurrentDomain.BaseDirectory + "input.txt";
            
            //read the incoming text file to list of Enrollee objects
            List<Enrollee> enrollees = new CSVReader(_path).ReadEnrollees().ToList();

            //if duplicate names, seleect highest version only
            var unique_enrollees = enrollees.GroupBy(x => x.FullName).Select(y => y.OrderByDescending(z => z.Version).FirstOrDefault());

            //group according to companies
            var enrollees_companies = unique_enrollees.GroupBy(x => x.InsuranceCompany)
                                                        .Select(grp => grp.ToList())
                                                        .ToList();

            //write enrollees to company-unique files
            foreach (var company in enrollees_companies)
            {
                CSVWriter writer = new CSVWriter(company);
                writer.WriteToFile();
            }
            Console.WriteLine("Done.");
            Console.Read();
        }
    }

    public class Enrollee
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string InsuranceCompany { get; set; }
        public string FormatString { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(FormatString))
                FormatString = "{0},{1},{2}";
            return string.Format(FormatString, Id, FullName, InsuranceCompany);
        }
    }

    public class CSVWriter
    {
        private List<Enrollee> _company;

        public CSVWriter(List<Enrollee> companyList)
        {
            _company = companyList;
        }
        public void WriteToFile()
        {
            string _file = AppDomain.CurrentDomain.BaseDirectory + ((Enrollee)_company[0]).InsuranceCompany.ToString() + ".txt";

            using (StreamWriter sw = new StreamWriter(_file))
            {
                foreach (Enrollee e in _company.OrderBy(y => y.LastName).ThenBy(y => y.FirstName))
                {
                    sw.WriteLine(e.ToString());
                }
            }
        }
    }

    public class CSVReader
    {
        private string _path;

        public CSVReader(string path)
        {
            _path = path;
        }
        public IEnumerable<Enrollee> ReadEnrollees()
        {
            var fileData = ReadFile();

            var enrollees = new List<Enrollee>();
            var lines = fileData.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                try
                {
                    var elements = line.Split(',');
                    var enrollee = new Enrollee()
                    {
                        Id = int.Parse(elements[0].Trim()),
                        FullName = elements[1].Trim(),
                        FirstName = elements[1].Trim().Split(' ')[0].Trim(),
                        LastName = elements[1].Trim().Split(' ')[1].Trim(),
                        Version = int.Parse(elements[2].Trim()),
                        InsuranceCompany = elements[3].Trim()
                    };
                    enrollees.Add(enrollee);
                }
                catch (Exception)
                {
                    // Skip the bad record, log it, and move to the next record
                    // log.Write($"Unable to parse record: {line}")
                }
            }

            return enrollees;
        }

        private string ReadFile()
        {
            using (var reader = new StreamReader(_path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
