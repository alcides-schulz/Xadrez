using FluentAssertions;
using NUnit.Framework;

namespace Enxadrista.Tipos
{
    public class PecaTeste
    {
        [Test]
        public void ParaIndice()
        {
            Peca.PeaoBranco.ParaIndice().Should().Be(0);
            Peca.CavaloBranco.ParaIndice().Should().Be(1);
            Peca.BispoBranco.ParaIndice().Should().Be(2);
            Peca.TorreBranca.ParaIndice().Should().Be(3);
            Peca.DamaBranca.ParaIndice().Should().Be(4);
            Peca.ReiBranco.ParaIndice().Should().Be(5);
            
            Peca.PeaoPreto.ParaIndice().Should().Be(6);
            Peca.CavaloPreto.ParaIndice().Should().Be(7);
            Peca.BispoPreto.ParaIndice().Should().Be(8);
            Peca.TorrePreta.ParaIndice().Should().Be(9);
            Peca.DamaPreta.ParaIndice().Should().Be(10);
            Peca.ReiPreto.ParaIndice().Should().Be(11);
        }

        [Test]
        public void ParaTipo()
        {
            Peca.PeaoBranco.ParaTipo().Should().Be(TipoPeca.Peao);
            Peca.CavaloBranco.ParaTipo().Should().Be(TipoPeca.Cavalo);
            Peca.BispoBranco.ParaTipo().Should().Be(TipoPeca.Bispo);
            Peca.TorreBranca.ParaTipo().Should().Be(TipoPeca.Torre);
            Peca.DamaBranca.ParaTipo().Should().Be(TipoPeca.Dama);
            Peca.ReiBranco.ParaTipo().Should().Be(TipoPeca.Rei);
            
            Peca.PeaoPreto.ParaTipo().Should().Be(TipoPeca.Peao);
            Peca.CavaloPreto.ParaTipo().Should().Be(TipoPeca.Cavalo);
            Peca.BispoPreto.ParaTipo().Should().Be(TipoPeca.Bispo);
            Peca.TorrePreta.ParaTipo().Should().Be(TipoPeca.Torre);
            Peca.DamaPreta.ParaTipo().Should().Be(TipoPeca.Dama);
            Peca.ReiPreto.ParaTipo().Should().Be(TipoPeca.Rei);
        }

        [Test]
        public void ParaTexto()
        {
            Peca.PeaoBranco.ParaTexto().Should().Be("P");
            Peca.CavaloBranco.ParaTexto().Should().Be("N");
            Peca.BispoBranco.ParaTexto().Should().Be("B");
            Peca.TorreBranca.ParaTexto().Should().Be("R");
            Peca.DamaBranca.ParaTexto().Should().Be("Q");
            Peca.ReiBranco.ParaTexto().Should().Be("K");
            
            Peca.PeaoPreto.ParaTexto().Should().Be("p");
            Peca.CavaloPreto.ParaTexto().Should().Be("n");
            Peca.BispoPreto.ParaTexto().Should().Be("b");
            Peca.TorrePreta.ParaTexto().Should().Be("r");
            Peca.DamaPreta.ParaTexto().Should().Be("q");
            Peca.ReiPreto.ParaTexto().Should().Be("k");
        }

        [Test]
        public void CaractereParaPeca()
        {
            'P'.ParaPeca().Should().Be(Peca.PeaoBranco);
            'N'.ParaPeca().Should().Be(Peca.CavaloBranco);
            'B'.ParaPeca().Should().Be(Peca.BispoBranco);
            'R'.ParaPeca().Should().Be(Peca.TorreBranca);
            'Q'.ParaPeca().Should().Be(Peca.DamaBranca);
            'K'.ParaPeca().Should().Be(Peca.ReiBranco);
            
            'p'.ParaPeca().Should().Be(Peca.PeaoPreto);
            'n'.ParaPeca().Should().Be(Peca.CavaloPreto);
            'b'.ParaPeca().Should().Be(Peca.BispoPreto);
            'r'.ParaPeca().Should().Be(Peca.TorrePreta);
            'q'.ParaPeca().Should().Be(Peca.DamaPreta);
            'k'.ParaPeca().Should().Be(Peca.ReiPreto);
        }
        
        [Test]
        public void ParaCor()
        {
            Peca.PeaoBranco.ParaCor().Should().Be(Cor.Branca);
            Peca.CavaloBranco.ParaCor().Should().Be(Cor.Branca);
            Peca.BispoBranco.ParaCor().Should().Be(Cor.Branca);
            Peca.TorreBranca.ParaCor().Should().Be(Cor.Branca);
            Peca.DamaBranca.ParaCor().Should().Be(Cor.Branca);
            Peca.ReiBranco.ParaCor().Should().Be(Cor.Branca);
            
            Peca.PeaoPreto.ParaCor().Should().Be(Cor.Preta);
            Peca.CavaloPreto.ParaCor().Should().Be(Cor.Preta);
            Peca.BispoPreto.ParaCor().Should().Be(Cor.Preta);
            Peca.TorrePreta.ParaCor().Should().Be(Cor.Preta);
            Peca.DamaPreta.ParaCor().Should().Be(Cor.Preta);
            Peca.ReiPreto.ParaCor().Should().Be(Cor.Preta);
        }
        
        [Test]
        public void InverteCor()
        {
            Peca.PeaoBranco.InverteCor().Should().Be(Peca.PeaoPreto);
            Peca.CavaloBranco.InverteCor().Should().Be(Peca.CavaloPreto);
            Peca.BispoBranco.InverteCor().Should().Be(Peca.BispoPreto);
            Peca.TorreBranca.InverteCor().Should().Be(Peca.TorrePreta);
            Peca.DamaBranca.InverteCor().Should().Be(Peca.DamaPreta);
            Peca.ReiBranco.InverteCor().Should().Be(Peca.ReiPreto);
            
            Peca.PeaoPreto.InverteCor().Should().Be(Peca.PeaoBranco);
            Peca.CavaloPreto.InverteCor().Should().Be(Peca.CavaloBranco);
            Peca.BispoPreto.InverteCor().Should().Be(Peca.BispoBranco);
            Peca.TorrePreta.InverteCor().Should().Be(Peca.TorreBranca);
            Peca.DamaPreta.InverteCor().Should().Be(Peca.DamaBranca);
            Peca.ReiPreto.InverteCor().Should().Be(Peca.ReiBranco);
        }
        
        [Test]
        public void TipoPecaParaPeca()
        {
            TipoPeca.Peao.ParaPeca(Cor.Branca).Should().Be(Peca.PeaoBranco);
            TipoPeca.Cavalo.ParaPeca(Cor.Branca).Should().Be(Peca.CavaloBranco);
            TipoPeca.Bispo.ParaPeca(Cor.Branca).Should().Be(Peca.BispoBranco);
            TipoPeca.Torre.ParaPeca(Cor.Branca).Should().Be(Peca.TorreBranca);
            TipoPeca.Dama.ParaPeca(Cor.Branca).Should().Be(Peca.DamaBranca);
            TipoPeca.Rei.ParaPeca(Cor.Branca).Should().Be(Peca.ReiBranco);
            
            TipoPeca.Peao.ParaPeca(Cor.Preta).Should().Be(Peca.PeaoPreto);
            TipoPeca.Cavalo.ParaPeca(Cor.Preta).Should().Be(Peca.CavaloPreto);
            TipoPeca.Bispo.ParaPeca(Cor.Preta).Should().Be(Peca.BispoPreto);
            TipoPeca.Torre.ParaPeca(Cor.Preta).Should().Be(Peca.TorrePreta);
            TipoPeca.Dama.ParaPeca(Cor.Preta).Should().Be(Peca.DamaPreta);
            TipoPeca.Rei.ParaPeca(Cor.Preta).Should().Be(Peca.ReiPreto);
        }
    }
}