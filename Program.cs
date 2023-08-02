using EFCoreProject.Data;
using EFCoreProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using static System.Formats.Asn1.AsnWriter;

namespace EFCoreProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection().AddDbContext<AppDbContext>(options => options.UseSqlServer("server=localhost;Database=EFCoreProject;Trusted_Connection=true;")).BuildServiceProvider();

            while (true)
            {
                Console.WriteLine("1 - Yeni Kişi Ve Adres Ekle");
                Console.WriteLine("2 - Kişileri Listele");
                Console.WriteLine("3 - Kişi Sil");
                Console.WriteLine("4 - Çıkış");
                Console.WriteLine("Seçiminizi Yapın(1/2/3)");
                var option = Console.ReadLine();
                

                switch (option)
                {
                    case "1":
                        AddNewPersonWithAdresses(serviceProvider); break;

                    case "2":
                        ListPeopleWithAdresses(serviceProvider); break;
                    case "3":
                        DeletePerson(serviceProvider);
                        break;

                    case "4":
                        Console.WriteLine("Uygulama Kapatılıyor..."); return;
                    default:
                        Console.WriteLine("Geçersiz seçenek, tekrar deneyin");
                        break;

                }
            }

        }

        static void AddNewPersonWithAdresses(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                Console.WriteLine("Adınız:");
                string FirstName = Console.ReadLine();

                Console.WriteLine("Soyadınız");
                string LastName = Console.ReadLine();

                Console.WriteLine("Doğum tarihi (yyyy-MM-dd)");

                if (DateTime.TryParse(Console.ReadLine(), out DateTime birthdate))
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var newPerson = new Person
                    {
                        FirstName = FirstName,
                        LastName = LastName,
                        BirthDate = birthdate
                    };
                    dbContext.People.Add(newPerson);
                    dbContext.SaveChanges();

                    Console.WriteLine("Adress: ");
                    string street = Console.ReadLine();
                    Console.WriteLine("City: ");
                    string city = Console.ReadLine();
                    Console.WriteLine("Zipcode: ");
                    string zipcode = Console.ReadLine();

                    if (int.TryParse(zipcode, out int zipCode))
                    {
                        Console.WriteLine("Country: ");
                        string country = Console.ReadLine();

                        var newAdress = new Adress
                        {
                            Street = street,
                            City = city,
                            ZipCode = zipCode,
                            PersonId = newPerson.Id,
                            Country = country
                            
                        };
                        dbContext.Adresses.Add(newAdress);
                        dbContext.SaveChanges();

                        Console.WriteLine("Yeni kişi ve adres başarıyla eklendi");
                    }
                    else
                    {
                        Console.WriteLine("Geçersiz posta kodu girişi.");
                    }
                }

                else
                {
                    Console.WriteLine("Geçersiz doğum tarihi girişi.");
                }


            }
        }
        static void DeletePerson(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Console.WriteLine("Silinecek kişinin ID'sini girin: ");
                if (int.TryParse(Console.ReadLine(), out int personId))
                {
                    var personToDelete = dbContext.People.FirstOrDefault(p => p.Id == personId);
                    if (personToDelete != null)
                    {
                        dbContext.People.Remove(personToDelete);
                        dbContext.SaveChanges();
                        Console.WriteLine($"Kişi ID {personId} başarıyla silindi.");
                    }
                    else
                    {
                        Console.WriteLine($"ID {personId} ile eşleşen kişi bulunamadı.");
                    }
                }
                else
                {
                    Console.WriteLine("Geçersiz ID girişi.");
                }
            }


        }

        static void ListPeopleWithAdresses(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();



                var people = dbContext.People.Include(p => p.Adresses).ToList();
                foreach (var person in people)
                {
                    Console.WriteLine($"{person.Id}: {person.FirstName}: {person.LastName}: {person.BirthDate}");
                    if (person.Adresses != null)
                    {
                        foreach (var adress in person.Adresses)
                        {
                            Console.WriteLine($"{adress.Street}, {adress.City}, {adress.Country}");
                        }
                    }
                }
            }
        }


    }
}