using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace ContactClassification;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        MySqlConnection conn = new MySqlConnection("Server=enki;Database=registration;Uid=stephen;Pwd=yaV2nEm2CS484VeFjq6R;");
        conn.Open();
        var contactList = LoadContacts(conn);


        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT r.reg_id, r.first_name, r.last_name, r.address, r.city, r.state, r.zipcode, r.country, r.best_phone, r.second_phone, r.email, p.aka_name 
FROM registration.registration r
INNER JOIN registration.person p on p.reg_id = r.reg_id and p.idx = 0
WHERE event_id = 23 ORDER BY reg_id desc"; 
        using var rdr = cmd.ExecuteReader();
        while(rdr.Read())
        {
            var reg = LoadRegistrationFromRecordSet(rdr);
            var bestScore = 0;
            var bestContact = new Contact();
            foreach(var contact in contactList)
            {
                var score = 0;
                score += Compare(reg.FirstName,contact.FirstName, 5);
                score += Compare(reg.LastName, contact.LastName, 3);
                score += Compare(reg.Email,contact.Email, 2);
                score += Compare(reg.MailingStreet,contact.MailingStreet, 2);
                score += Compare(reg.MailingCity,contact.MailingCity, 2);
                score += Compare(reg.MailingPostalCode,contact.MailingPostalCode, 2);
                score += Compare(reg.MailingCountry,contact.MailingCountry, 2);
                if (reg.MailingState != "MA") score += Compare(reg.MailingState,contact.MailingState,1);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestContact = contact;
                }
            }
            Console.WriteLine($"Input:    {reg.FirstName} {reg.LastName} ({reg.LegalName}) {reg.Email} {reg.MailingStreet} {reg.MailingCity} {reg.MailingState}");
            if (bestScore >= 7)
            {
                Console.WriteLine($"  Contact:{bestContact.FirstName} {bestContact.LastName} {bestContact.Email} {bestContact.MailingStreet} {bestContact.MailingCity} {bestContact.MailingState} {bestScore}");
            }
        }

    }

    static int Compare(string a, string b, int maxScore)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
        //return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        var score = Compute(a, b);
        return score > maxScore ? 0 : maxScore - score;
    }

    static bool Compare(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
        //return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        return Compute(a, b) <= 3;
    }

    static int Compute(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Verify arguments.
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Initialize arrays.
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Begin looping.
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                // Compute cost.
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                d[i - 1, j - 1] + cost);
            }
        }
        // Return cost.
        return d[n, m];
    }




    static Registration LoadRegistrationFromRecordSet(MySqlDataReader rdr)
    {
        return new Registration()
        {
            RegId = rdr.GetInt64(0),
            FirstName = rdr.GetString(1),
            LastName = rdr.GetString(2),
            MailingStreet = rdr.GetString(3),
            MailingCity = rdr.GetString(4),
            MailingState = rdr.GetString(5),
            MailingPostalCode = rdr.GetString(6),
            MailingCountry = rdr.GetString(7),
            BestPhone = rdr.GetString(8),
            SecondPhone = rdr.GetString(9),
            Email = rdr.GetString(10),
            LegalName = rdr.GetString(11)
        };
    }


    static List<Contact> LoadContacts(MySqlConnection conn)
    {
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FirstName, LastName, MailingStreet, MailingCity, MailingState, MailingPostalCode, MailingCountry, phone, homephone, Email FROM rowan.contact";
        using var rdr = cmd.ExecuteReader();
        var contactList = new List<Contact>();
        while (rdr.Read())
        {
            contactList.Add(
            new Contact()
            {
                Id = rdr.GetString(0),
                FirstName = rdr.GetString(1),
                LastName = rdr.GetString(2),
                MailingStreet = rdr.GetString(3),
                MailingCity = rdr.GetString(4),
                MailingState = rdr.GetString(5),
                MailingPostalCode = rdr.GetString(6),
                MailingCountry = rdr.GetString(7),
                Phone = rdr.GetString(8),
                Homephone = rdr.GetString(9),
                Email = rdr.GetString(10)
            }
            );
        }
        rdr.Close();
        return contactList;
    }
}

#nullable disable

public class Registration
{
    public long RegId { get; set; }
    public string FirstName { get; set; }
    public string  LastName {get; set;} 
    public string  LegalName {get; set;} 
    public string  MailingStreet {get; set;} 
    public string  MailingCity {get; set;} 
    public string  MailingState {get; set;} 
    public string  MailingPostalCode {get; set;} 
    public string  MailingCountry {get; set;} 
    public string  BestPhone {get; set;} 
    public string  SecondPhone {get; set;} 
    public string  Email { get; set;}
}

public class Contact
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MailingStreet { get; set; }
    public string MailingCity { get; set; }
    public string MailingState { get; set; }
    public string MailingPostalCode { get; set; }
    public string MailingCountry { get; set; }
    public string Phone { get; set; }
    public string Homephone { get; set; }
    public string Email { get; set; }
}
