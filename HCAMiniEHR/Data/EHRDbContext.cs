using HCAMiniEHR.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HCAMiniEHR.Data
{
    public class EHRDbContext : DbContext
    {
        public EHRDbContext(DbContextOptions<EHRDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LabOrder>()
                .HasOne(l => l.Appointment)
                .WithMany(a => a.LabOrders)
                .HasForeignKey(l => l.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed 10 Patients
            var patients = new[]
            {
                new Patient { PatientId = 1, FirstName = "John", LastName = "Smith", DateOfBirth = new DateTime(1985, 5, 15), Gender = "Male", PhoneNumber = "555-0101", Email = "john.smith@email.com", Address = "123 Main St", CreatedDate = DateTime.Now.AddMonths(-6) },
                new Patient { PatientId = 2, FirstName = "Mary", LastName = "Johnson", DateOfBirth = new DateTime(1990, 8, 22), Gender = "Female", PhoneNumber = "555-0102", Email = "mary.j@email.com", Address = "456 Oak Ave", CreatedDate = DateTime.Now.AddMonths(-5) },
                new Patient { PatientId = 3, FirstName = "Robert", LastName = "Williams", DateOfBirth = new DateTime(1978, 3, 10), Gender = "Male", PhoneNumber = "555-0103", Email = "r.williams@email.com", Address = "789 Pine Rd", CreatedDate = DateTime.Now.AddMonths(-4) },
                new Patient { PatientId = 4, FirstName = "Patricia", LastName = "Brown", DateOfBirth = new DateTime(1995, 11, 5), Gender = "Female", PhoneNumber = "555-0104", Email = "pat.brown@email.com", Address = "321 Elm St", CreatedDate = DateTime.Now.AddMonths(-3) },
                new Patient { PatientId = 5, FirstName = "Michael", LastName = "Davis", DateOfBirth = new DateTime(1982, 7, 18), Gender = "Male", PhoneNumber = "555-0105", Email = "m.davis@email.com", Address = "654 Maple Dr", CreatedDate = DateTime.Now.AddMonths(-2) },
                new Patient { PatientId = 6, FirstName = "Jennifer", LastName = "Miller", DateOfBirth = new DateTime(1988, 2, 28), Gender = "Female", PhoneNumber = "555-0106", Email = "jen.miller@email.com", Address = "987 Cedar Ln", CreatedDate = DateTime.Now.AddMonths(-1) },
                new Patient { PatientId = 7, FirstName = "David", LastName = "Wilson", DateOfBirth = new DateTime(1992, 9, 12), Gender = "Male", PhoneNumber = "555-0107", Email = "d.wilson@email.com", Address = "147 Birch Ct", CreatedDate = DateTime.Now.AddDays(-20) },
                new Patient { PatientId = 8, FirstName = "Linda", LastName = "Moore", DateOfBirth = new DateTime(1975, 4, 25), Gender = "Female", PhoneNumber = "555-0108", Email = "linda.m@email.com", Address = "258 Spruce Way", CreatedDate = DateTime.Now.AddDays(-15) },
                new Patient { PatientId = 9, FirstName = "James", LastName = "Taylor", DateOfBirth = new DateTime(1998, 12, 8), Gender = "Male", PhoneNumber = "555-0109", Email = "james.t@email.com", Address = "369 Willow Pl", CreatedDate = DateTime.Now.AddDays(-10) },
                new Patient { PatientId = 10, FirstName = "Barbara", LastName = "Anderson", DateOfBirth = new DateTime(1987, 6, 30), Gender = "Female", PhoneNumber = "555-0110", Email = "barb.anderson@email.com", Address = "741 Ash Blvd", CreatedDate = DateTime.Now.AddDays(-5) }
            };
            modelBuilder.Entity<Patient>().HasData(patients);

            // Seed 5 Appointments
            var appointments = new[]
            {
                new Appointment { AppointmentId = 1, PatientId = 1, AppointmentDate = DateTime.Now.AddDays(-10), AppointmentType = "Checkup", DoctorName = "Dr. Sarah Connor", Status = "Completed", Notes = "Annual physical examination", CreatedDate = DateTime.Now.AddDays(-15) },
                new Appointment { AppointmentId = 2, PatientId = 2, AppointmentDate = DateTime.Now.AddDays(-5), AppointmentType = "Follow-up", DoctorName = "Dr. John McClane", Status = "Completed", Notes = "Blood pressure follow-up", CreatedDate = DateTime.Now.AddDays(-10) },
                new Appointment { AppointmentId = 3, PatientId = 3, AppointmentDate = DateTime.Now.AddDays(5), AppointmentType = "Checkup", DoctorName = "Dr. Sarah Connor", Status = "Scheduled", Notes = "Routine checkup", CreatedDate = DateTime.Now.AddDays(-2) },
                new Appointment { AppointmentId = 4, PatientId = 5, AppointmentDate = DateTime.Now.AddDays(-2), AppointmentType = "Emergency", DoctorName = "Dr. Ellen Ripley", Status = "Completed", Notes = "Chest pain evaluation", CreatedDate = DateTime.Now.AddDays(-3) },
                new Appointment { AppointmentId = 5, PatientId = 7, AppointmentDate = DateTime.Now.AddDays(10), AppointmentType = "Follow-up", DoctorName = "Dr. John McClane", Status = "Scheduled", Notes = "Lab results review", CreatedDate = DateTime.Now.AddDays(-1) }
            };
            modelBuilder.Entity<Appointment>().HasData(appointments);

            // Seed 5 Lab Orders
            var labOrders = new[]
            {
                new LabOrder { LabOrderId = 1, AppointmentId = 1, TestName = "Complete Blood Count (CBC)", Status = "Completed", OrderDate = DateTime.Now.AddDays(-10), CompletedDate = DateTime.Now.AddDays(-8), Results = "All values within normal range" },
                new LabOrder { LabOrderId = 2, AppointmentId = 1, TestName = "Lipid Panel", Status = "Completed", OrderDate = DateTime.Now.AddDays(-10), CompletedDate = DateTime.Now.AddDays(-8), Results = "Cholesterol slightly elevated" },
                new LabOrder { LabOrderId = 3, AppointmentId = 2, TestName = "Blood Glucose", Status = "Completed", OrderDate = DateTime.Now.AddDays(-5), CompletedDate = DateTime.Now.AddDays(-3), Results = "Normal fasting glucose" },
                new LabOrder { LabOrderId = 4, AppointmentId = 4, TestName = "Cardiac Enzymes", Status = "Pending", OrderDate = DateTime.Now.AddDays(-2), Notes = "Urgent - chest pain evaluation" },
                new LabOrder { LabOrderId = 5, AppointmentId = 5, TestName = "Thyroid Function (TSH)", Status = "Pending", OrderDate = DateTime.Now.AddDays(-1), Notes = "Follow-up from previous abnormal result" }
            };
            modelBuilder.Entity<LabOrder>().HasData(labOrders);
        }
    }
}