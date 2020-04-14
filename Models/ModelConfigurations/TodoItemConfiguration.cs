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
            builder.HasKey(prop => prop.id);
            
            builder.Property(prop => prop.UserId)
                .IsRequired();

            builder.Property(prop => prop.OnCreate)
                .IsRequired();

            builder.Property(prop => prop.Body);

            builder.Property(prop => prop.Title)
                .IsRequired();

        }
        
    }
}