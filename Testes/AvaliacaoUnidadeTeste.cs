using Enxadrista;
using NUnit.Framework;

namespace UnitTest
{
    public class AvaliacaoUnidadeTeste
    {
        [Test]
        public void Avaliacao_Fase()
        {
            Tabuleiro t = new Tabuleiro();
            t.NovaPartida(Defs.FEN_POSICAO_INICIAL);
            Avaliacao a = new Avaliacao(t);
            int pontos = a.ObtemPontuacao();
            Assert.AreEqual(0, a.Fase);
        }
    }
}
