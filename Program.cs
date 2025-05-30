using Microsoft.EntityFrameworkCore;
using PrescriptionSystem.Data;
using PrescriptionSystem.Models;
using PrescriptionSystem.Services;

namespace PrescriptionSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Prescription Management System ===");
            Console.WriteLine("Initializing database...");

            using var context = new PrescriptionContext();
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Initialize services
            var patientService = new PatientService(context);
            var prescriptionService = new PrescriptionService(context);
            var otpService = new OTPService(context);
            var doctorService = new DoctorService(context);
            var pharmacyService = new PharmacyService(context);

            try
            {
                // Demo 1: Create Doctors
                Console.WriteLine("\n--- Creating Doctors ---");
                var doctor1 = await CreateSampleDoctor(doctorService, "Dr. John Smith", "Cardiology", "MD001", "0771234567");
                var doctor2 = await CreateSampleDoctor(doctorService, "Dr. Sarah Johnson", "Pediatrics", "MD002", "0772345678");
                
                // Demo 2: Create Pharmacies
                Console.WriteLine("\n--- Creating Pharmacies ---");
                var pharmacy1 = await CreateSamplePharmacy(pharmacyService, "MediPlus Pharmacy", "PH001", "0113456789");
                var pharmacy2 = await CreateSamplePharmacy(pharmacyService, "HealthCare Pharmacy", "PH002", "0114567890");

                // Demo 3: Create Patients
                Console.WriteLine("\n--- Creating Patients ---");
                var patient1 = await CreateSamplePatient(patientService, "Alice Johnson", "0777123456", 28);
                var patient2 = await CreateSamplePatient(patientService, "Bob Wilson", "0777234567", 45);

                // Demo 4: Create Prescriptions with Doctors
                Console.WriteLine("\n--- Creating Prescriptions ---");
                var prescription1 = await CreatePrescriptionWithDoctor(prescriptionService, patient1.PatientId, doctor1.DoctorId, "Lisinopril 10mg - Take once daily");
                var prescription2 = await CreatePrescriptionWithDoctor(prescriptionService, patient2.PatientId, doctor2.DoctorId, "Amoxicillin 500mg - Take twice daily");

                // Demo 5: Pharmacy Operations
                Console.WriteLine("\n--- Pharmacy Operations ---");
                await DemonstratePharmacyOperations(pharmacyService, prescription1.PrescriptionId, pharmacy1.PharmacyId);

                // Demo 6: Search and Reports
                Console.WriteLine("\n--- Search and Reports ---");
                await DemonstrateSearchAndReports(doctorService, pharmacyService, doctor1.DoctorId, pharmacy1.PharmacyId);

                // Demo 7: OTP Verification (existing functionality)
                Console.WriteLine("\n--- OTP Verification ---");
                await DemonstrateOTPVerification(otpService, patient1.PhoneNumber);

                Console.WriteLine("\n=== All demos completed successfully! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task<Doctor> CreateSampleDoctor(DoctorService doctorService, string name, string specialization, string license, string phone)
        {
            var doctor = new Doctor
            {
                FullName = name,
                Specialization = specialization,
                LicenseNumber = license,
                PhoneNumber = phone,
                Email = $"{name.Replace(" ", "").ToLower()}@hospital.com",
                ClinicAddress = "Main Hospital, Colombo"
            };

            var createdDoctor = await doctorService.AddDoctorAsync(doctor);
            Console.WriteLine($"✓ Doctor created: {createdDoctor.FullName} ({createdDoctor.Specialization})");
            return createdDoctor;
        }

        static async Task<Pharmacy> CreateSamplePharmacy(PharmacyService pharmacyService, string name, string license, string phone)
        {
            var pharmacy = new Pharmacy
            {
                PharmacyName = name,
                LicenseNumber = license,
                PhoneNumber = phone,
                Address = "Main Street, Colombo",
                City = "Colombo",
                PostalCode = "00100"
            };

            var createdPharmacy = await pharmacyService.AddPharmacyAsync(pharmacy);
            Console.WriteLine($"✓ Pharmacy created: {createdPharmacy.PharmacyName}");
            return createdPharmacy;
        }

        static async Task<Patient> CreateSamplePatient(PatientService patientService, string name, string phone, int age)
        {
            var patient = new Patient
            {
                FullName = name,
                PhoneNumber = phone,
                Age = age,
                Address = "Sample Address, Colombo"
            };

            var createdPatient = await patientService.AddPatientAsync(patient);
            Console.WriteLine($"✓ Patient created: {createdPatient.FullName} ({createdPatient.PhoneNumber})");
            return createdPatient;
        }

        static async Task<Prescription> CreatePrescriptionWithDoctor(PrescriptionService prescriptionService, int patientId, int doctorId, string medications)
        {
            var prescription = new Prescription
            {
                PatientId = patientId,
                DoctorId = doctorId,
                Medications = medications,
                Instructions = "Follow prescribed dosage",
                DateIssued = DateTime.Now
            };

            var createdPrescription = await prescriptionService.SavePrescriptionAsync(prescription);
            Console.WriteLine($"✓ Prescription created: ID {createdPrescription.PrescriptionId}");
            return createdPrescription;
        }

        static async Task DemonstratePharmacyOperations(PharmacyService pharmacyService, int prescriptionId, int pharmacyId)
        {
            // Search available prescriptions
            Console.WriteLine("Searching available prescriptions...");
            var availablePrescriptions = await pharmacyService.SearchAvailablePrescriptionsAsync();
            Console.WriteLine($"✓ Found {availablePrescriptions.Count} available prescriptions");

            // Dispense a prescription
            Console.WriteLine("Dispensing prescription...");
            var dispense = await pharmacyService.DispensePrescriptionAsync(
                prescriptionId, 
                pharmacyId, 
                "John Pharmacist", 
                25.50m, 
                "Dispensed successfully"
            );
            Console.WriteLine($"✓ Prescription dispensed: Dispense ID {dispense.DispenseId}, Amount: ${dispense.TotalAmount}");
        }

        static async Task DemonstrateSearchAndReports(DoctorService doctorService, PharmacyService pharmacyService, int doctorId, int pharmacyId)
        {
            // Get doctor's prescriptions
            Console.WriteLine("Getting doctor's prescription history...");
            var doctorPrescriptions = await doctorService.GetDoctorPrescriptionsAsync(doctorId);
            Console.WriteLine($"✓ Doctor has {doctorPrescriptions.Count} prescriptions");

            // Get pharmacy dispense history
            Console.WriteLine("Getting pharmacy dispense history...");
            var pharmacyHistory = await pharmacyService.GetPharmacyDispenseHistoryAsync(pharmacyId);
            Console.WriteLine($"✓ Pharmacy has {pharmacyHistory.Count} dispense records");

            foreach (var record in pharmacyHistory)
            {
                Console.WriteLine($"  - Patient: {record.Prescription.Patient.FullName}, Amount: ${record.TotalAmount}, Date: {record.DispensedAt:yyyy-MM-dd}");
            }
        }

        static async Task DemonstrateOTPVerification(OTPService otpService, string phoneNumber)
        {
            // Generate OTP
            string otp = await otpService.GenerateOTPAsync(phoneNumber);
            Console.WriteLine($"✓ OTP generated for {phoneNumber}: {otp}");

            // Verify OTP
            bool isValid = await otpService.VerifyOTPAsync(phoneNumber, otp);
            Console.WriteLine($"✓ OTP verification result: {(isValid ? "Valid" : "Invalid")}");
        }
    }
}