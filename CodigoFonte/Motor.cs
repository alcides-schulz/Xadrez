using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enxadrista
{
    /// <summary>
    /// test
    /// </summary>
    public class Motor
    {
        public Tabuleiro Tabuleiro;
        public Avaliacao Avaliacao;
        public Pesquisa Pesquisa;

        public Motor(Transposicao transposicao)
        {
            Tabuleiro = new Tabuleiro();
            Avaliacao = new Avaliacao(Tabuleiro);
            Pesquisa = new Pesquisa(Tabuleiro, Avaliacao, transposicao);
        }

    }
}
