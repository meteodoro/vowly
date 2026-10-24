using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vowly.Api.Models;

namespace Vowly.Api.Data;

public class AppDbContext : IdentityDbContext<Usuario>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Casamento> Casamentos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Compromisso> Compromissos { get; set; }
    public DbSet<Despesa> Despesas { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Parcela> Parcelas { get; set; }
    public DbSet<Tarefa> Tarefas { get; set; }
    public DbSet<Assinatura> Assinaturas { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Casamento)
            .WithOne(c => c.Usuario)
            .HasForeignKey<Casamento>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Casamento>()
            .Property(c => c.OrcamentoTotal)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Assinatura)
            .WithOne(a => a.Usuario)
            .HasForeignKey<Assinatura>(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Assinatura>()
            .Property(a => a.ValorMensal)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Despesa>()
            .HasMany(d => d.Parcelas)
            .WithOne(p => p.Despesa)
            .HasForeignKey(p => p.DespesaId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Despesa>()
            .Property(d => d.ValorTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Categoria>()
            .HasMany(c => c.Despesas)
            .WithOne(d => d.Categoria)
            .HasForeignKey(d => d.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Parcela>()
            .Property(p => p.Valor)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Fornecedor>()
            .HasMany(f => f.Despesas)
            .WithOne(d => d.Fornecedor)
            .HasForeignKey(d => d.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }

}