using System;
using System.Collections.Generic;

namespace TRS.Models;

public partial class VwHrEmployeeInfo
{
    public string EmployeeNo { get; set; }

    public string EmployeeName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string EmployeeStatus { get; set; }

    public string ServiceDescription { get; set; }

    public string EmployeeService { get; set; }

    public string PositionName { get; set; }

    public string PositionCode { get; set; }

    public string EmployeeTypeDesc { get; set; }

    public string EmployeeType { get; set; }

    public string JobClassDescription { get; set; }

    public string JobClassCode { get; set; }

    public string EmployeeLevel { get; set; }

    public string Branch { get; set; }

    public string Satellite { get; set; }

    public int? SuperiorId { get; set; }

    public string SuperiorFullname { get; set; }

    public string SectionId { get; set; }

    public string SectionName { get; set; }

    public int? SectionHeadId { get; set; }

    public string SectionHeadFullname { get; set; }

    public string DepartmentId { get; set; }

    public string DepartmentName { get; set; }

    public int? DepartmentHeadId { get; set; }

    public string DepartmentHeadFullname { get; set; }

    public string DivisionId { get; set; }

    public string DivisionName { get; set; }

    public int DivisionHeadId { get; set; }

    public string DivisionHeadFullname { get; set; }

    public string PersonalPhoneNo { get; set; }

    public string EmailAddress { get; set; }
}
