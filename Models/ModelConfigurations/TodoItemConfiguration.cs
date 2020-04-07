using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TodoApi.Models.ModelConfigurations
{
    public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
    {
        public TodoItemConfiguration()
        {
        }

        public void Configure(EntityTypeBuilder<TodoItem> builder){
            builder.HasKey(prop => prop.Id);

            builder.Property(prop => prop.OnCreate)
                .HasColumnType("TIMEMAPS(0)")
                .IsRequired();

            builder.Property(prop => prop.Body);

            builder.Property(prop => prop.Title)
                .IsRequired();

            builder.Property(prop => prop.IsComplete)
                .IsRequired();

        }
        
    }
}