namespace Vowly.Api.Models;

public class Tarefa
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateOnly? Prazo { get; set; }
    public bool Concluida { get; set; }
    
    public int CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;

}