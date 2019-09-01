using Enxadrista;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTest
{
    public class CorTeste
    {
        [Test]
        public void Invertida()
        {
            Cor.Branca.Invertida().Should().Be(Cor.Preta);
            Cor.Preta.Invertida().Should().Be(Cor.Branca);
        }
        
        [Test]
        public void ParaTexto()
        {
            Cor.Branca.ParaTexto().Should().Be("w");
            Cor.Preta.ParaTexto().Should().Be("b");
            Cor.Nenhuma.ParaTexto().Should().Be("-");
        }
        
        [Test]
        public void ParaCor()
        {
            "w".ParaCor().Should().Be(Cor.Branca);
            "b".ParaCor().Should().Be(Cor.Preta);
            "-".ParaCor().Should().Be(Cor.Nenhuma);
        }
        
        [Test]
        public void Multiplicador()
        {
            Cor.Branca.Multiplicador().Should().Be(1);
            Cor.Preta.Multiplicador().Should().Be(-1);
        }
    }
}