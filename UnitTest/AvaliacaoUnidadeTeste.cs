using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Enxadrista;

namespace UnitTest
{
    [TestClass]
    public class AvaliacaoUnidadeTeste
    {
        [TestMethod]
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
