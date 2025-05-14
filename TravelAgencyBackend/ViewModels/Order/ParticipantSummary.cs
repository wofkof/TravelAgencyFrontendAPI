using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;
public class ParticipantSummaryViewModel
{
    public int ParticipantId { get; set; }

    [Display(Name = "姓名")]
    public string Name { get; set; }

    [Display(Name = "電話")]
    public string Phone { get; set; }

    [Display(Name = "Email")]
    public string Email { get; set; }
   
}

