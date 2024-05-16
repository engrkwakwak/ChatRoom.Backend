using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configuration {
    public class StatusConfiguration : IEntityTypeConfiguration<Status> {
        public void Configure(EntityTypeBuilder<Status> builder) {
            builder.HasData(
                new Status {
                    StatusId = 1,
                    StatusName = "Active"
                },
                new Status {
                    StatusId = 2,
                    StatusName = "Approved"
                },
                new Status {
                    StatusId = 3,
                    StatusName = "Deleted"
                });
        }
    }
}
