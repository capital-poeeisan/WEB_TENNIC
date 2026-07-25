using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEB_TENNIC.Models;

public partial class WtMCustomer
{
    [Key]
    public string CustomerCd { get; set; } = null!;

    public DateOnly ChangeDate { get; set; }

    public byte VariousFlg { get; set; }

    public string? CustomerName { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? LongName1 { get; set; }

    public string? LongName2 { get; set; }

    public string? KanaName { get; set; }

    public byte StoreKbn { get; set; }

    public byte CustomerKbn { get; set; }

    public byte StoreTankaKbn { get; set; }

    public byte AliasKbn { get; set; }

    public byte BillingType { get; set; }

    public string? GroupName { get; set; }

    public byte BillingFlg { get; set; }

    public byte CollectFlg { get; set; }

    public string? BillingCd { get; set; }

    public string? CollectCd { get; set; }

    public DateOnly? BirthDate { get; set; }

    public byte Sex { get; set; }

    public string? Tel11 { get; set; }

    public string? Tel12 { get; set; }

    public string? Tel13 { get; set; }

    public string? Tel21 { get; set; }

    public string? Tel22 { get; set; }

    public string? Tel23 { get; set; }

    public string? ZipCd1 { get; set; }

    public string? ZipCd2 { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? President { get; set; }

    public string? Employee1 { get; set; }

    public string? Employee2 { get; set; }

    public string? MailAddress { get; set; }

    public string? TankaCd { get; set; }

    public byte PointFlg { get; set; }

    public decimal LastPoint { get; set; }

    public decimal WaitingPoint { get; set; }

    public decimal TotalPoint { get; set; }

    public decimal TotalPurchase { get; set; }

    public decimal UnpaidAmount { get; set; }

    public decimal UnpaidCount { get; set; }

    public DateOnly? LastSalesDate { get; set; }

    public string? LastSalesStoreCd { get; set; }

    public string? MainStoreCd { get; set; }

    public string? StaffCd { get; set; }

    public byte AttentionFlg { get; set; }

    public byte ConfirmFlg { get; set; }

    public string? ConfirmComment { get; set; }

    public byte BillingCloseDate { get; set; }

    public byte CollectPlanMonth { get; set; }

    public byte CollectPlanDate { get; set; }

    public byte HolidayKbn { get; set; }

    public byte TaxTiming { get; set; }

    public byte TaxPrintKbn { get; set; }

    public byte TaxFractionKbn { get; set; }

    public byte AmountFractionKbn { get; set; }

    public byte CreditLevel { get; set; }

    public decimal CreditCard { get; set; }

    public decimal CreditInsurance { get; set; }

    public decimal CreditDeposit { get; set; }

    public decimal CreditEtc { get; set; }

    public decimal CreditAmount { get; set; }

    public decimal CreditAdditionAmount { get; set; }

    public byte CreditCheckKbn { get; set; }

    public string? CreditMessage { get; set; }

    public decimal FareLevel { get; set; }

    public decimal Fare { get; set; }

    public string? PaymentMethodCd { get; set; }

    public string? KouzaCd { get; set; }

    public int DisplayOrder { get; set; }

    public byte PaymentUnit { get; set; }

    public byte NoInvoiceFlg { get; set; }

    public byte PrintNoAmountFlg { get; set; }

    public byte PrintNoDetailsFlg { get; set; }

    public byte NoInvoiceSendingFlg { get; set; }

    public byte? WebInvoiceKbn { get; set; }

    public string? CarrierCd { get; set; }

    public byte CountryKbn { get; set; }

    public string? CountryName { get; set; }

    public string? RegisteredNumber { get; set; }

    public byte Dmflg { get; set; }

    public string? RemarksOutStore { get; set; }

    public string? RemarksInStore { get; set; }

    public byte? StoreStampFlg { get; set; }

    public string? AnalyzeCd1 { get; set; }

    public string? AnalyzeCd2 { get; set; }

    public string? AnalyzeCd3 { get; set; }

    public byte DeleteFlg { get; set; }

    public byte UsedFlg { get; set; }

    public string? CustomerJancd { get; set; }

    public string? InsertOperator { get; set; }

    public DateTime? InsertDateTime { get; set; }

    public string? UpdateOperator { get; set; }

    public DateTime? UpdateDateTime { get; set; }
}
