using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Domain.ProcessEngine.Entities;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Domain.ProcessEngine.Enums;


namespace WorkflowEngine.Infrastructure.ProcessEngine.Presistence
{
    public class RepositoryDBContext : DbContext
    {
        public RepositoryDBContext(DbContextOptions options) : base(options)
        { }

        // Application
        public DbSet<Application> Applications { get; set; }
        
        // Module tables - Only concrete types
        public DbSet<ProcessModule> ProcessModules { get; set; }
        public DbSet<DatabaseActionModule> DatabaseActionModules { get; set; }
        public DbSet<DialogActionModule> DialogActionModules { get; set; }
        public DbSet<FieldModule> FieldModules { get; set; }
        public DbSet<CompareActionModule> CompareActionsModules { get; set; }
        public DbSet<CalculateActionModule> CalculateActionsModules { get; set; }
        public DbSet<ListModule> ListModules { get; set; }

        // Process module steps
        public DbSet<ProcessModuleDetail> ProcessModuleDetails { get; set; }
        // Calculate module steps
        public DbSet<CalculateModuleDetail> CalculateModuleDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // APPLICATIONS
            // =============================================
            modelBuilder.Entity<Application>(entity =>
            {
                entity.ToTable("t_applications");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Version)
                    .HasColumnName("version")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(e => e.VersionBuild)
                    .HasColumnName("version_build")
                    .HasMaxLength(10);

                entity.Property(e => e.ActivateOnStart)
                    .HasColumnName("activate_on_start")
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.LastCompiled)
                    .HasColumnName("last_compiled");

                entity.Property(e => e.LastActivated)
                    .HasColumnName("last_activated");

                entity.Property(e => e.CreatedDate)
                    .HasColumnName("created_date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnName("modified_date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                //entity.HasIndex(e => e.Name)
                //    .IsUnique()
                //    .HasDatabaseName("uq_application_name");
            });

            // =============================================
            // MODULE BASE TYPE - Configure TPC Strategy
            // =============================================
            modelBuilder.Entity<Module>(entity =>
            {
                // ✅ Tell EF Core to use TPC (Table Per Concrete Type) strategy
                entity.UseTpcMappingStrategy();
                
                // ✅ Configure the key on the BASE type
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ApplicationId)
                    .HasColumnName("application_id")
                    .IsRequired();

                entity.Property(e => e.ModuleType)
                    .HasColumnName("module_type")
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.Version)
                    .HasColumnName("version")
                    .IsRequired();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description");

                entity.Property(e => e.LockedBy)
                    .HasColumnName("locked_by")
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedDate)
                    .HasColumnName("created_date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnName("modified_date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // =============================================
            // PROCESS MODULE - Concrete Type
            // =============================================
            modelBuilder.Entity<ProcessModule>(entity =>
            {
                entity.ToTable("t_process_modules");


                // ✅ ONLY configure type-specific properties
                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasColumnType("text");

                // Relationship to ProcessModuleDetail
                entity.HasMany(e => e.Details)
                    .WithOne(d => d.ProcessModule)
                    .HasForeignKey(d => d.ProcessModuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                //// Your explicitly named index
                //entity.HasIndex(e => e.ApplicationId)
                //    .HasDatabaseName("idx_process_modules_application");

                //entity.HasIndex(e => new { e.ApplicationId, e.Name })
                //    .IsUnique()
                //    .HasDatabaseName("uq_process_module_name_per_application");
            });

            // =============================================
            // PROCESS MODULE DETAIL - STEPS
            // =============================================
            modelBuilder.Entity<ProcessModuleDetail>(entity =>
            {
                entity.ToTable("t_process_module_details");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ProcessModuleId)
                    .HasColumnName("process_module_id")
                    .IsRequired();

                entity.Property(e => e.Sequence)
                    .HasColumnName("sequence")
                    .IsRequired();

                entity.Property(e => e.LabelName)
                    .HasColumnName("label_name")
                    .HasMaxLength(255);

                entity.Property(e => e.ActionType)
                    .HasColumnName("action_type")
                    .HasConversion<int?>();

                entity.Property(e => e.ModuleId)
                    .HasColumnName("module_id");

                entity.Property(e => e.ActionModuleType)
                    .HasColumnName("action_module_type")
                    .HasConversion<int?>();

                entity.Property(e => e.PassLabel)
                    .HasColumnName("pass_label")
                    .HasMaxLength(255);

                entity.Property(e => e.FailLabel)
                    .HasColumnName("fail_label")
                    .HasMaxLength(255);

                entity.Property(e => e.CommentedFlag)
                    .HasColumnName("commented_flag")
                    .HasDefaultValue(false);

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasColumnType("text");

                //entity.HasIndex(e => e.ProcessModuleId)
                //    .HasDatabaseName("idx_process_module_details_module");

                //entity.HasIndex(e => new { e.ProcessModuleId, e.Sequence })
                //    .IsUnique()
                //    .HasDatabaseName("uq_process_module_detail_sequence");
            });

            // =============================================
            // DATABASE ACTION MODULE - Concrete Type
            // =============================================
            modelBuilder.Entity<DatabaseActionModule>(entity =>
            {
                entity.ToTable("t_database_action_modules");
                

                // ✅ ONLY configure type-specific properties
                entity.Property(e => e.SqlStatement)
                    .HasColumnName("sql_statement")
                    .HasColumnType("text")
                    .IsRequired();

                //entity.HasIndex(e => e.ApplicationId)
                //    .HasDatabaseName("idx_database_action_modules_application");

                //entity.HasIndex(e => new { e.ApplicationId, e.Name })
                //    .IsUnique()
                //    .HasDatabaseName("uq_database_action_module_name_per_application");
            });

            // =============================================
            // DIALOG ACTION MODULE - Concrete Type
            // =============================================
            modelBuilder.Entity<DialogActionModule>(entity =>
            {
                entity.ToTable("t_dialog_action_modules");

                // ✅ ONLY configure type-specific properties

                // Dialog behavior
                entity.Property(e => e.DialogType)
                    .HasColumnName("dialog_type")
                    .HasConversion<int>()
                    .IsRequired();

                // Result field
                entity.Property(e => e.ResultFieldId)
                    .HasColumnName("result_field_id");

                // Content fields
                entity.Property(e => e.MessageFieldId)
                    .HasColumnName("message_field_id");

                entity.Property(e => e.Help1FieldId)
                    .HasColumnName("help1_field_id");

                entity.Property(e => e.Help2FieldId)
                    .HasColumnName("help2_field_id");

                entity.Property(e => e.Help3FieldId)
                    .HasColumnName("help3_field_id");

                entity.Property(e => e.OptionsFieldId)
                    .HasColumnName("options_field_id");

                // List support
                entity.Property(e => e.ListModuleId)
                    .HasColumnName("list_module_id");

                // Input masking
                entity.Property(e => e.MaskInput)
                    .HasColumnName("mask_input")
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.MaskCharacter)
                    .HasColumnName("mask_character")
                    .HasMaxLength(1)
                    .IsRequired()
                    .HasDefaultValue("*");

                // Optional: Indexes (uncomment if needed)
                //entity.HasIndex(e => e.ApplicationId)
                //    .HasDatabaseName("idx_dialog_action_modules_application");

                //entity.HasIndex(e => e.ResultFieldId)
                //    .HasDatabaseName("idx_dialog_action_modules_result_field");

                //entity.HasIndex(e => new { e.ApplicationId, e.Name })
                //    .IsUnique()
                //    .HasDatabaseName("uq_dialog_action_module_name_per_application");
            });


            // =============================================
            // FIELD MODULE - Concrete Type
            // =============================================
            modelBuilder.Entity<FieldModule>(entity =>
            {
                entity.ToTable("t_field_modules");
                

                // ✅ ONLY configure type-specific properties
                entity.Property(e => e.FieldType)
                    .HasColumnName("field_type")
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.DefaultValue)
                    .HasColumnName("default_value")
                    .HasMaxLength(500);

                //entity.HasIndex(e => e.ApplicationId)
                //    .HasDatabaseName("idx_field_modules_application");

                //entity.HasIndex(e => new { e.ApplicationId, e.Name })
                //    .IsUnique()
                //    .HasDatabaseName("uq_field_module_name_per_application");
            });

            // =============================================
            // COMPARE ACTION MODULE - Concrete Type
            // =============================================
            modelBuilder.Entity<CompareActionModule>(entity =>
            {
                entity.ToTable("t_compare_action_modules");

                entity.Property(e => e.OperatorId)
                    .HasColumnName("operator_id")
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.Input1IsConstant)
                    .HasColumnName("input1_is_constant")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(e => e.Input1FieldId)
                    .HasColumnName("input1_field_id")
                    .HasDefaultValue(null);

                entity.Property(e => e.Input1Value)
                    .HasColumnName("input1_value")
                    .HasDefaultValue(string.Empty);

                entity.Property(e => e.Input2IsConstant)
                    .HasColumnName("input2_is_constant")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(e => e.Input2FieldId)
                    .HasColumnName("input2_field_id")
                    .HasDefaultValue(null);

                entity.Property(e => e.Input2Value)
                    .HasColumnName("input2_value")
                    .HasDefaultValue(string.Empty);
            });

            // =============================================
            // CALCULATE ACTION MODULE - Concrete Type
            // =============================================
            modelBuilder.Entity<CalculateActionModule>(entity =>
            {
                entity.ToTable("t_calculate_action_modules");

                // Relationship to ProcessModuleDetail
                entity.HasMany(e => e.Details)
                    .WithOne(d => d.CalculateActionModule)
                    .HasForeignKey(d => d.CalculateActionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =============================================
            // CALCULATE MODULE DETAIL - STEPS
            // =============================================

            modelBuilder.Entity<CalculateModuleDetail>(entity =>
            {
                entity.ToTable("t_calculate_module_details");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CalculateActionId)
                    .HasColumnName("calculate_action_id")
                    .IsRequired();

                entity.Property(e => e.Sequence)
                    .HasColumnName("sequence")
                    .IsRequired();

                entity.Property(e => e.OperatorId)
                    .HasColumnName("operator_id")
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.Input1IsConstant)
                    .HasColumnName("input1_is_constant")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(e => e.Input1FieldId)
                    .HasColumnName("input1_field_id")
                    .HasDefaultValue(null);

                entity.Property(e => e.Input1Value)
                    .HasColumnName("input1_value")
                    .HasDefaultValue(string.Empty);

                entity.Property(e => e.Input2IsConstant)
                    .HasColumnName("input2_is_constant")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(e => e.Input2FieldId)
                    .HasColumnName("input2_field_id")
                    .HasDefaultValue(null);

                entity.Property(e => e.Input2Value)
                    .HasColumnName("input2_value")
                    .HasDefaultValue(string.Empty);

                entity.Property(e => e.ResultFieldId)
                    .HasColumnName("result_field_id")
                    .IsRequired();

            });

            // =============================================
            // LIST MODULE - ✅ ADDED
            // =============================================
            modelBuilder.Entity<ListModule>(entity =>
            {
                entity.ToTable("t_list_modules");

                entity.Property(e => e.MaxRows)
                    .HasColumnName("max_rows");
            });
        }
    }
}

