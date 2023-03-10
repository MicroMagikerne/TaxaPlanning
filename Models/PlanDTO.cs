using System;

namespace PlanningService.Models;

public class PlanDTO
{

    public string Kundenavn { get; set; }

    public DateTime Starttidspunkt { get; set; }

    public string Startsted { get; set; }

    public string Slutsted { get; set; }
     public PlanDTO(string kundenavn, DateTime starttidspunkt, string startsted, string slutsted)
        {
            Kundenavn = kundenavn;
            Starttidspunkt = starttidspunkt;
            Startsted = startsted;
            Slutsted = slutsted;
        }

          public PlanDTO()
        {
           
        }

}