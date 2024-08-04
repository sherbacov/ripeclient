namespace ClientsRipe.LirResources.Models;

public class LirResourcesReply
{
    public string RegId { get; set; }
    public AsnResource[] AsnResources { get; set; }
    public Ipv4Allocations[] Ipv4Allocations { get; set; }
    public Ipv4Assignments[] Ipv4Assignments { get; set; }
    public Ipv4ErxResources[] Ipv4ErxResources { get; set; }
    public Ipv6Allocations[] Ipv6Allocations { get; set; }
    public object Ipv6Assignments { get; set; }
}

public class Ipv4Allocations
{
    public string RegistrationDate { get; set; }
    public string WhoisQueryUrl { get; set; }
    public Ticket Ticket { get; set; }
    public string Prefix { get; set; }
    public int Size { get; set; }
    public string Status { get; set; }

    public override string ToString()
    {
        return $"{Prefix}";
    }
}

public class Ipv6Allocations
{
    public string RegistrationDate { get; set; }
    public string WhoisQueryUrl { get; set; }
    public Ticket Ticket { get; set; }
    public string Prefix { get; set; }

    public override string ToString()
    {
        return $"{Prefix}";
    }
}

public class Ticket
{
    public string TicketNumber { get; set; }
    public string ShowTicketUrl { get; set; }
}

public class Ipv4Assignments
{
    public string RegistrationDate { get; set; }
    public string WhoisQueryUrl { get; set; }
    public Ticket Ticket { get; set; }
    public string Prefix { get; set; }
    public string OrganisationName { get; set; }
    public object Type { get; set; }
    public string IndependentResourceStatus { get; set; }
    
    public override string ToString()
    {
        return $"{Prefix}";
    }
}

public class Ipv4ErxResources
{
    public string RegistrationDate { get; set; }
    public string WhoisQueryUrl { get; set; }
    public Ticket Ticket { get; set; }
    public string Prefix { get; set; }
    public string OrganisationName { get; set; }
    
    public override string ToString()
    {
        return $"{Prefix}";
    }
}



public class AsnResource
{
    public string RegistrationDate { get; set; }
    public string WhoisQueryUrl { get; set; }
    public Ticket Ticket { get; set; }
    public string Number { get; set; }
    public string PeersQueryUrl { get; set; }
    public Organisation Organisation { get; set; }
    public string IndependentResourceStatus { get; set; }

    public override string ToString()
    {
        return $"AS{Number} [{Organisation?.Name}]";
    }
}


public class Organisation
{
    public string Name { get; set; }
}

