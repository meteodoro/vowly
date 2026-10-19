namespace Vowly.Api.Models;

public class Casamento
{
    public int Id { get; set; }
    public string NomeNoivo1 { get; set; } = string.Empty;
    public string NomeNoivo2 { get; set; } = string.Empty;
    public DateOnly DataPrevista { get; set; }
    public string? Cidade { get; set; }
    public decimal OrcamentoTotal { get; set; }
    
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario Usuario { get; set; }  = null!;
    
    public List<Categoria> Categorias { get; set; } = new();
    public List<Fornecedor> Fornecedores { get; set; } = new();
    public List<Despesa> Despesas { get; set; } = new();
    public List<Compromisso> Compromissos { get; set; } = new();
    public List<Tarefa> Tarefas { get; set; } = new();
    
}