using System;
using System.Collections.Generic;

namespace WEB_TENNIC.Models;

public partial class WtProjectDetail
{
    public int Id { get; set; }

    public string ProjectCd { get; set; } = null!;

    public string CustomerCd { get; set; } = null!;

    public string? StaffCd { get; set; }

    public byte Status { get; set; }

    public string? Remark { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? UpdateDateTime { get; set; }

    public DateTime? DeleteDateTime { get; set; }
}
