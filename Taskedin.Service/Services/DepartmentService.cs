using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskedin.Domain.Entities;
using Taskedin.Domain.Models;
using Taskedin.Service.Interfaces;

namespace Taskedin.Service.Services
{
    public class DepartmentService : IDepartmentService
    {
        public DepartmentService() 
        {
            _departments.AddRange(
            new[] { new Department { Id = 1, Name = "IT", 
            CreatedAt = new DateTime(2026, 10, 1) },

            new Department { Id = 2, Name = "HR",
                CreatedAt = new DateTime(2026, 10, 3) } }
                                  );


          _profiles.AddRange(new[] { new Profile { Id = 1, Name = "Ahmed", DepartmentId = 1, 
              CreatedAt = new DateTime(2026, 10, 1) },

                new Profile { Id = 2, Name = "Mohamed", DepartmentId = 1, 
                    CreatedAt = new DateTime(2026, 10, 2) },

                new Profile { Id = 3, Name = "Ali", DepartmentId = 1, 
                    CreatedAt = new DateTime(2026, 10, 4) }, 

                new Profile { Id = 4, Name = "Omar", DepartmentId = 2,
                    CreatedAt = new DateTime(2026, 10, 2) }, 

                new Profile { Id = 5, Name = "Hassan", DepartmentId = 2, 
                    CreatedAt = new DateTime(2026, 10, 5) },

                new Profile { Id = 6, Name = "Mahmoud", DepartmentId = 2,
                    CreatedAt = new DateTime(2026, 10, 6) } }); 
        }
        private readonly List<Department> _departments = new();
        private readonly List<Profile> _profiles = new();

        public Task<Department> CreateAsync(Department department)
        {
            _departments.Add(department);

            return Task.FromResult(department);
        }

        public Task<Department?> GetByIdAsync(int id)
        {
            var department = _departments.FirstOrDefault(d => d.Id == id);

            return Task.FromResult(department);
        }

        public Task<bool> UpdateAsync(int id, Department department)
        {
            var existingDepartment = _departments.FirstOrDefault(d => d.Id == id);

            if (existingDepartment == null)
                return Task.FromResult(false);

            existingDepartment.Name = department.Name;
            existingDepartment.CreatedAt = department.CreatedAt;

            return Task.FromResult(true);
        }

        public Task<List<Profile>> GetProfilesAsync(GetProfilesRequest request)
        {
            var profiles = _profiles
                .Where(p =>
                    p.DepartmentId == request.DepartmentId &&
                    (!request.FromDate.HasValue || p.CreatedAt >= request.FromDate.Value) &&
                    (!request.ToDate.HasValue || p.CreatedAt <= request.ToDate.Value))
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Task.FromResult(profiles);
        }
    }
}