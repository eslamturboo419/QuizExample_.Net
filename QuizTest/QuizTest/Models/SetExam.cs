//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QuizTest.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SetExam
    {
        public int Id { get; set; }
        public string Exam_Name { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> Score { get; set; }
        public Nullable<int> Stu_Id { get; set; }
    
        public virtual Student Student { get; set; }
    }
}