using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Services;

namespace Vowly.Api.Tests;

public class PlanoRegrasTests
{
    private readonly DateTime _agoraUtc =
        new DateTime(2026, 10, 24, 20, 0, 0, DateTimeKind.Utc);
    
    [Theory]
    [InlineData(0, true)]
    [InlineData(19, true)]
    public void SemAssinatura_RespeitaLimite_PodeCadastrarDespesa(int quantidadeAtual, bool esperado)
    {
        //Arrange
        Assinatura? assinatura = null;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.Equal(esperado, resultado);
        
    }
    
    [Theory]
    [InlineData(20, false)]
    [InlineData(21, false)]
    [InlineData(100, false)]
    public void SemAssinatura_AcimaDoLimite_NaoPodeCadastrarDespesa(int quantidadeAtual, bool esperado)
    {
        //Arrange
        Assinatura? assinatura = null;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.Equal(esperado, resultado);
        
    }

    [Fact]
    public void PlanoPremiumAtivo_DataFimNoFuturo_PodeCadastrarDespesa()
    {
        //Arrange
        var assinatura = new Assinatura
        {
            Plano = TipoPlano.Premium,
            Status = StatusAssinatura.Ativa,
            DataFim = new DateTime(2027, 10, 24, 20, 0, 0, DateTimeKind.Utc)
        };
        const int quantidadeAtual = 50;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.True(resultado);
    }
    
    [Fact]
    public void PlanoPremiumCancelado_DataFimNoFuturo_PodeCadastrarDespesa()
    {
        //Arrange
        var assinatura = new Assinatura
        {
            Plano = TipoPlano.Premium,
            Status = StatusAssinatura.Cancelada,
            DataFim = new DateTime(2027, 10, 24, 20, 0, 0, DateTimeKind.Utc)
        };
        const int quantidadeAtual = 50;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.True(resultado);
    }
    
    [Fact]
    public void PlanoPremiumVencido_DataFimNoPassado_NaoPodeCadastrarDespesa()
    {
        //Arrange
        var assinatura = new Assinatura
        {
            Plano = TipoPlano.Premium,
            DataFim = new DateTime(2026, 08, 24, 20, 0, 0, DateTimeKind.Utc)
        };
        const int quantidadeAtual = 20;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.False(resultado);
    }
    
    [Fact]
    public void PlanoPremiumVencido_RespeitaLimite_PodeCadastrarDespesa()
    {
        //Arrange
        var assinatura = new Assinatura
        {
            Plano = TipoPlano.Premium,
            DataFim = new DateTime(2026, 08, 24, 20, 0, 0, DateTimeKind.Utc)
        };
        const int quantidadeAtual = 19;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.True(resultado);
    }
    
    [Fact]
    public void PlanoPremium_DataFimIgualDataAtual_NaoPodeCadastrarDespesa()
    {
        //Arrange
        var assinatura = new Assinatura
        {
            Plano = TipoPlano.Premium,
            DataFim = _agoraUtc
        };
        const int quantidadeAtual = 20;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.False(resultado);
    }
    
    [Fact]
    public void PlanoGratuito_AcimaDoLimite_NaoPodeCadastrarDespesa()
    {
        //Arrange
        var assinatura = new Assinatura
        {
            Plano = TipoPlano.Gratuito,
            DataFim = _agoraUtc
        };
        const int quantidadeAtual = 20;
        
        //Act
        var resultado = PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, _agoraUtc);
        
        //Assert
        Assert.False(resultado);
    }
}