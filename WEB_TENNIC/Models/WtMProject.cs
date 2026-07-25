using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEB_TENNIC.Models;

public partial class WtMProject
{
    [Key]
    public string ProjectCd { get; set; } = null!;
    public string CustomerCd { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public int OrderAmt { get; set; }

    public byte EndFlag { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? UpdateDateTime { get; set; }

    public DateTime? DeleteDateTime { get; set; }
}
