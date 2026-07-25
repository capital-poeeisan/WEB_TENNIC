using System;
using System.Collections.Generic;

namespace WEB_TENNIC.Models;

public partial class WtMStaff
{
    public string StaffCd { get; set; } = null!;

    public DateOnly ChangeDate { get; set; }

    public string? StaffName { get; set; }

    public string? StaffKana { get; set; }

    public string? StoreCd { get; set; }

    public string? Bmncd { get; set; }

    public string? MenuCd { get; set; }

    public string? StoreMenuCd { get; set; }

    public string? AuthorizationsCd { get; set; }

    public string? StoreAuthorizationsCd { get; set; }

    public string? PositionCd { get; set; }

    public DateOnly? JoinDate { get; set; }

    public DateOnly? LeaveDate { get; set; }

    public string? Password { get; set; }

    public string? Remarks { get; set; }

    public string? ReceiptPrint { get; set; }

    public byte DeleteFlg { get; set; }

    public byte UsedFlg { get; set; }

    public string? InsertOperator { get; set; }

    public DateTime? InsertDateTime { get; set; }

    public string? UpdateOperator { get; set; }

    public DateTime? UpdateDateTime { get; set; }
}
